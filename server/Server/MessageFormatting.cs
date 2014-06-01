using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameServer.Server
{
    class MessageFormatting
    {
        public static String formatCollection(Queue<String> strings) {
            StringBuilder builder = new StringBuilder();

            while (strings.Count > 0)
            {
                builder.Append(strings.Dequeue());
                builder.Append(";");
            }

            return builder.ToString();
        }
    }
}
