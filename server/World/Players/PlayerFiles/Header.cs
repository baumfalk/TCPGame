using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TCPGameServer.World.Players.PlayerFiles
{
    class Header
    {
        public static HeaderData Read(StreamReader fileReader)
        {
            HeaderData toReturn = new HeaderData();

            toReturn.name = fileReader.ReadLine();
            toReturn.salt = Convert.FromBase64String(fileReader.ReadLine());
            toReturn.password = fileReader.ReadLine();
            toReturn.area = fileReader.ReadLine();
            toReturn.tileIndex = int.Parse(fileReader.ReadLine());

            return toReturn;
        }

        public static void Write(HeaderData toWrite, StreamWriter fileWriter)
        {
            fileWriter.WriteLine(toWrite.name);
            fileWriter.WriteLine(Convert.ToBase64String(toWrite.salt));
            fileWriter.WriteLine(toWrite.password);
            fileWriter.WriteLine(toWrite.area);
            fileWriter.WriteLine(toWrite.tileIndex);
        }
    }
}
