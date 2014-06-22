using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Map.IO.MapFile.Parts
{
    public class TileBlock
    {
        public static TileBlockData Read(StreamReader fileReader)
        {
            TileBlockData toReturn = new TileBlockData();

            int numberOfTiles = int.Parse(fileReader.ReadLine());

            toReturn.numberOfTiles = numberOfTiles;
            toReturn.tileData = new TileData[numberOfTiles];

            for (int n = 0; n < numberOfTiles; n++)
            {
                toReturn.tileData[n] = IOTile.Read(fileReader);
            }

            return toReturn;
        }

        public static void Write(TileBlockData toWrite, StreamWriter fileWriter)
        {
            fileWriter.WriteLine(toWrite.numberOfTiles);

            int numberOfTiles = toWrite.numberOfTiles;

            for (int n = 0; n < numberOfTiles; n++)
            {
                IOTile.Write(toWrite.tileData[n], fileWriter);
            }
        }
    }
}
