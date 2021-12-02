using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace ReplicatedLogMaster
{
    enum SecondaryHealth
    {
        Undefined,
        Unhealthy,
        Healthy
    }

    class Server
    {
        List<string> m_messages;

        List<Uri> m_secondaries;
        List<SecondaryHealth> m_secondariesStatus;

        Dictionary<Uri, List<MessagesOut>> m_retryQueue;

        HttpListener m_listener;

        MessageSender m_sender;

        int m_quorum;
        int m_batchSize;

        public Server(string host, int port, int retryTimeout, List<Uri> secondaries, int broadCastingTimeOut, int retryDelay, int pingDelay, int quorum, int batchSize)
        {
            m_retryQueue = new Dictionary<Uri, List<MessagesOut>>();

            m_batchSize = batchSize;
            m_quorum = quorum;

            m_secondaries = secondaries;
            m_secondariesStatus = new();

            foreach (var s in m_secondaries)
            {
                m_secondariesStatus.Add(SecondaryHealth.Undefined);
                m_retryQueue[s] = new List<MessagesOut>();
            }

            m_messages = new List<string>();

            m_sender = new MessageSender(broadCastingTimeOut);

            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://" + host + ":" + port + "/");
            m_listener.Start();

            Console.WriteLine("Server is running\n");

            var pingTask = new Task(() =>
            {
                while (true)
                {
                    Ping();
                    Thread.Sleep(pingDelay);
                }
            });

            pingTask.Start();

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

                        string json = new Messages(m_messages).GetJson();

                        var buffer = Encoding.ASCII.GetBytes(json);

                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();

                        Console.WriteLine("GET request processed");
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        Console.WriteLine("POST request processing...");

                        if (IsQuorum())
                        {
                            byte[] buffer = new byte[request.ContentLength64];
                            request.InputStream.Read(buffer, 0, buffer.Length);
                            MessageIn msg = MessageIn.FromJson(Encoding.UTF8.GetString(buffer));

                            Console.WriteLine("Message received: " + msg.message);

                            request.InputStream.Close();

                            int msgId = m_messages.Count + 1;

                            AddMessage(msg.message);

                            if (!Broadcast(new MessagesOut(msg.message, msgId), msg.w))
                            {
                                Console.WriteLine("Concern parameter is not satisfied");
                                PostStatus(response, HttpStatusCode.NotModified, "Concern parameter is not satisfied");
                            }

                            Console.WriteLine("POST request processed");
                        }
                        else
                        {
                            Console.WriteLine("No quorum");
                            PostStatus(response, HttpStatusCode.NotModified, "No quorum");
                        }
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

        private void AddMessage(string msg)
        {
            var someObj = new object();
            lock (someObj)
                m_messages.Add(msg);
        }

        private void PostStatus(HttpListenerResponse responese, HttpStatusCode statusCode, string msg)
        {
            responese.StatusCode = (int)statusCode;

            byte[] buffer = Encoding.UTF8.GetBytes("{\"msg\":\"" + msg +"\"}");

            responese.ContentType = "Application/json";
            responese.ContentLength64 = buffer.Length;

            System.IO.Stream output = responese.OutputStream;
            output.Write(buffer, 0, buffer.Length);
        }

        private bool IsQuorum()
        {
            return m_secondariesStatus.Count(x => x == SecondaryHealth.Healthy) >= m_quorum;
        }

        private void EmptyRetryQueue(Uri uri)
        {
            while(m_retryQueue[uri].Count != 0)
            {
                var batch = new MessagesOut();

                var batchSize = Math.Min(m_batchSize, m_retryQueue[uri].Count);

                for (int i = 0; i < batchSize; i++)
                    batch.Append(m_retryQueue[uri][i]);

                if (SendMessages(batch, uri))
                {
                    Console.WriteLine("Slave " + uri.ToString() + " - received a batch of size " + batchSize);
                    m_retryQueue[uri].RemoveRange(0, batchSize);
                }
                else
                    break;
            }
        }

        private void Ping(Uri uri, int secondaryId)
        {
            Uri uriHealth = new Uri(uri, "health");
            var task = m_sender.GetAsync(uriHealth);

            task.ContinueWith(result =>
            {
                try
                {
                    if (result.Result)
                    {
                        if (m_secondariesStatus[secondaryId] != SecondaryHealth.Healthy)
                            Console.WriteLine("Slave " + uriHealth.ToString() + " - available");

                        m_secondariesStatus[secondaryId] = SecondaryHealth.Healthy;

                        EmptyRetryQueue(uri);

                        return;
                    }
                }
                catch (Exception)
                {
                }

                if (m_secondariesStatus[secondaryId] != SecondaryHealth.Unhealthy)
                    Console.WriteLine("Slave " + uriHealth.ToString() + " - unavailable");

                m_secondariesStatus[secondaryId] = SecondaryHealth.Unhealthy;
            });
        }

        private void Ping()
        {
            for (int i = 0; i < m_secondaries.Count; i++)
            {
                Uri uri = m_secondaries[i];
                Ping(uri, i);
            }
        }
        
        private bool SendMessages(MessagesOut msgs, Uri uri)
        {
            return m_sender.SendMessage(msgs.GetJson(), uri);
        }

        private void SendMessageAsync(MessagesOut msg, Uri uri, CountdownEvent cdeConcern, CountdownEvent cdeTotal)
        {
            var task = m_sender.SendMessageAsync(msg.GetJson(), uri);

            task.ContinueWith(result =>
            {
                try
                {
                    if (result.Result)
                    {
                        Console.WriteLine("Slave " + uri.ToString() + " - received");

                        if (!cdeConcern.IsSet)
                            cdeConcern.Signal();

                        cdeTotal.Signal();

                        if (cdeTotal.IsSet)
                            cdeConcern.Reset(0);

                        return;
                    }
                }
                catch (Exception)
                {
                }                               

                Console.WriteLine("Slave " + uri.ToString() + " - failed");

                m_retryQueue[uri].Add(msg);

                cdeTotal.Signal();

                if (cdeTotal.IsSet)
                    cdeConcern.Reset(0);
            });
        }

        private bool Broadcast(MessagesOut msg, int w)
        {        
            Console.WriteLine("Broadcasting message started");

            var cdeConcern = new CountdownEvent(w - 1);
            var cdeTotal = new CountdownEvent(m_secondaries.Count);

            foreach (var s in m_secondaries)
                SendMessageAsync(msg, s, cdeConcern, cdeTotal);

            cdeConcern.Wait();

            Console.WriteLine("Broadcasting finished");

            return cdeConcern.IsSet;
        }
    }
}
