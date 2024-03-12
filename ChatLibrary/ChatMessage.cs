namespace ChatLibrary
{
    [Serializable]
    public class ChatMessage
    {
        public enum MessageType { TEXT, FILE_SEND, LOGIN, KEY_EXCHANGE, NEW_CHAT, ALL_LIST, CONNECTED_LIST,
            CONNECT, DISCONNECT, FILE_REQUEST, SIGNUP, ACCEPT, REJECT, NEW_SECRET_CHAT, CHAT_HISTORY }
        public MessageType Type { get; }
        public string Sender { get; }
        public byte[]? Data { get; set; }
        public string Receiver { get; }
        public int ChatID { get; }
        public int ID { get; }
        public string FileName { get; }
        public string Time { get; }

        public ChatMessage(MessageType type, string sender, byte[]? data, string receiver = "", int chatid = 0, 
            int id = 0, string fileName = "", string time = "")
            => (Type, Sender, Data, ChatID, FileName, ID, Receiver, Time) = 
            (type, sender, data, chatid, fileName, id, receiver, time);
    }
}