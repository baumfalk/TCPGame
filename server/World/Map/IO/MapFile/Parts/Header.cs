using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TCPGameServer.World.Map.IO.MapFile.Parts
{
    public class Header
    {
        public static HeaderData Read(StreamReader fileReader)
        {
            HeaderData toReturn = new HeaderData();

            toReturn.fileType = fileReader.ReadLine();
            toReturn.areaType = fileReader.ReadLine();
            toReturn.seed = int.Parse(fileReader.ReadLine());
            toReturn.mapGridLocation.x = int.Parse(fileReader.ReadLine());
            toReturn.mapGridLocation.y = int.Parse(fileReader.ReadLine());
            toReturn.mapGridLocation.z = int.Parse(fileReader.ReadLine());

            return toReturn;
        }

        public static void Write(HeaderData toWrite, StreamWriter fileWriter)
        {
            fileWriter.WriteLine(toWrite.fileType);
            fileWriter.WriteLine(toWrite.areaType);
            fileWriter.WriteLine(toWrite.seed);
            fileWriter.WriteLine(toWrite.mapGridLocation.x);
            fileWriter.WriteLine(toWrite.mapGridLocation.y);
            fileWriter.WriteLine(toWrite.mapGridLocation.z);
        }
    }
}
