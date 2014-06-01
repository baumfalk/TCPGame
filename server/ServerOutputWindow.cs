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
        Controller server;

        public ServerOutputWindow()
        {
            InitializeComponent();
        }

        private void ServerOutputWindow_Load(object sender, EventArgs e)
        {
            server = new Controller(this);
        }
    }
}
