using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using TCPGameServer.Control;

namespace TCPGameServer
{
    static class Server
    {
        public static String version = Properties.Settings.Default.versionString;
        private static int port = Properties.Settings.Default.port;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            // if one of the arguments is --headless, we run in headless mode. Otherwise we're using the window
            bool headless = ParseArgumentExists(args, "--headless");
            // if a port is passed, use it instead of the default from App.config
            if (ParseArgumentExists(args, "--port"))
                port = int.Parse(ParseArgumentParameters(args, "--port", 1)[0]);

            new Controller(headless, port);
        }

        private static bool ParseArgumentExists(string[] args, string key)
        {
            return args.Contains(key);
        }

        private static string[] ParseArgumentParameters(string[] args, string key, int numParameters)
        {
            List<string> values = new List<string>(numParameters);
            for (int n = 0; n < args.Length; n++)
            {
                if (args[n].Equals(key))
                {
                    for (int i = 0; i < numParameters; i++)
                    {
                        if ((n + i) < args.Length)
                            values.Add(args[n + i]);
                        else throw new ArgumentNullException("args[" + (n + i) + "]");
                    }
                }
            }
            return values.ToArray();
        }
    }
}
