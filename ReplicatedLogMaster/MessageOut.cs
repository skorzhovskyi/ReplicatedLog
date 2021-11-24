using System.Text.Json;

namespace ReplicatedLogMaster
{
    class MessageOut
    {
        public string message { get; set; }
        public int id { get; set; }

        public MessageOut() { }
        public MessageOut(string val, int _id)
        {
            message = val;
            id = _id;
        }

        public static MessageOut FromJson(string json)
        {
            return JsonSerializer.Deserialize<MessageOut>(json);
        }

        public string GetJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }
    }
}
