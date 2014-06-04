using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using TCPGameServer.Control;

namespace TCPGameServer
{
    public partial class ServerOutputWindow : Form
    {
        // needed for thread-safe output to the textbox
        delegate void SetTextCallback(String text);
        delegate void ShutdownCallback();

        // we want only one window. We keep a tab on it so static printing can be routed to
        // the window that's open.
        private static ServerOutputWindow onlyWindow;

        // create a window, but only if none exist.
        public ServerOutputWindow()
        {
            if (onlyWindow != null) return;
            onlyWindow = this;

            InitializeComponent();
        }

        // tells the only window to close
        public static void Shutdown()
        {
            if (onlyWindow != null) onlyWindow.DoShutdown();
        }

        // invokes this method to close it down on the right thread
        private void DoShutdown()
        {
            if (this.InvokeRequired)
            {
                ShutdownCallback d = new ShutdownCallback(DoShutdown);
                this.Invoke(d, new object[0]);
            }
            else
            {
                Close();
            }
        }

        // gets the open window and calls the addMessageToTextbox method on it
        public static void Print(String message)
        {
            // if a message is sent before the form is loaded, let it wait until the
            // form is loaded, and write it then.
            if (onlyWindow == null) new Thread(MessageTooSoon).Start(message);

            onlyWindow.addMessageToTextbox(message);
        }

        // saves messages sent before the form is loaded, and sends them back to
        // the Print method when it is. The order of the messages will not be kept.
        private static void MessageTooSoon(object message)
        {
            String string_message = "(printed before form was loaded)" + (String)message;

            while (onlyWindow == null)
            {
                Thread.Sleep(1);
            }

            Print(string_message);
        }

        private void addMessageToTextbox(String message)
        {
            // if this request comes from another thread than the one that created the
            // form (which should be the case), we need to tell the thread that did create
            // it to come write something in the textbox. That's what we're doing here.
            if (this.textBox1.InvokeRequired)
            {
                try
                {
                    SetTextCallback d = new SetTextCallback(addMessageToTextbox);
                    this.Invoke(d, new object[] { message });
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
                textBox1.AppendText(message + "\n");
            }
        }

        private void ServerOutputWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Controller.Stop();
        }
    }
}
