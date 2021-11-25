﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace ReplicatedLogMaster
{
    class Server
    {
        ConcurrentBag<string> m_messages;

        List<Uri> m_secondaries;
        List<bool> m_secondariesStatus;

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
                m_secondariesStatus.Add(true);

            m_messages = new ConcurrentBag<string>();

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

                        string json = (new Messages(m_messages.ToList())).GetJson();

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

                            m_messages.Add(msg.message);

                            request.InputStream.Close();

                            int msgId = m_messages.Count;

                            Broadcast(new MessageOut(msg.message, msgId).GetJson(), msgId, msg.w, retryTimeout);

                            Console.WriteLine("POST request processed");
                        }
                        else
                            Console.WriteLine("No quorum");
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

        bool IsQuorum()
        {
            return m_secondariesStatus.Count(x => x) >= m_quorum;
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
                        if (!m_secondariesStatus[secondaryId])
                            Console.WriteLine("Slave " + uri.ToString() + " - available");
                        m_secondariesStatus[secondaryId] = true;
                        return;
                    }
                }
                catch (Exception)
                {
                }

                if (m_secondariesStatus[secondaryId])
                    Console.WriteLine("Slave " + uri.ToString() + " - unavailable");

                m_secondariesStatus[secondaryId] = false;
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
                        Console.WriteLine("Retry in " + m_retryDelay + " sec...");
                        Thread.Sleep(m_retryDelay);
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
}