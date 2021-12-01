using System.Collections.Generic;
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

    class MessagesOut
    {
        public List<string> messages { get; set; }
        public List<int> ids { get; set; }

        public MessagesOut() { }
        public MessagesOut(List<MessageOut> msgs)
        {
            messages = new List<string>();
            ids = new List<int>();

            foreach(var msg in msgs)
            {
                messages.Add(msg.message);
                ids.Add(msg.id);
            }
        }

        public static MessagesOut FromJson(string json)
        {
            return JsonSerializer.Deserialize<MessagesOut>(json);
        }

        public string GetJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }
    }
}
