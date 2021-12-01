using System.Collections.Generic;
using System.Text.Json;

namespace ReplicatedLogMaster
{
    class MessagesOut
    {
        public List<string> messages { get; set; }
        public List<int> ids { get; set; }

        public MessagesOut() 
        {
            messages = new List<string>();
            ids = new List<int>();
        }
        public MessagesOut(List<MessagesOut> msgs)
        {
            messages = new List<string>();
            ids = new List<int>();

            foreach (var msg in msgs)
                Append(msg);
        }

        public MessagesOut(string val, int id)
        {
            messages = new List<string>();
            ids = new List<int>();

            messages.Add(val);
            ids.Add(id);            
        }

        public void Append(MessagesOut msgs)
        {
            messages.AddRange(msgs.messages);
            ids.AddRange(msgs.ids);
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
