using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using TCPGameServer.Control;

namespace TCPGameServer
{
    static class Server
    {
        public static String version = "infinite adventure 0.2";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            // if one of the arguments is --headless, we run in headless mode. Otherwise we're using the window
            if (args.Length > 0 && args.Contains("--headless"))
            {
                // headless controller
                new Controller(true);
            }
            else
            {
                // "headed" controller
                new Controller(false);       
            }
        }
    }
}
