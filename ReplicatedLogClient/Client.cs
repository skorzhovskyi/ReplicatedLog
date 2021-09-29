using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReplicatedLogClient
{
    class Client
    {
        private TcpClient socket;

        public Client(IPEndPoint endpoint)
        {
            this.socket = new TcpClient();
            socket.Connect(endpoint);
        }

        ~Client()
        {
            if (this.socket != null)
            {
                this.socket.Close();
            }
        }

        public void sendMessage(String msg)
        {
            Thread.Sleep(10000);
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            this.socket.GetStream().Write(buffer, 0, buffer.Length);
            Thread receiveThread = new Thread(receiveMessage);
            receiveThread.Start();
        }

        public String getIP()
        {
            return this.socket.Client.RemoteEndPoint.ToString();
        }

        private void receiveMessage()
        {
            while (socket.Connected)
            {
                byte[] buffer = new byte[socket.Available];
                this.socket.GetStream().Read(buffer, 0, buffer.Length);
                String message = Encoding.UTF8.GetString(buffer);

                MessageBox.Show(message);
            }
        }
    }
}
