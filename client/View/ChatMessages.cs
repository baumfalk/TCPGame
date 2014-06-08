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
        // needed for thread-safe output to the textbox
        private delegate void SetTextCallback(String text);
        public ChatMessages()
        {
            InitializeComponent();
        }

        public void addMessage(string rawMessage)
        {
            // if this request comes from another thread than the one that created the
            // form (which should be the case), we need to tell the thread that did create
            // it to come write something in the textbox. That's what we're doing here.
            if (this.txtMessages.InvokeRequired)
            {
                try
                {
                    SetTextCallback d = new SetTextCallback(addMessage);
                    this.Invoke(d, new object[] { rawMessage });
                }
                catch (ObjectDisposedException e)
                {
                    System.Diagnostics.Debug.Print("form was disposed on write");
                    System.Diagnostics.Debug.Print(e.Message);
                }
            }
            else
            {
                // if this is the right thread, just write it down.
                String[] message = rawMessage.Split(new char[]{','},4); // split in 4 parts: time, command, from, and message.
                this.txtMessages.Text += message[2] + ": " + message[3] + "\r\n";
            }
        }
    }
}
