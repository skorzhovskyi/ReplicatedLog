using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReplicatedLogClient
{
    public partial class FormReplicatedLogClient : Form
    {
        public FormReplicatedLogClient()
        {
            InitializeComponent();
            iPv4EndPointTextBox.IPEndPoint = new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 192, 168, 0, 105 }), 2100);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Client client = new Client(iPv4EndPointTextBox.IPEndPoint);

            client.sendMessage("Hi!");
        }
    }
}
