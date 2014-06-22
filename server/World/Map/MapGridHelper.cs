using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Map
{
    class MapGridHelper
    {
        // converts tile location to bottom left on map
        public static Location TileLocationToBottomLeft(Location tileLocation)
        {
            Location bottomLeft = TileLocationToMapGridLocation(tileLocation);

            bottomLeft.x *= 100;
            bottomLeft.y *= 100;

            return bottomLeft;
        }

        // converts tile location to map grid location
        public static Location TileLocationToMapGridLocation(Location tileLocation)
        {
            Location mapGridPosition = new Location();

            mapGridPosition.x = ((tileLocation.x < 0) ? tileLocation.x + 1 : tileLocation.x) / 100;
            mapGridPosition.y = ((tileLocation.y < 0) ? tileLocation.y + 1 : tileLocation.y) / 100;
            mapGridPosition.z = tileLocation.z;

            if (tileLocation.x < 0) mapGridPosition.x -= 1;
            if (tileLocation.y < 0) mapGridPosition.y -= 1;

            return mapGridPosition;
        }

        // converts tile location to position on current map
        public static Location TileLocationToCurrentMapLocation(Location tileLocation)
        {
            Location currentMapLocation = new Location();
            Location bottomLeft = TileLocationToBottomLeft(tileLocation);

            currentMapLocation.x = tileLocation.x - bottomLeft.x;
            currentMapLocation.y = tileLocation.y - bottomLeft.y;
            currentMapLocation.z = tileLocation.z - bottomLeft.z;

            return currentMapLocation;
        }

        // converts current map location and bottom left position to a tile location
        public static Location CurrentMapLocationToTileLocation(Location tileLocation, Location bottomLeft)
        {
            Location realLocation = new Location();

            realLocation.x = tileLocation.x + bottomLeft.x;
            realLocation.y = tileLocation.y + bottomLeft.y;
            realLocation.z = tileLocation.z + bottomLeft.z;

            return realLocation;
        }

        // converts map grid location to bottom left location
        public static Location MapGridLocationToBottomLeft(Location mapGridLocation)
        {
            Location bottomLeft = new Location();

            bottomLeft.x = mapGridLocation.x * 100;
            bottomLeft.y = mapGridLocation.y * 100;
            bottomLeft.z = mapGridLocation.z;

            return bottomLeft;
        }
    }
}
