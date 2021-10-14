using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Http;
using System.Text.Json;

namespace ReplicatedLogMaster
{
    class Message
    {
        public string message { get; set; }

        public Message() { }
        public Message(string val)
        {
            message = val;             
        }

        public static Message FromJson(string json)
        {
            return new Message(JsonSerializer.Deserialize<Message>(json).message);
        }

        public string GetJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }
    }

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

    class Client
    {
        private HttpClient m_httpClient;

        public HttpClient HttpClient
        {
            get => m_httpClient;
        }

        public Client()
        {
            m_httpClient = new HttpClient();
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

        public bool SendMessage(string msg, Uri uri)
        {
            HttpContent content = new StringContent(msg, Encoding.UTF8, "application/json");
            HttpResponseMessage response = m_httpClient.PostAsync(uri, content).Result;

            return response.StatusCode == HttpStatusCode.OK;
        }
    }

    class Server
    {
        List<string> m_messages;

        List<Uri> m_secondaries;

        HttpListener m_listener;

        Client m_client;

        public Server(string host, int port, List<Uri> secondaries)
        {
            m_secondaries = secondaries;

            m_messages = new List<string>();

            m_client = new Client();

            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://" + host + ":" + port + "/");
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

                    string json = (new Messages(m_messages)).GetJson();

                    var buffer = Encoding.ASCII.GetBytes(json);

                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();

                    Console.WriteLine("GET request processed");
                }
                else if (request.HttpMethod == "POST")
                {
                    Console.WriteLine("POST request processing...");

                    byte[] buffer = new byte[request.ContentLength64];
                    request.InputStream.Read(buffer, 0, buffer.Length);
                    string msg = Message.FromJson(Encoding.UTF8.GetString(buffer)).message;

                    Console.WriteLine("Message received: " + msg);

                    m_messages.Add(msg);

                    request.InputStream.Close();

                    Broadcast(new Message(msg).GetJson());

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

        private void Broadcast(string message)
        {
            Console.WriteLine("Broadcasting message started");

            foreach(var slave in m_secondaries)
            {
                if (m_client.SendMessage(message, slave))
                    Console.WriteLine("Slave " + slave.ToString() + " - received");
                else
                    Console.WriteLine("Slave " + slave.ToString() + " - failed");
            }

            Console.WriteLine("Broadcasting message finished");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string? _numOfSlaves = Environment.GetEnvironmentVariable("SLAVES_NUM");
            string? _host = Environment.GetEnvironmentVariable("MASTER_HOST");
            string? _port = Environment.GetEnvironmentVariable("MASTER_PORT");

            string host = _host == null ? "localhost" : _host;
            int port = _port == null ? 2100 : int.Parse(_port);
            int numOfSlaves = _numOfSlaves == null ? 2 : int.Parse(_numOfSlaves);

            Console.WriteLine("Host: " + host);
            Console.WriteLine("Port: " + port);

            List<Uri> secondaries = new List<Uri>();

            if (_host == null)
                secondaries.Add(new Uri("http://localhost:2200"));
            else
            {
                for (int id = 0; id < numOfSlaves; id++)
                {
                    string? slave_host = Environment.GetEnvironmentVariable("SLAVE" + id + "_HOST");
                    string? slave_port = Environment.GetEnvironmentVariable("SLAVE" + id + "_PORT");
                
                    if (slave_host == null || slave_port == null)
                        break;
                
                    Console.WriteLine("Slave host: " + slave_host);
                    Console.WriteLine("Slave port: " + slave_port);
                
                    secondaries.Add(new Uri("http://" + slave_host + ":" + slave_port));
                
                    ++id;
                }
            }

            new Server(host, port, secondaries);
        }
    }
}
