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
        HttpListener Listener;

        public Server(int port)
        {
            IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(x => x.AddressFamily == AddressFamily.InterNetwork);

            Listener = new HttpListener();
            Listener.Prefixes.Add("http://localhost:" + port + "/replicated_log/");
            Listener.Start();
            
            Console.WriteLine("Server is running, prefixes: " + Listener.Prefixes);

            while (true)
            {
                HttpListenerContext context = Listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                
                Console.WriteLine("New client is connected");

                Console.WriteLine("Request processing...");
                Thread.Sleep(10000);

                byte[] buffer;
                string message = "";
            
                buffer = new byte[request.ContentLength64];
                request.InputStream.Read(buffer, 0, buffer.Length);
                message = Encoding.UTF8.GetString(buffer);

                Console.WriteLine("Message received: " + message);

                buffer = Encoding.UTF8.GetBytes("Message received");
                response.OutputStream.Write(buffer, 0, buffer.Length);

                response.OutputStream.Close();
            }
        }

        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
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
