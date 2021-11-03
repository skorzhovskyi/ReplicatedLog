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
    class MessageIn
    {
        public string message { get; set; }
        public int w { get; set; }

        public MessageIn() { }
        public MessageIn(MessageIn val)
        {
            message = val.message;
            w = val.w;
        }

        public static MessageIn FromJson(string json)
        {
            return new MessageIn(JsonSerializer.Deserialize<MessageIn>(json));
        }

        public string GetJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }
    }

    class MessageOut
    {
        public string message { get; set; }

        public MessageOut() { }
        public MessageOut(string val)
        {
            message = val;
        }

        public static MessageOut FromJson(string json)
        {
            return new MessageOut(JsonSerializer.Deserialize<MessageOut>(json).message);
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

    class Server
    {
        List<string> m_messages;

        List<Uri> m_secondaries;

        HttpListener m_listener;

        MessageSender m_sender;

        public Server(string host, int port, List<Uri> secondaries, int broadCastingTimeOut)
        {
            m_secondaries = secondaries;

            m_messages = new List<string>();

            m_sender = new MessageSender(broadCastingTimeOut);

            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://" + host + ":" + port + "/");
            m_listener.Start();

            Console.WriteLine("Server is running\n");

            while (true)
            {
                Task<HttpListenerContext> task = m_listener.GetContextAsync();

                task.ContinueWith(_context =>
                {
                    HttpListenerContext context = _context.Result;
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
                        MessageIn msg = MessageIn.FromJson(Encoding.UTF8.GetString(buffer));

                        Console.WriteLine("Message received: " + msg.message);

                        m_messages.Add(msg.message);

                        request.InputStream.Close();

                        Broadcast(new MessageOut(msg.message).GetJson(), msg.w);

                        Console.WriteLine("POST request processed");
                    }

                    Console.WriteLine();
                    response.Close();
                });

                task.Wait();
            }
        }

        ~Server()
        {
            if (m_listener != null)
            {
                m_listener.Stop();
            }
        }

        private void Broadcast(string message, int w)
        {
            Console.WriteLine("Broadcasting message started");

            int requestsReceived = 0;
            int requestsFailed = 0;

            foreach (var s in m_secondaries)
            {
                Uri slave = s;
                var task = m_sender.SendMessageAsync(message, slave);

                task.ContinueWith(result =>
                {
                    try
                    {
                        if (result.Result)
                        {
                            Console.WriteLine("Slave " + slave.ToString() + " - received");
                            Interlocked.Increment(ref requestsReceived);
                        }
                        else
                        {
                            Console.WriteLine("Slave " + slave.ToString() + " - failed");
                            Interlocked.Increment(ref requestsFailed);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Slave " + slave.ToString() + " - failed");
                        Interlocked.Increment(ref requestsFailed);
                    }
                });
            }

            while (requestsReceived < w - 1 && requestsReceived + requestsFailed < m_secondaries.Count)
                Thread.Sleep(100);

            Console.WriteLine("Broadcasting finished");
            Console.WriteLine("{0} secondaries received the message", requestsReceived);
            Console.WriteLine("{0} secondaries failed to receive the message", requestsFailed);
            Console.WriteLine("{0} secondaries are in the progress of receiving the message", m_secondaries.Count - requestsFailed - requestsReceived);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string? _numOfSlaves = Environment.GetEnvironmentVariable("SECONDARIES_NUM");
            string? _host = Environment.GetEnvironmentVariable("MASTER_HOST");
            string? _port = Environment.GetEnvironmentVariable("MASTER_PORT");
            string? _broadCastingTimeOut = Environment.GetEnvironmentVariable("BROADCASTING_TIME_OUT");

            string host = _host == null ? "localhost" : _host;
            int port = _port == null ? 2100 : int.Parse(_port);
            int numOfSlaves = _numOfSlaves == null ? 2 : int.Parse(_numOfSlaves);
            int broadCastingTimeOut = _broadCastingTimeOut == null ? 20000 : int.Parse(_broadCastingTimeOut);

            Console.WriteLine("Host: " + host);
            Console.WriteLine("Port: " + port);

            List<Uri> secondaries = new List<Uri>();

            if (_host == null)
            {
                secondaries.Add(new Uri("http://localhost:2201"));
                secondaries.Add(new Uri("http://localhost:2202"));
            }
            else
            {
                for (int id = 1; id <= numOfSlaves; id++)
                {
                    string? slave_host = Environment.GetEnvironmentVariable("SECONDARY" + id + "_HOST");
                    string? slave_port = Environment.GetEnvironmentVariable("SECONDARY" + id + "_PORT");
                
                    if (slave_host == null || slave_port == null)
                        break;

                    Console.WriteLine("Slave host: " + slave_host);
                    Console.WriteLine("Slave port: " + slave_port);

                    secondaries.Add(new Uri("http://" + slave_host + ":" + slave_port));
                }
            }

            new Server(host, port, secondaries, broadCastingTimeOut);
        }
    }
}
