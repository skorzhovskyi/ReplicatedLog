using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace ReplicatedLogClient
{
    class Client 
    {
        private HttpClient m_httpClient;

        public HttpClient HttpClient
        {
            get => m_httpClient;
        }

        public Client(Uri endpoint)
        {
            m_httpClient = new HttpClient();
            m_httpClient.BaseAddress = endpoint;
        }

        ~Client()
        {
            if (m_httpClient != null)
                m_httpClient.Dispose();            
        }

        public void SendMessage(String msg)
        {
            HttpContent content = new StringContent(msg);
            HttpResponseMessage response = m_httpClient.PostAsync(m_httpClient.BaseAddress, content).Result;
            
            if (response.StatusCode == HttpStatusCode.OK)
            {

            }            
        }

        public List<string> GetMessages()
        {
            HttpResponseMessage response = m_httpClient.GetAsync(m_httpClient.BaseAddress).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<string> result = new List<string>();

                var buffer = response.Content.ReadAsByteArrayAsync().Result;

                int pos = 0;

                while (pos < buffer.Length)
                {
                    int size = BitConverter.ToInt32(buffer, pos);
                    pos += sizeof(int);

                    result.Add(Encoding.ASCII.GetString(buffer, pos, size));

                    pos += size;
                }

                return result;
            }

            return null;
        }
    }
}
