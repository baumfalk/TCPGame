using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.Network
{
    class MessageFormatting
    {
        // we handle a lot of queues which have to be translated to strings
        public static String FormatMessageQueue(Queue<String> strings) {
            // use a stringbuilder since we'll be concatenating a lot of text
            StringBuilder builder = new StringBuilder();

            // run through the entire queue, append each item after each other, with
            // a semicolon behind each item
            while (strings.Count > 0)
            {
                builder.Append(strings.Dequeue());
                builder.Append(";");
            }

            // build the string and return it
            return builder.ToString();
        }
    }
}
