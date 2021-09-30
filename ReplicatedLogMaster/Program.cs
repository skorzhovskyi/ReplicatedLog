using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ReplicatedLogMaster
{
    class Server
    {
        List<string> m_messages;

        HttpListener m_listener;

        public Server(int port)
        {
            m_messages = new List<string>();

            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://localhost:" + port + "/replicated_log/master/");
            m_listener.Start();

            Console.WriteLine("Server is running, prefixes: ");

            foreach (var pref in m_listener.Prefixes)
                Console.WriteLine("\t" + pref);

            while (true)
            {
                HttpListenerContext context = m_listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                
                Console.WriteLine("\nNew client is connected");

                if (request.HttpMethod == "GET")
                {
                    Console.WriteLine("Request processing...");
                    Thread.Sleep(10000);

                    byte[] buffer = new byte[GetMessagesSize()];

                    int pos = 0;

                    foreach (var msg in m_messages)
                    {
                        Array.Copy(BitConverter.GetBytes(msg.Length), 0, buffer, pos, sizeof(int));
                        pos += sizeof(int);
                        Array.Copy(Encoding.ASCII.GetBytes(msg), 0, buffer, pos, msg.Length);
                        pos += msg.Length;
                    }

                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
                else if (request.HttpMethod == "POST")
                {
                    byte[] buffer = new byte[request.ContentLength64];
                    request.InputStream.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer);

                    Console.WriteLine("Message received: " + msg);

                    m_messages.Add(msg);

                    request.InputStream.Close();
                }

                response.Close();

                Console.WriteLine("");
            }
        }

        ~Server()
        {
            if (m_listener != null)
            {
                m_listener.Stop();
            }
        }


        private int GetMessagesSize()
        {
            int size = sizeof(int) * m_messages.Count;

            foreach (var msg in m_messages)
                size += msg.Length;

            return size;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new Server(2100);
        }
    }
}
