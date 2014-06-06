using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TCPGameClient.View
{
    public partial class ChatMessages : Form
    {
        public ChatMessages()
        {
            InitializeComponent();
        }

        public void addMessage(string rawMessage)
        {
            String[] message = rawMessage.Split(',');
            this.txtMessages.Text += message[1] + ": " + message[2] + "\r\n";
        }
    }
}
