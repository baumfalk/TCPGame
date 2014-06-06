using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control;

namespace TCPGameServer.Control.IO
{
    class Output
    {
        // Contains a log of everything that is printed so far.
        private static List<string> log = new List<string>();

        // output to the window, or to the debug stream if headless. Also added to the log.
        public static void Print(string message)
        {
            if (!Controller.headless) ServerOutputWindow.Print(message);
            Console.WriteLine(message);

            log.Add(message);
        }

        // special output, only when not headless. Useful for debugging.
        public static void Debug(string message)
        {
            if (!Controller.headless) {
                ServerOutputWindow.Print(message);
                log.Add(message);
            }
        }

        // return the log
        public static List<String> GetLog()
        {
            return log;
        }
    }
}
