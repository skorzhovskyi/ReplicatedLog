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
            var uri = new Uri(textBoxURL.Text);

            Client client;

            foreach (var it in listViewServers.Items)
            {
                client = (Client)((ListViewItem)it).Tag;

                if (client.HttpClient.BaseAddress == uri)
                {
                    MessageBox.Show("The server is already connected", "Info");
                    return;
                }
            }
            
            client = new Client(uri);

            var item = listViewServers.Items.Add(textBoxURL.Text);
            item.Tag = client;
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            foreach (var it in listViewServers.Items)
            {
                ListViewItem item = (ListViewItem)it;

                if (!item.Selected)
                    continue;

                Client client = (Client)item.Tag;

                client.SendMessage(textBoxMessage.Text);

                break;
            }
        }

        private void UpdateMessagesList()
        {
            foreach (var it in listViewServers.Items)
            {
                ListViewItem item = (ListViewItem)it;

                if (!item.Selected)
                    continue;

                Client client = (Client)item.Tag;

                var messages = client.GetMessages();

                listViewMessages.Clear();

                foreach (var msg in messages)
                    listViewMessages.Items.Add(msg);

                break;
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            UpdateMessagesList();
        }
    }
}
