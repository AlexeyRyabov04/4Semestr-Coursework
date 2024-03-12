using ChatLibrary;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System.Collections.Concurrent;
using Org.BouncyCastle.Crypto.EC;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace Server
{
    public class Server
    {
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int port = 5555;
        private const string ip = "127.0.0.1";

        ConcurrentDictionary<string, Socket> connectedClients = new();
        ConcurrentDictionary<string, BigInteger> privateKeys = new();
        public delegate void TextMessageReceive(ChatMessage message);
        public event TextMessageReceive? TextMessageReceived;
        ConcurrentDictionary<Socket, byte[]> sessionKeys = new();
        ConcurrentBag<string> users = new();

        X9ECParameters curveParams;
        ECDomainParameters domainParams;
        string connectionString;
        int lastChatID = 0;
        bool isSend = true;
        object locker = new();
        public Server()
        {
            connectionString = ConfigurationManager.ConnectionStrings["ServerDatabase"].ConnectionString;
            curveParams = CustomNamedCurves.GetByName("secp256r1");
            domainParams = new ECDomainParameters(curveParams.Curve, curveParams.G, curveParams.N, curveParams.H);
            GetUsersList();
            lastChatID = GetLastChatID();
        }
        public async void StartServer()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            ServerSocket.Bind(ipPoint);
            ServerSocket.Listen(10);
            while (true)
            {
                var clientSocket = await ServerSocket.AcceptAsync();
                _ = HandleClient(clientSocket);
            }
        }

        private async Task HandleClient(Socket clientSocket)
        {
            while (CheckConnection(clientSocket))
            {
                var message = await ReceiveMessage(clientSocket);
                if (message != null)
                    HandleMessage(message, clientSocket);
            }
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        private int GetLastChatID()
        {
            SqlConnection sqlConnection = new(connectionString);
            int id = 0;
            sqlConnection.Open();
            string query = $"SELECT MAX(Id) FROM Chats";
            try
            {
                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    id = Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception) { }
            sqlConnection.Close();
            return id;
        }
        private void Registration(ChatMessage message, Socket socket)
        {
            ChatMessage.MessageType type = ChatMessage.MessageType.ACCEPT;
            int id = 0;
            string query = "SELECT * FROM Users WHERE LOWER(Name) = LOWER(@name)";
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                command.Parameters.AddWithValue("@name", message.Sender.ToLowerInvariant());
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    type = ChatMessage.MessageType.REJECT;
                }
                else
                {
                    sqlConnection.Close();
                    sqlConnection.Open();
                    string query2 = "INSERT INTO Users (Name, Password, ChatsID) VALUES (@name, @password, ''); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand insertCommand = new SqlCommand(query2, sqlConnection))
                    {
                        insertCommand.Parameters.AddWithValue("@name", message.Sender);
                        insertCommand.Parameters.AddWithValue("@password", Encoding.Default.GetString(message.Data ?? new byte[0]));
                        id = Convert.ToInt32(insertCommand.ExecuteScalar());
                    }
                }
            }
            SendMessage(new ChatMessage(type, "", BitConverter.GetBytes(id), message.Sender), socket);
            sqlConnection.Close();
        }

        private void Authentication(ChatMessage message, Socket socket)
        {
            ChatMessage.MessageType type = ChatMessage.MessageType.REJECT;
            int id = 0;
            if (!connectedClients.ContainsKey(message.Sender))
            {
                string query = "SELECT * FROM Users WHERE Name = @name AND Password = @password; SELECT ID FROM INSERTED;";
                SqlConnection sqlConnection = new(connectionString);
                sqlConnection.Open();
                using SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@name", message.Sender);
                command.Parameters.AddWithValue("@password", Encoding.Default.GetString(message.Data ?? new byte[0]));
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    id = Convert.ToInt32(reader["ID"]);
                    type = ChatMessage.MessageType.ACCEPT;
                }
                sqlConnection.Close();
            }
            SendMessage(new ChatMessage(type, "", BitConverter.GetBytes(id), message.Sender), socket);
        }

        private int AddChatToDB(string users, string type)
        {
            int id = 0;
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = "INSERT INTO Chats (Users, Type) VALUES (@users, @type); SELECT SCOPE_IDENTITY();";
            using (SqlCommand insertCommand = new SqlCommand(query, sqlConnection))
            {
                insertCommand.Parameters.AddWithValue("@users", users);
                insertCommand.Parameters.AddWithValue("@type", type);
                id = Convert.ToInt32(insertCommand.ExecuteScalar());
            }
            sqlConnection.Close();
            return id;
        }

        private void CreateChatHistory(int id)
        {
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = $"CREATE TABLE [Chat{id}Messages] (Id INT NOT NULL PRIMARY KEY IDENTITY, " +
                $"Sender NCHAR (15), Type NCHAR (10), Data NVARCHAR (MAX), Time NCHAR(30), FileName NVARCHAR(MAX))";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
                command.ExecuteNonQuery();
            sqlConnection.Close();
        }

        private void GetUsersList()
        {
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = "SELECT Name FROM Users";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(reader.GetString(0));
                    }
                }
            }
            sqlConnection.Close();
        }

        private void CreateNewChat(ChatMessage message, string type)
        {
            string str = Encoding.Default.GetString(message.Data ?? new byte[0]);
            int id = AddChatToDB(str, type);
            CreateChatHistory(id);
            string[] users = str.Split(',');
            foreach (string user in users)
            {
                if (connectedClients.ContainsKey(user))
                {
                    string chatName = str.Replace(",", ", ");
                    if (users.Length == 2)
                        chatName = chatName.Contains(user + ", ") ? chatName.Replace(user + ", ", "")
                            : chatName.Replace(", " + user, "");
                    string chatType = (type == "common") ? "Обычный" : "Секретный"; 
                    ChatInfo info = new(chatName, id, chatType);
                    if (chatType == "Секретный")
                    {
                        foreach (var client in users)
                            if (!connectedClients.ContainsKey(client))
                                return;
                    }
                    if (connectedClients.ContainsKey(user)) 
                        SendMessage(new ChatMessage(ChatMessage.MessageType.NEW_CHAT, "",
                            JsonSerializer.SerializeToUtf8Bytes(info), chatid: id), connectedClients[user]);
                }
            }
        }

        private List<string> GetChatUsersList(int chatID)
        {
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = "SELECT Users FROM Chats WHERE Id = @chatID";
            string users = "";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                command.Parameters.AddWithValue("@chatID", chatID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users = reader.GetString(0);
                    }
                }
            }
            sqlConnection.Close();
            return users.Split(',').ToList();
        }

        private int SaveMessageToDB(int chatID, string sender, string type, string data, string time, string fileName)
        {
            int id = 0;
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = $"INSERT INTO [Chat{chatID}Messages] (Sender, Type, Data, Time, FileName) VALUES " +
                $"(@sender, @type, @data, @time, @filename); SELECT SCOPE_IDENTITY();";
            using (SqlCommand insertCommand = new SqlCommand(query, sqlConnection))
            {
                insertCommand.Parameters.AddWithValue("@sender", sender);
                insertCommand.Parameters.AddWithValue("@type", type);
                insertCommand.Parameters.AddWithValue("@data", data);
                insertCommand.Parameters.AddWithValue("@time", time);
                insertCommand.Parameters.AddWithValue("@filename", fileName);
                id = Convert.ToInt32(insertCommand.ExecuteScalar());
            }
            sqlConnection.Close();
            return id;
        }

        private void SaveFileToStorage(int chatID, int id, byte[] fileBytes, string fileName)
        {
            string folder = "Files";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, $"{chatID}-{id}{Path.GetExtension(fileName)}");
            Task.Run(() => File.WriteAllBytes(filePath, fileBytes));
        }
        private void SendMessageToUsers(ChatMessage message)
        {
            
            List<string> list = GetChatUsersList(message.ChatID);
            string messageText = "";
            if (message.Type == ChatMessage.MessageType.TEXT)
            {
                messageText = BitConverter.ToString(message.Data ?? new byte[0]).Replace("-", "");
            }
            string type = message.Type == ChatMessage.MessageType.TEXT ? "Text" : "File";
            int id = SaveMessageToDB(message.ChatID, message.Sender, type, messageText, message.Time, message.FileName);
            if (message.Type == ChatMessage.MessageType.FILE_SEND)
            {
                SaveFileToStorage(message.ChatID, id, message.Data ?? new byte[0], message.FileName);
                message.Data = null;
            }
            foreach (var user in list)
            {
                if (connectedClients.ContainsKey(user))
                {
                    var userSocket = connectedClients[user];
                    SendMessage(new ChatMessage(message.Type, message.Sender, message.Data
                        , chatid: message.ChatID, id: id, time: message.Time, fileName: message.FileName), userSocket);
                }
            }
        }

        private List<ChatInfo> GetUsersChat(string name)
        {
            List<ChatInfo> chatsInfo = new List<ChatInfo>();
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = "SELECT * FROM Chats WHERE Users LIKE '%' + @searchString1 + '%'" +
                "OR Users LIKE @searchString2 + '%' OR Users LIKE '%' + @searchString3";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                command.Parameters.AddWithValue("@searchString1", "," + name + ",");
                command.Parameters.AddWithValue("@searchString2", name + ",");
                command.Parameters.AddWithValue("@searchString3", "," + name);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string type = reader.GetString(reader.GetOrdinal("Type"));
                        string str = reader.GetString(reader.GetOrdinal("Users"));
                        type = type.Contains("common") ? "Обычный" : "Секретный";
                        string[] users = str.Split(',');
                        string chatName = str.Replace(",", ", ");
                        if (users.Length == 2)
                            chatName = chatName.Contains(name + ", ") ? chatName.Replace(name + ", ", "")
                                : chatName.Replace(", " + name, "");
                        chatsInfo.Add(new ChatInfo(chatName, id, type));
                    }
                }
            }
            sqlConnection.Close();
            return chatsInfo;
        }

        private void SendChatsToUser(string user, Socket socket)
        {
            List<ChatInfo> chatsInfo = GetUsersChat(user);
            SendMessage(new ChatMessage(ChatMessage.MessageType.CONNECT, "", 
                JsonSerializer.SerializeToUtf8Bytes(chatsInfo)), socket);

        }

        private List<ChatMessage> GetChatHistory(int id)
        {
            List<ChatMessage> history = new();
            SqlConnection sqlConnection = new(connectionString);
            sqlConnection.Open();
            string query = $"SELECT * FROM Chat{id}Messages";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string sender = reader.GetString(reader.GetOrdinal("Sender")).Trim();
                        string data = reader.GetString(reader.GetOrdinal("Data")).Trim();
                        string time = reader.GetString(reader.GetOrdinal("Time"));
                        string type = reader.GetString(reader.GetOrdinal("Type"));
                        int messageID = reader.GetInt32(reader.GetOrdinal("Id"));
                        string fileName = reader.GetString(reader.GetOrdinal("FileName")).Trim();
                        ChatMessage.MessageType messageType = type.Contains("Text") ?
                            ChatMessage.MessageType.TEXT : ChatMessage.MessageType.FILE_SEND;
                        byte[]? messageData = new byte[data.Length / 2];
                        for (int i = 0; i < messageData.Length; i++)
                        {
                            string hexByte = data.Substring(i * 2, 2);
                            messageData[i] = Convert.ToByte(hexByte, 16);
                        }
                        if (messageType == ChatMessage.MessageType.FILE_SEND)
                            messageData = null;
                        history.Add(new ChatMessage(messageType, sender, messageData
                            , id: messageID, time: time, fileName: fileName));
                    }
                }
            }
            sqlConnection.Close();
            return history;
        }
        private void SendHistory(Socket socket, int chatID)
        {
            List<ChatMessage> history = GetChatHistory(chatID);
            SendMessage(new ChatMessage(ChatMessage.MessageType.CHAT_HISTORY, ""
                , JsonSerializer.SerializeToUtf8Bytes(history), chatid: chatID), socket);
        }
        private byte[] GetFileFromStorage(int id, int chatID, string fileName)
        {
            string folder = "Files";
            string file = Path.Combine(folder, $"{chatID}-{id}{Path.GetExtension(fileName)}");
            return File.ReadAllBytes(file);
        }
        private void SendFileToUser(int id, int chatID, string fileName, Socket socket)
        {
            byte[] fileBytes = GetFileFromStorage(id, chatID, fileName);
            SendMessage(new ChatMessage(ChatMessage.MessageType.FILE_REQUEST, "", fileBytes, fileName: fileName, chatid: chatID), socket);
        }

        private void HandleMessage(ChatMessage message, Socket socket)
        {
            switch (message.Type)
            {
                case ChatMessage.MessageType.FILE_SEND:
                    SendMessageToUsers(message);
                    break;
                case ChatMessage.MessageType.FILE_REQUEST:
                    SendFileToUser(message.ID, message.ChatID
                        , Encoding.Default.GetString(message.Data ?? new byte[0]), socket);
                    break;
                case ChatMessage.MessageType.KEY_EXCHANGE:
                    if (message.Receiver == "")
                        ShareKey(message, socket);
                    else
                    {
                        lock (locker)
                        {
                            if (isSend)
                            {
                                lastChatID++;
                                isSend = false;
                            }
                            else
                                isSend = true;
                        }
                        if (connectedClients.ContainsKey(message.Receiver))
                            SendMessage(new ChatMessage(message.Type, message.Sender, message.Data
                                , message.Receiver, lastChatID), connectedClients[message.Receiver]);
                    }
                    break;
                case ChatMessage.MessageType.LOGIN:
                    Authentication(message, socket);
                    break;
                case ChatMessage.MessageType.SIGNUP:
                    Registration(message, socket);
                    break;
                case ChatMessage.MessageType.CONNECT:
                    connectedClients.TryAdd(message.Sender, socket);
                    SendChatsToUser(message.Sender, socket);
                    break;
                case ChatMessage.MessageType.DISCONNECT:
                    connectedClients.TryRemove(message.Sender, out _);
                    sessionKeys.TryRemove(socket, out _);
                    break;
                case ChatMessage.MessageType.NEW_CHAT:
                    lock (locker)
                        lastChatID++;
                    CreateNewChat(message, "common");
                    break;
                case ChatMessage.MessageType.NEW_SECRET_CHAT:
                    CreateNewChat(message, "secret");
                    break;
                case ChatMessage.MessageType.ALL_LIST:
                    SendMessage(new ChatMessage(ChatMessage.MessageType.ALL_LIST, "",
                        JsonSerializer.SerializeToUtf8Bytes(users.ToList())), socket);
                    break;
                case ChatMessage.MessageType.CONNECTED_LIST:
                    SendMessage(new ChatMessage(ChatMessage.MessageType.CONNECTED_LIST, "",
                        JsonSerializer.SerializeToUtf8Bytes(connectedClients.Keys.ToList())), socket);
                    break;
                case ChatMessage.MessageType.TEXT:
                    SendMessageToUsers(message);
                    TextMessageReceived?.Invoke(message);
                    break;
                case ChatMessage.MessageType.CHAT_HISTORY:
                    SendHistory(socket, message.ChatID);
                    break;
            }
        }
        private void ShareKey(ChatMessage message, Socket socket)
        {
            BigInteger privateKey = GenerateRandomKey();
            var sessionKey = GenerateGeneralKey(message, privateKey);
            ECPoint? key = domainParams.G.Multiply(privateKey);
            SendMessage(new ChatMessage(ChatMessage.MessageType.KEY_EXCHANGE, "", key.GetEncoded(),
                message.Sender), socket);
            sessionKeys.TryAdd(socket, sessionKey);
        }

        private byte[] GenerateGeneralKey(ChatMessage message, BigInteger? privateKey)
        {
            var publicKey = curveParams.Curve.DecodePoint(message.Data);
            var secretKey = publicKey.Multiply(privateKey);
            var xBytes = secretKey?.Normalize().XCoord.GetEncoded() ?? new byte[0];
            var yBytes = secretKey?.Normalize().YCoord.GetEncoded() ?? new byte[0];
            privateKeys.TryRemove(message.Sender, out _);
            return xBytes.Take(16)
               .Concat(yBytes.Take(16))
               .Select(b => b.ToString("X2"))
               .Select(s => byte.Parse(s, System.Globalization.NumberStyles.HexNumber))
               .ToArray();
        }

        private BigInteger GenerateRandomKey()
        {
            byte[] privateKeyBytes = new byte[32];
            Random random = new Random();
            random.NextBytes(privateKeyBytes);
            return new BigInteger(1, privateKeyBytes);
        }

        public void SendMessage(ChatMessage message, Socket socket)
        {
            byte[] messageBytes = JsonSerializer.SerializeToUtf8Bytes<ChatMessage>(message);
            if (sessionKeys.ContainsKey(socket))
                messageBytes = AES.Encrypt(messageBytes, sessionKeys[socket]);
            int objSize = messageBytes.Length;
            byte[] sizeBytes = BitConverter.GetBytes(objSize);
            socket.Send(sizeBytes);
            socket.Send(messageBytes);
        }
        private async Task<ChatMessage?> ReceiveMessage(Socket socket)
        {
            byte[] sizeBytes = new byte[4];
            await socket.ReceiveAsync(sizeBytes);
            int objSize = BitConverter.ToInt32(sizeBytes, 0);
            byte[] obj = new byte[objSize];
            int bytesReceived = 0;
            const int chunkSize = 1024 * 1024;
            while (bytesReceived < objSize)
            {
                int remaining = objSize - bytesReceived;
                int size = remaining < chunkSize ? remaining : chunkSize;
                byte[] chunk = new byte[size];
                int bytesRead = await socket.ReceiveAsync(chunk);
                Buffer.BlockCopy(chunk, 0, obj, bytesReceived, bytesRead);
                bytesReceived += bytesRead;
            }
            if (sessionKeys.ContainsKey(socket))
                obj = AES.Decrypt(obj, sessionKeys[socket]);
            return JsonSerializer.Deserialize<ChatMessage>(obj);
        }
        private bool CheckConnection(Socket socket)
        {
            bool part1 = socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }
}
