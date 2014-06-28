using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Map.IO.MapFile
{
    public class IOTile
    {
        public static TileData Read(StreamReader fileReader)
        {
            TileData toReturn = new TileData();

            toReturn.ID = int.Parse(fileReader.ReadLine());
            toReturn.type = (TileType) Enum.Parse(typeof(TileType), fileReader.ReadLine());
            toReturn.representation =
                (TileRepresentation)Enum.Parse(typeof(TileRepresentation), fileReader.ReadLine());
            toReturn.location.x = int.Parse(fileReader.ReadLine());
            toReturn.location.y = int.Parse(fileReader.ReadLine());
            toReturn.location.z = int.Parse(fileReader.ReadLine());
            toReturn.links = fileReader.ReadLine().Split(',');

            return toReturn;
        }

        public static void Write(TileData tileData, StreamWriter fileWriter)
        {
            fileWriter.WriteLine(tileData.ID);
            fileWriter.WriteLine(tileData.type);
            fileWriter.WriteLine(tileData.representation);
            fileWriter.WriteLine(tileData.location.x);
            fileWriter.WriteLine(tileData.location.y);
            fileWriter.WriteLine(tileData.location.z);
            fileWriter.WriteLine(tileData.links[Directions.NORTH] + "," +
                                 tileData.links[Directions.EAST] + "," +
                                 tileData.links[Directions.UP] + "," +
                                 tileData.links[Directions.SOUTH] + "," +
                                 tileData.links[Directions.WEST] + "," +
                                 tileData.links[Directions.DOWN]);
            
        }
    }
}
