using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngbandMud
{
    class Server
    {
        private static byte[] strMOTD = System.Text.Encoding.ASCII.GetBytes(
            "Welcome to AngbandMUD\r\n\n" + 
            "This is just a mockup, so there isn't much here at the moment.\r\n\n" + 
            "Go north/south/east/west/up/down using the commands n/s/e/w/u/d.\r\n" + 
            "kill creatures by typing kill <name>.\r\n" +
            "turn on/off room descs typing toggle short\r\n" + 
            "kijken wat er in de kamers om je heen is kan met scan\r\n" + 
            "Nothing else is implemented.\r\n\n" + 
            "What is your character name?\r\n");

        static void Main(string[] args)
        {
            TCPServer tsServer = new TCPServer();

            tsServer.Start();

            Console.ReadLine();

            tsServer.Stop();
        }



        
    }
}
