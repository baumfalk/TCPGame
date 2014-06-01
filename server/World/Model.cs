using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TCPGameServer.World
{
    class Model
    {
        private Tile[,,] tiles = new Tile[40, 8, 2];

        private List<Player> players;

        public Model() {
            players = new List<Player>();

            createModel();
        }

        public void doUpdate()
        {
            // er zijn nog geen updates die zonder player interaction gebeuren, maar die moeten hier

            foreach (Player player in players)
            {

            }
        }

        public void addPlayer(Player player)
        {
            players.Add(player);
        }

        public void removePlayer(Player player)
        {
            players.Remove(player);
        }

        public List<Player> getPlayers()
        {
            return players;
        }

        public List<Tile> getSurroundingTiles(Tile centerTile, List<String> outputData, int depth)
        {
            List<Tile> tilesToSend = new List<Tile>();
            
            centerTile.setColor(depth);

            Queue<Tile> tileQueue = new Queue<Tile>();
            tileQueue.Enqueue(centerTile);

            BFS_To_Depth(tileQueue, tilesToSend);

            foreach (Tile tile in tilesToSend)
            {
                // set color back to unexplored status
                tile.setColor(0);

                // add tile to output
                

                // je moet hier ook naar occupants kijken. Doe ik nog niet.
            }

            return tilesToSend;
        }

        private void BFS_To_Depth(Queue<Tile> tileQueue, List<Tile> tilesToSend)
        {
            while (tileQueue.Count > 0)
            {
                Tile activeTile = tileQueue.Dequeue();
                int depth = activeTile.getColor();

                if (depth == 0) return;

                tilesToSend.Add(activeTile);

                for (int direction = 0; direction < 6; direction++)
                {
                    if (activeTile.hasNeighbor(direction))
                    {
                        Tile neighbor = activeTile.getNeighbor(direction);
                        if (neighbor.getColor() == 0)
                        {
                            neighbor.setColor(depth - 1);

                            tileQueue.Enqueue(neighbor);
                        }
                    }
                }
            }
            
        }

        private void createModel()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    String type;

                    if (x == 0 || x == 9 || y == 0 || y == 7)
                    {
                        type = "wall";
                    }
                    else if ((x == 1 && y == 1) || (x == 8 && y == 6))
                    {
                        type = "stairs";
                    }
                    else
                    {
                        type = "floor";
                    }

                    tiles[x, y, 0] = new Tile(type, type, x, y, 0);
                    tiles[x, y, 1] = new Tile(type, type, x, y, 1);

                    if (x != 0)
                    {
                        tiles[x, y, 0].link(Directions.WEST, tiles[x - 1, y, 0]);
                        tiles[x, y, 1].link(Directions.WEST, tiles[x - 1, y, 1]);
                    }
                    if (y != 0)
                    {
                        tiles[x, y, 0].link(Directions.NORTH, tiles[x, y - 1, 0]);
                        tiles[x, y, 1].link(Directions.NORTH, tiles[x, y - 1, 1]);
                    }
                }
            }

            tiles[1, 1, 0].link(Directions.UP, tiles[1, 1, 1]);
            tiles[8, 6, 0].link(Directions.UP, tiles[8, 6, 1]);
        }
    }
}
