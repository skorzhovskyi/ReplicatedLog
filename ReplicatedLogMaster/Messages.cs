using System.Collections.Generic;
using System.Text.Json;

namespace ReplicatedLogMaster
{
    class Messages
    {
        public List<string> messages { get; set; }
        public Messages() { }
        public Messages(List<string> val)
        {
            messages = val;
        }

        public static Messages FromJson(string json)
        {
            return new Messages(JsonSerializer.Deserialize<Messages>(json).messages);
        }

        public string GetJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }
    }
}
