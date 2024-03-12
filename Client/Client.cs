using ChatLibrary;
using System.Net.Sockets;
using System.Text.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System.Collections.Concurrent;
using System.Net;
using Org.BouncyCastle.Crypto.EC;
using System.Text;

namespace Client
{
    public class Client
    {
        public Socket socket { get; } = new Socket(AddressFamily.InterNetwork
            , SocketType.Stream, ProtocolType.Tcp);
        public int ID { get; set; }
        public string Name { get; set; } = "";

        public delegate void MessageReceive(ChatMessage message);
        public event MessageReceive? MessageReceived;
        public delegate void ChatsListReceive(List<ChatInfo> chatsInfo);
        public event ChatsListReceive? ChatsListReceived;

        ConcurrentDictionary<string, BigInteger> privateKeys = new();
        private byte[] sessionKey = new byte[0];
        ConcurrentDictionary<int, byte[]> secretKeys = new();
        X9ECParameters curveParams;
        ECDomainParameters domainParams;
        public List<string> usersList = new();

        public Client()
        {
            curveParams = CustomNamedCurves.GetByName("secp256r1");
            domainParams = new ECDomainParameters(curveParams.Curve, curveParams.G, curveParams.N, curveParams.H);
        }
        public void ConnectToServer(string ip, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(ipPoint);
        }

        public void GenerateSessionKey()
        {
            SendDHParams();
            Task.Run(async () =>
            {
                var message = await ReceiveMessage();
                if (message != null)
                    sessionKey = GenerateGeneralKey(message, privateKeys[message.Sender]);
            });
        }
        private void GetSecretKeys()
        {
            string folder = $"{Name}SecretKeys";
            if (Directory.Exists(folder))
            {
                var files = Directory.GetFiles(folder);
                foreach (string file in files)
                {
                    int chatID = Convert.ToInt32(Path.GetFileNameWithoutExtension(file));
                    string key = File.ReadAllText(file);
                    byte[] bytes = new byte[key.Length / 2];
                    for (int i = 0; i < key.Length; i += 2)
                    {
                        string hexByte = key.Substring(i, 2);
                        bytes[i / 2] = Convert.ToByte(hexByte, 16);
                    }
                    if (bytes.Length == 32)
                        secretKeys.TryAdd(chatID, bytes);
                }
            }
        }

        public void RequestChatHistory(int chatID)
        {
            SendMessage(new ChatMessage(ChatMessage.MessageType.CHAT_HISTORY, Name, null, chatid: chatID));
        }

        private async void StartCommunication(ChatMessage message)
        {
            Name = message.Receiver;
            ID = BitConverter.ToInt32(message.Data);
            GetSecretKeys();
            SendMessage(new ChatMessage(ChatMessage.MessageType.CONNECT, Name, null));
            var mess = await ReceiveMessage();
            var list = JsonSerializer.Deserialize<List<ChatInfo>>(mess?.Data);
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!secretKeys.ContainsKey(list[i].ChatID) && list[i].ChatType == "Секретный")
                    {
                        list.Remove(list[i]);
                        i--;
                    }
                }
                ChatsListReceived?.Invoke(list);
            }
            CommunicateWithServer();
        }
        public void GetUsersList(ChatMessage.MessageType type)
        {
            usersList.Clear();
            SendMessage(new ChatMessage(type, Name, null));
        }
        public async Task<bool> Authorization(string name, string password, ChatMessage.MessageType type)
        {
            SendMessage(new ChatMessage(type, name, Encoding.Default.GetBytes(password)));
            var message = await ReceiveMessage();
            bool isAccess = false;
            if (message?.Type == ChatMessage.MessageType.ACCEPT)
            {
                isAccess = true;
                StartCommunication(message);
            }
            return isAccess;
        }
        private void SendDHParams(string receiver = "")
        {
            X9ECParameters curveParams = CustomNamedCurves.GetByName("secp256r1");
            ECDomainParameters domainParams = new ECDomainParameters(curveParams.Curve, curveParams.G, curveParams.N, curveParams.H);
            BigInteger privateKey = GenerateRandomKey();
            ECPoint publicKey = domainParams.G.Multiply(privateKey);
            privateKeys.TryAdd(receiver, privateKey);
            SendMessage(new ChatMessage(ChatMessage.MessageType.KEY_EXCHANGE, Name, publicKey.GetEncoded(), 
                receiver));
        }

        private BigInteger GenerateRandomKey()
        {
            byte[] privateKeyBytes = new byte[32];
            Random random = new Random();
            random.NextBytes(privateKeyBytes);
            return new BigInteger(1, privateKeyBytes);
        }
        
        public void SendFileRequest(int chatID, int id, string filePath)
        {
            SendMessage(new ChatMessage(ChatMessage.MessageType.FILE_REQUEST, ""
                , Encoding.Default.GetBytes(filePath), chatid: chatID, id: id));
        }
        public void SendMessageToUsers(byte[] data, string time, bool isSecret, int chatID, string fileName = "")
        {
            ChatMessage.MessageType type = ChatMessage.MessageType.TEXT;
            if (fileName != "")
                type = ChatMessage.MessageType.FILE_SEND;
            if (isSecret)
                data = AES.Encrypt(data, secretKeys[chatID]);
            SendMessage(new ChatMessage(type, Name, data
                , chatid: chatID, time: time, fileName: fileName));
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

        public void SendMessage(ChatMessage message)
        {
            byte[] messageBytes = JsonSerializer.SerializeToUtf8Bytes<ChatMessage>(message);
            if (sessionKey.Length != 0)
                messageBytes = AES.Encrypt(messageBytes, sessionKey);
            int objSize = messageBytes.Length;
            byte[] sizeBytes = BitConverter.GetBytes(objSize);
            socket.Send(sizeBytes);
            socket.Send(messageBytes);
        }
        private async Task<ChatMessage?> ReceiveMessage()
        {
            byte[] sizeBytes = new byte[4];
            await socket.ReceiveAsync(sizeBytes, SocketFlags.None);
            int objSize = BitConverter.ToInt32(sizeBytes, 0);
            byte[] obj = new byte[objSize];
            int bytesReceived = 0;
            const int chunkSize = 1024 * 1024;
            while (bytesReceived < objSize)
            {
                int remaining = objSize - bytesReceived;
                int size = remaining < chunkSize ? remaining : chunkSize;
                byte[] chunk = new byte[size];
                int bytesRead = await socket.ReceiveAsync(chunk, SocketFlags.None);
                Buffer.BlockCopy(chunk, 0, obj, bytesReceived, bytesRead);
                bytesReceived += bytesRead;
            }
            if (sessionKey.Length != 0)
                obj = AES.Decrypt(obj, sessionKey);
            return JsonSerializer.Deserialize<ChatMessage>(obj);
        }

            private bool CheckConnection()
        {
            bool part1 = socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public void NewChat(string users, ChatMessage.MessageType type, string receiver)
        {
            if (type == ChatMessage.MessageType.NEW_SECRET_CHAT)
                SendDHParams(receiver);
            SendMessage(new ChatMessage(type, Name, Encoding.Default.GetBytes(users)));
        }
        public void Disconnect()
        {
            if (socket != null)
            {
                try
                {
                    SendMessage(new ChatMessage(ChatMessage.MessageType.DISCONNECT, Name, null));
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception) { }
            }
        }
        private void ShareKey(ChatMessage message)
        {
            BigInteger privateKey = GenerateRandomKey();
            secretKeys.TryAdd(message.ChatID, GenerateGeneralKey(message, privateKey));
            ECPoint? publicKey = domainParams.G.Multiply(privateKey);
            SendMessage(new ChatMessage(ChatMessage.MessageType.KEY_EXCHANGE, Name, publicKey.GetEncoded(),
                message.Sender));
        }

        private void SaveSecretKey(byte[] key, int id)
        {
            string folder = $"{Name}SecretKeys";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, $"{id}.txt");
            string keyString = "";
            foreach (var keyByte in key)
                keyString += keyByte.ToString("X2");
            File.WriteAllText(filePath, keyString);

        }

        private void HandleMessage(ChatMessage message)
        {
            switch (message.Type)
            {
                case ChatMessage.MessageType.KEY_EXCHANGE:
                    if (privateKeys.ContainsKey(message.Sender)) 
                        secretKeys.TryAdd(message.ChatID, GenerateGeneralKey(message
                            , privateKeys[message.Sender]));
                    else
                        ShareKey(message);
                    SaveSecretKey(secretKeys[message.ChatID], message.ChatID);
                    break;
                case ChatMessage.MessageType.ALL_LIST:
                    usersList = JsonSerializer.Deserialize<List<string>>(message?.Data) ?? new();
                    break;
                case ChatMessage.MessageType.CONNECTED_LIST:
                    usersList = JsonSerializer.Deserialize<List<string>>(message?.Data) ?? new();
                    break;
                case ChatMessage.MessageType.TEXT:
                    if (secretKeys.ContainsKey(message.ChatID))
                    {
                        message.Data = AES.Decrypt(message.Data ?? new byte[0]
                            , secretKeys[message.ChatID]);
                    }
                    break;
                case ChatMessage.MessageType.FILE_REQUEST:
                    if (secretKeys.ContainsKey(message.ChatID))
                    {
                        message.Data = AES.Decrypt(message.Data ?? new byte[0]
                            , secretKeys[message.ChatID]);
                    }
                    break;
                case ChatMessage.MessageType.CHAT_HISTORY:
                    if (secretKeys.ContainsKey(message.ChatID))
                    {
                        List<ChatMessage>? list = JsonSerializer.Deserialize<List<ChatMessage>>(message.Data);
                        if (list != null) {
                            for (int i = 0; i < list.Count; i++)
                            {
                                ChatMessage item = list[i];
                                if (item.Data != null && item?.Data.Length != 0 && item != null)
                                {
                                    item.Data = AES.Decrypt(item.Data ?? new byte[0], secretKeys[message.ChatID]);
                                    list[i] = item;
                                    message.Data = JsonSerializer.SerializeToUtf8Bytes(list);
                                }
                            }
                        }
                    }
                    break;
            }
        }
        public void CommunicateWithServer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (socket != null && CheckConnection())
                        {
                            var message = await ReceiveMessage();
                            if (message != null)
                            {
                                HandleMessage(message);
                                MessageReceived?.Invoke(message);
                            }
                        }
                        else
                            break;
                    }
                    catch (Exception) { }
                }
            });
        }
    }
}
