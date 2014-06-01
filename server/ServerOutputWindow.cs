using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TCPGameServer.Server;

namespace TCPGameServer
{
    public partial class ServerOutputWindow : Form
    {
        delegate void SetTextCallback(String text);

        public static ServerOutputWindow onlyWindow;

        Controller server;

        public ServerOutputWindow()
        {
            InitializeComponent();

            //lelijk, maar whatever. hack hack
            onlyWindow = this;
        }

        private void ServerOutputWindow_Load(object sender, EventArgs e)
        {
            server = new Controller(this);
        }

        public void addMessageToTextbox(String message)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addMessageToTextbox);
                this.Invoke(d, new object[] { message });
            } 
            else
            {
                textBox1.AppendText(message + "\n");
            }
        }
    }
}
