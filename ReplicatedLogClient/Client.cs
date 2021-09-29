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

        public Client(Uri endpoint)
        {
            m_httpClient = new HttpClient();
            m_httpClient.BaseAddress = endpoint;
        }

        ~Client()
        {
            if (this.m_httpClient != null)
            {
                this.m_httpClient.Dispose();
            }
        }

        public void sendMessage(String msg)
        {
            HttpContent content = new StringContent(msg);
            HttpResponseMessage response = m_httpClient.PostAsync(m_httpClient.BaseAddress, content).Result;
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                String respMsg = response.Content.ReadAsStringAsync().Result;
                MessageBox.Show(respMsg);
            }
        }
    }
}
