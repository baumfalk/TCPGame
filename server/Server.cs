using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPGameServer
{
    static class Server
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            if (args.Length > 0 && args[0].Equals("--headless") || Network.Controller.headless)
            {
                new Network.Controller();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ServerOutputWindow());
            }
        }
    }
}
