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

namespace TCPGameServer.Control.IO
{
    public partial class ServerOutputWindow : Form
    {
        // needed for thread-safe output to the textbox
        private delegate void SetTextCallback(String text);
        private delegate void ShutdownCallback();

        // we want only one window. We keep a tab on it so static printing can be routed to
        // the window that's open.
        private static ServerOutputWindow onlyWindow;

        private static bool loaded = false;

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
            if (!loaded) return;

            onlyWindow.addMessageToTextbox(message);
        }

        private void addMessageToTextbox(String message)
        {
            // if this request comes from another thread than the one that created the
            // form (which should be the case), we need to tell the thread that did create
            // it to come write something in the textbox. That's what we're doing here.

            try
            {
                if (this.textBox1.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(addMessageToTextbox);
                    this.Invoke(d, new object[] { message });
                }
                else
                {
                    // if this is the right thread, just write it down.
                    textBox1.AppendText(message + "\n");
                }
            }
            catch (ObjectDisposedException e)
            {
                System.Diagnostics.Debug.Print("form was disposed on write");
                System.Diagnostics.Debug.Print(e.Message);
            }
        }

        private void ServerOutputWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Controller.Stop();
        }

        private void ServerOutputWindow_Load(object sender, EventArgs e)
        {
            loaded = true;

            List<String> log = Output.GetLog();

            foreach (String text in log)
            {
                Print(text);
            }
        }
    }
}
