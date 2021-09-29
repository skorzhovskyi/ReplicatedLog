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
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Client client = new Client(new Uri(textBoxURL.Text));

            client.sendMessage("Hi!");
        }
    }
}
