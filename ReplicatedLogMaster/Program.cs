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
using System.Collections.Concurrent;

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
        private const int RETRY_DELAY = 5;

        ConcurrentBag<string> m_messages;

        List<Uri> m_secondaries;

        HttpListener m_listener;

        MessageSender m_sender;

        public Server(string host, int port, int retryTimeout, List<Uri> secondaries, int broadCastingTimeOut)
        {
            m_secondaries = secondaries;

            m_messages = new ConcurrentBag<string>();

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

                        string json = (new Messages(m_messages.ToList())).GetJson();

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

                        int msgId = m_messages.Count;

                        Broadcast(new MessageOut(msg.message, msgId).GetJson(), msgId, msg.w, retryTimeout);

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

        private void SendMessage(string message, int id, Uri uri, CountdownEvent cde, System.Timers.Timer timer, bool retry = false)
        {
            var task = m_sender.SendMessageAsync(message, uri);

            task.ContinueWith(result =>
            {
                try
                {
                    if (result.Result)
                    {
                        Console.WriteLine("Slave " + uri.ToString() + " - received");

                        if (!cde.IsSet)
                            cde.Signal();

                        return;
                    }
                }
                catch (Exception)
                {
                }

                if (!retry || timer.Enabled)
                    Console.WriteLine("Slave " + uri.ToString() + " - failed");

                if (retry)
                {
                    if (timer.Enabled)
                    {
                        Console.WriteLine("Retry in " + RETRY_DELAY + " sec...");
                        Thread.Sleep(RETRY_DELAY * 1000);
                    }

                    if (timer.Enabled)
                        SendMessage(message, id, uri, cde, timer, retry);
                }

            });
        }

        private void Broadcast(string message, int id, int w, int retryTimeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            System.Timers.Timer timer = new(retryTimeout)
            {
                AutoReset = false
            };

            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                cts.Cancel();
            };

            timer.Start();

            Console.WriteLine("Broadcasting message started");

            var cde = new CountdownEvent(w - 1);

            foreach (var s in m_secondaries)
                SendMessage(message, id, s, cde, timer, true);

            try
            {
                cde.Wait(cts.Token);
            }
            catch (OperationCanceledException oce)
            {
                if (oce.CancellationToken == cts.Token)
                {
                    Console.WriteLine("Retry timeout");
                }
                else
                {
                    throw; 
                }
            }
            finally
            {
                cde.Dispose();
                cts.Dispose();
            }

            Console.WriteLine("Broadcasting finished");
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
            string? _retryTimeout = Environment.GetEnvironmentVariable("RETRY_TIME_OUT");

            string host = _host == null ? "localhost" : _host;
            int port = _port == null ? 2100 : int.Parse(_port);
            int numOfSlaves = _numOfSlaves == null ? 2 : int.Parse(_numOfSlaves);
            int broadCastingTimeOut = _broadCastingTimeOut == null ? 20000 : int.Parse(_broadCastingTimeOut) * 1000;
            int retryTimeout = _retryTimeout == null || int.Parse(_retryTimeout) == -1 ? 3600000 : int.Parse(_retryTimeout) * 1000;

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

            new Server(host, port, retryTimeout, secondaries, broadCastingTimeOut);
        }
    }
}
