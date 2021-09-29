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
        TcpListener Listener;

        public Server(int port)
        {
            IPAddress ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(x => x.AddressFamily == AddressFamily.InterNetwork);

            Listener = new TcpListener(ipAddress, port);
            Listener.Start();
            
            Console.WriteLine("Server is running, IP address: " + Listener.Server.LocalEndPoint.ToString());

            while (true)
            {
                TcpClient client = Listener.AcceptTcpClient();

                Console.WriteLine("New client with IP {0} is connected", client.Client.RemoteEndPoint.ToString());

                while (client.Available == 0) ;

                byte[] buffer;
                string message = "";

                buffer = new byte[client.Available];
                client.GetStream().Read(buffer, 0, buffer.Length);
                message = Encoding.UTF8.GetString(buffer);

                Console.WriteLine("Message received: " + message);

                buffer = Encoding.UTF8.GetBytes("Message received");
                client.GetStream().Write(buffer, 0, buffer.Length);
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
