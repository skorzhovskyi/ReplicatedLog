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
        const int MAX_RETRY_DELAY = 60000;
        List<string> m_messages;

        List<Uri> m_secondaries;
        List<SecondaryHealth> m_secondariesStatus;

        HttpListener m_listener;

        MessageSender m_sender;

        int m_retryDelay;
        int m_quorum;

        public Server(string host, int port, int retryTimeout, List<Uri> secondaries, int broadCastingTimeOut, int retryDelay, int pingDelay, int quorum)
        {
            m_retryDelay = retryDelay;
            m_quorum = quorum;

            m_secondaries = secondaries;
            m_secondariesStatus = new();

            foreach (var s in m_secondaries)
                m_secondariesStatus.Add(SecondaryHealth.Undefined);

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

                            if (Broadcast(new MessageOut(msg.message, msgId).GetJson(), msgId, msg.w, retryTimeout))
                                AddMessage(msg.message);
                            else
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

        private void Ping(Uri uri, int secondaryId)
        {
            var task = m_sender.GetAsync(uri);

            task.ContinueWith(result =>
            {
                try
                {
                    if (result.Result)
                    {
                        if (m_secondariesStatus[secondaryId] != SecondaryHealth.Healthy)
                            Console.WriteLine("Slave " + uri.ToString() + " - available");
                        m_secondariesStatus[secondaryId] = SecondaryHealth.Healthy;
                        return;
                    }
                }
                catch (Exception)
                {
                }

                if (m_secondariesStatus[secondaryId] != SecondaryHealth.Unhealthy)
                    Console.WriteLine("Slave " + uri.ToString() + " - unavailable");

                m_secondariesStatus[secondaryId] = SecondaryHealth.Unhealthy;
            });
        }

        private void Ping()
        {
            for (int i = 0; i < m_secondaries.Count; i++)
            {
                Uri uri = new Uri(m_secondaries[i], "health");
                Ping(uri, i);
            }
        }

        private void SendMessage(string message, int id, Uri uri, CountdownEvent cde, CancellationToken ct, System.Timers.Timer timer, int retryDelay, bool retry = false)
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

                Console.WriteLine("Slave " + uri.ToString() + " - failed");

                if (retry)
                {
                    if (timer.Enabled)
                        Task.Delay(retryDelay, ct).Wait(ct);

                    if (timer.Enabled)
                    {
                        Console.WriteLine("Retry slave " + uri.ToString());
                        SendMessage(message, id, uri, cde, ct, timer, Math.Min(retryDelay * 2, MAX_RETRY_DELAY), retry);
                    }
                }

            });
        }

        private bool Broadcast(string message, int id, int w, int retryTimeout)
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
                SendMessage(message, id, s, cde, cts.Token, timer, m_retryDelay, true);

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

            return cde.IsSet;
        }
    }
}
