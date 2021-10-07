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

        public Server(string host, int port)
        {
            m_messages = new List<string>();

            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://" + host + ":" + port + "/replicated_log/master/");
            m_listener.Start();

            Console.WriteLine("Server is running\n");

            while (true)
            {
                HttpListenerContext context = m_listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET")
                {
                    Console.WriteLine("GET request processing...");
                    Thread.Sleep(5000);

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

                    Console.WriteLine("GET request processed");
                }
                else if (request.HttpMethod == "POST")
                {
                    Console.WriteLine("POST request processing...");
                    Thread.Sleep(5000);

                    byte[] buffer = new byte[request.ContentLength64];
                    request.InputStream.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer);

                    Console.WriteLine("Message received: " + msg);

                    m_messages.Add(msg);

                    request.InputStream.Close();

                    Console.WriteLine("POST request processed");
                }

                Console.WriteLine();
                response.Close();
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
            string? _host = Environment.GetEnvironmentVariable("MASTER_HOST");
            string? _port = Environment.GetEnvironmentVariable("MASTER_PORT");

            string host = _host == null ? "localhost" : _host;
            int port = _port == null ? 2100 : int.Parse(_port);

            Console.WriteLine("Host: " + host);
            Console.WriteLine("Port: " + port);

            new Server(host, port);
        }
    }
}
