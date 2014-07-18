using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TCPGameClient.Control;

namespace TCPGameClient
{
    static class Client
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            new Controller();
        }
    }
}
