using System.Text.Json;

namespace ReplicatedLogMaster
{
    class MessageIn
    {
        public string message { get; set; }
        public int w { get; set; }

        public MessageIn() { }
        public MessageIn(MessageIn val)
        {
            message = val.message;
            w = val.w;
        }

        public static MessageIn FromJson(string json)
        {
            return new MessageIn(JsonSerializer.Deserialize<MessageIn>(json));
        }

        public string GetJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }
    }
}
