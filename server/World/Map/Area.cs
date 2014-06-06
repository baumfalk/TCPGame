using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map
{
    public class Area
    {
        private World world;

        private String name;

        private Tile[] tiles;
        private String[] links;

        private String gitPath = @"../../../map/";

        public Area(World world, String name)
        {
            Output.Print("Loading area " + name);

            this.world = world;
            this.name = name;

            StreamReader fileReader = new StreamReader(gitPath + name + ".are");

            int numTiles = int.Parse(fileReader.ReadLine());

            tiles = new Tile[numTiles];
            links = new String[numTiles];

            for (int n = 0; n < numTiles; n++)
            {
                int id = int.Parse(fileReader.ReadLine());
                String type = fileReader.ReadLine();
                String representation = fileReader.ReadLine();
                int x = int.Parse(fileReader.ReadLine());
                int y = int.Parse(fileReader.ReadLine());
                int z = int.Parse(fileReader.ReadLine());
                links[n] = fileReader.ReadLine();

                if (n != id) Output.Print("ID mismatch (ID = " + n + ", file ID = " + id + "), map file incorrect. Bad links expected.");

                tiles[n] = new Tile(type, representation, x, y, z, id, this, world);
            }

            for (int n = 0; n < numTiles; n++)
            {
                String[] link = links[n].Split(',');

                for (int direction = 0; direction < 6; direction++)
                {
                    // should add a position mismatch for coordinates

                    if (link[direction].Contains(';'))
                    {
                        String[] areaLink = link[direction].Split(';');

                        String areaName = areaLink[0];
                        int ID = int.Parse(areaLink[1]);

                        tiles[n].CreateAreaLink(direction, areaName, ID);
                    }
                    else
                    {
                        int linkTo = int.Parse(link[direction]);

                        if (linkTo > -1) tiles[n].Link(direction, tiles[linkTo]);
                    }
                }
            }
        }

        public String GetName()
        {
            return name;
        }

        public Tile GetTile(int ID)
        {
            return tiles[ID];
        }
    }
}
