using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TCPGameServer.Network;

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

        // I have no idea how to properly start a window from the controller, so this form
        // is the entry point for the program. Starts a controller.
        private void ServerOutputWindow_Load(object sender, EventArgs e)
        {
            new Controller();
        }

        // tells the only window to close
        public static void Shutdown()
        {
            onlyWindow.DoShutdown();
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
            onlyWindow.addMessageToTextbox(message);
        }

        
        private void addMessageToTextbox(String message)
        {
            // if this request comes from another thread than the one that created the
            // form (which should be the case), we need to tell the thread that did create
            // it to come write something in the textbox. That's what we're doing here.
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
    }
}
