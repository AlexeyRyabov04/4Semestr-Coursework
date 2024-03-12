using System.Text.Json.Serialization;

namespace ChatLibrary
{
    [Serializable]
    public class ChatInfo
    {
        public string ChatName { get; } = string.Empty;
        public int ChatID { get; }
        public string ChatType { get; } = string.Empty;
        [JsonConstructor]
        public ChatInfo(string chatName, int chatID, string chatType)
        {
            ChatName = chatName;
            ChatID = chatID;
            ChatType = chatType;
        }
        public ChatInfo() { }
    }
}
