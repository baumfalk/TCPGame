using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using TCPGameServer.World.Map.IO.MapFile.Parts;
using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.IO.MapFile
{
    public class AreaFile
    {
        private static String GetFileName(String name)
        {
            // path, based on where the files are in the git repository
            String areaPath = @"../../../map/";

            return areaPath + name + ".are";
        }

        public static bool Exists(String name)
        {
            return (File.Exists(GetFileName(name)));
        }

        public static bool IsStub(String name)
        {
            StreamReader fileReader = new StreamReader(GetFileName(name));

            HeaderData data = Header.Read(fileReader);

            fileReader.Close();

            return data.fileType.Equals("Stub");
        }

        public static AreaFileData Read(String name)
        {
            StreamReader fileReader = new StreamReader(GetFileName(name));

            AreaFileData data = Read(fileReader);

            fileReader.Close();

            return data;
        }

        private static AreaFileData Read(StreamReader fileReader)
        {
            AreaFileData toReturn = new AreaFileData();

            toReturn.header = Header.Read(fileReader);
            toReturn.entrances = TileBlock.Read(fileReader);

            int numEntrances = toReturn.entrances.numberOfTiles;

            toReturn.fixedTiles = new TileBlockData[numEntrances];

            for (int n = 0; n < numEntrances; n++ )
            {
                toReturn.fixedTiles[n] = TileBlock.Read(fileReader);
            }

            return toReturn;
        }

        public static HeaderData ReadHeader(String name)
        {
            StreamReader fileReader = new StreamReader(GetFileName(name));

            HeaderData data = Header.Read(fileReader);

            fileReader.Close();

            return data;
        }

        public static void Write(AreaFileData toWrite, String name)
        {
            StreamWriter fileWriter = new StreamWriter(GetFileName(name));

            Write(toWrite, fileWriter);

            fileWriter.Close();
        }

        private static void Write(AreaFileData toWrite, StreamWriter fileWriter)
        {
            Header.Write(toWrite.header, fileWriter);

            TileBlock.Write(toWrite.entrances, fileWriter);

            int numEntrances = toWrite.entrances.numberOfTiles;

            for (int n = 0; n < numEntrances; n++)
            {
                TileBlock.Write(toWrite.fixedTiles[n], fileWriter);
            }
        }
    }
}
