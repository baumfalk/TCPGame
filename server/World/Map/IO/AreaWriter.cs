using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.IO
{
    class AreaWriter
    {
        private class AbstractTile
        {
            public String ID;
            public String type;
            public String representation;
            public String xLoc;
            public String yLoc;
            public String zLoc;
            public String links;
        }

        // path, based on where the files are in the git repository
        private static String gitPath = @"../../../map/";

        public static int AddEntrance(Tile exit, int direction, String name)
        {
            Output.Print("creating link from tile at " + exit.GetX() + "," + exit.GetY() + "," + exit.GetZ() + " in direction " + Directions.ToString(direction));

            bool createFile = !AreaReader.Exists(name);

            AbstractTile target = new AbstractTile();

            target.type = exit.GetType();
            target.representation = exit.GetRepresentation();

            int[] targetLocation = Directions.GetNeighboring(direction, exit.GetX(), exit.GetY(), exit.GetZ());
            target.xLoc = targetLocation[0].ToString();
            target.yLoc = targetLocation[1].ToString();
            target.zLoc = targetLocation[2].ToString();
            
            int linkDirection = Directions.Inverse(direction);

            String[] targetLinks = new String[6];
            for (int dir = 0; dir < 6; dir++)
            {
                if (dir == linkDirection) targetLinks[dir] = exit.GetArea().GetName() + ";" + exit.GetID();
                else targetLinks[dir] = "-1";
            }
            target.links = targetLinks[0] + "," + targetLinks[1] + "," + targetLinks[2] + "," + targetLinks[3] + "," + targetLinks[4] + "," + targetLinks[5];

            int mapGridPositionX = targetLocation[0] / 100;
            int mapGridPositionY = targetLocation[1] / 100;
            int mapGridPositionZ = targetLocation[2];

            if (targetLocation[0] < 0) mapGridPositionX -= 1;
            if (targetLocation[1] < 0) mapGridPositionY -= 1;

            Output.Print("target location is " + targetLocation[0] + ", " + targetLocation[1] + ", " + targetLocation[2]);

            if (!name.Equals("x" + mapGridPositionX + "y" + mapGridPositionY + "z" + mapGridPositionZ)) {
                Output.Print("name (" + name + ") does not match area (" + "x" + mapGridPositionX + "y" + mapGridPositionY + "z" + mapGridPositionZ + "), aborting AddEntrance");
                
                return -1;
            }

            if (!createFile)
            {
                Output.Print("not writing a new file");
                return -1;
            }
            else
            {
                CreateMapFile(name, new int[] {mapGridPositionX, mapGridPositionY, mapGridPositionZ}, target, exit.GetWorld());

                return 0;
            }
        }

        private static void CreateMapFile(String name, int[] mapGridPosition, AbstractTile target, World world)
        {
            StreamWriter fileWriter = new StreamWriter(gitPath + name + ".are");

            fileWriter.WriteLine("Stub");

            fileWriter.WriteLine(world.GetAreaType(mapGridPosition[0], mapGridPosition[1], mapGridPosition[2]));

            fileWriter.WriteLine(world.GetAreaSeed(mapGridPosition[0], mapGridPosition[1], mapGridPosition[2]));

            fileWriter.WriteLine(mapGridPosition[0]);

            fileWriter.WriteLine(mapGridPosition[1]);

            fileWriter.WriteLine(mapGridPosition[2]);

            fileWriter.WriteLine(1);

            target.ID = "0";
            WriteAbstractTile(fileWriter, target);

            fileWriter.WriteLine(0);

            fileWriter.Close();
        }

        public static void SaveStatic(String name, int seed, String areaType, int[] mapGridPosition, Tile[] entrances, Tile[] exits, Tile[][] fixedTiles)
        {
            Output.Print("Writing static for " + name);

            StreamWriter fileWriter = new StreamWriter(gitPath + name + ".are2");

            fileWriter.WriteLine("Generated");

            fileWriter.WriteLine(areaType);

            fileWriter.WriteLine(seed);

            fileWriter.WriteLine(mapGridPosition[0]);

            fileWriter.WriteLine(mapGridPosition[1]);

            fileWriter.WriteLine(mapGridPosition[2]);

            fileWriter.WriteLine(entrances.Length + exits.Length);

            Output.Print(entrances.Length + " entrances");
            for (int n = 0; n < entrances.Length; n++) {
                AbstractTile abstractTile = ConvertTile(entrances[n], 0);
                WriteAbstractTile(fileWriter, abstractTile);
            }

            Output.Print(exits.Length + " exits");
            for (int n = 0; n < exits.Length; n++)
            {
                AbstractTile abstractTile = ConvertTile(exits[n], 0);
                WriteAbstractTile(fileWriter, abstractTile);
            }

            for (int n = 0; n < fixedTiles.Length; n++)
            {
                fileWriter.WriteLine(fixedTiles[n].Length);

                for (int i = 0; i < fixedTiles[n].Length; i++)
                {
                    // indices of fixed tiles go up by exits.Length
                    AbstractTile abstractTile = ConvertTile(fixedTiles[n][i], exits.Length);
                    WriteAbstractTile(fileWriter, abstractTile);
                }
            }

            fileWriter.Close();
        }

        private static AbstractTile ConvertTile(Tile toWrite, int exitOffset)
        {
            AbstractTile abstractTile = new AbstractTile();

            abstractTile.ID = (toWrite.GetID() + exitOffset).ToString();
            abstractTile.type = toWrite.GetType();
            abstractTile.representation = toWrite.GetRepresentation();
            abstractTile.xLoc = toWrite.GetX().ToString();
            abstractTile.yLoc = toWrite.GetY().ToString();
            abstractTile.zLoc = toWrite.GetZ().ToString();

            String[] links = new String[6];

            for (int direction = 0; direction < 6; direction++)
            {
                int link = toWrite.GetLink(direction);

                if (link == -1) links[direction] = "-1";
                if (link == -2) links[direction] = toWrite.GetLinkText(direction);
                else links[direction] = (link + exitOffset).ToString();
            }
            abstractTile.links = links[0] + "," + links[1] + "," + links[2] + "," + links[3] + "," + links[4] + "," + links[5];

            return abstractTile;
        }

        private static void WriteAbstractTile(StreamWriter fileWriter, AbstractTile abstractTile)
        {
            fileWriter.WriteLine(abstractTile.ID);
            fileWriter.WriteLine(abstractTile.type);
            fileWriter.WriteLine(abstractTile.representation);
            fileWriter.WriteLine(abstractTile.xLoc);
            fileWriter.WriteLine(abstractTile.yLoc);
            fileWriter.WriteLine(abstractTile.zLoc);
            fileWriter.WriteLine(abstractTile.links);
        }
    }
}
