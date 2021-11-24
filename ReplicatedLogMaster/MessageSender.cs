using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace ReplicatedLogMaster
{
    class MessageSender
    {
        private HttpClient m_httpClient;

        public HttpClient HttpClient
        {
            get => m_httpClient;
        }

        public MessageSender(int postTimeOut)
        {
            m_httpClient = new HttpClient();
            m_httpClient.Timeout = TimeSpan.FromMilliseconds(postTimeOut);
        }

        public MessageSender(Uri endpoint, int postTimeOut)
        {
            m_httpClient = new HttpClient();
            m_httpClient.BaseAddress = endpoint;
            m_httpClient.Timeout = TimeSpan.FromMilliseconds(postTimeOut);
        }

        ~MessageSender()
        {
            if (m_httpClient != null)
                m_httpClient.Dispose();
        }

        public bool SendMessage(string msg, Uri uri)
        {
            HttpContent content = new StringContent(msg, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = m_httpClient.PostAsync(uri, content).Result;
                return response != null && response.StatusCode == HttpStatusCode.OK;
            }
            catch (AggregateException)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> SendMessageAsync(string msg, Uri uri)
        {
            HttpContent content = new StringContent(msg, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await m_httpClient.PostAsync(uri, content);
            return response != null && response.StatusCode == HttpStatusCode.OK;
        }
    }
}
