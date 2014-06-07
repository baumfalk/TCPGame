using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map
{
    class Geography
    {
        // gets the tiles surrounding a tile to a certain range
        public static List<Tile> getSurroundingTiles(Tile centerTile, int depth)
        {
            // return buffer
            List<Tile> tilesToSend = new List<Tile>();

            // color is used to signify the depth
            centerTile.SetColor(depth);

            // create a queue for the BFS
            Queue<Tile> tileQueue = new Queue<Tile>();
            tileQueue.Enqueue(centerTile);

            // run BFS, depth is signalled by the color
            BFS_To_Depth(tileQueue, tilesToSend);

            // set color for all tiles back to unexplored status
            foreach (Tile tile in tilesToSend)
            {
                tile.SetColor(0);
            }

            // return result
            return tilesToSend;
        }

        // finds tiles to a certain depth using a queue and tile color
        private static void BFS_To_Depth(Queue<Tile> tileQueue, List<Tile> tilesToSend)
        {
            // run while the queue isn't empty
            while (tileQueue.Count > 0)
            {
                // get next tile and how much further we need to go from here
                Tile activeTile = tileQueue.Dequeue();
                int depth = activeTile.GetColor();

                // add the tile to the output
                tilesToSend.Add(activeTile);

                // if we don't need to go any further, don't explore more tiles
                if (depth == 0) continue;

                // check in all six directions
                for (int direction = 0; direction < 6; direction++)
                {
                    // only if there's a neighbor
                    if (activeTile.HasNeighbor(direction))
                    {
                        // get the neighbor
                        Tile neighbor = activeTile.GetNeighbor(direction);
                        // check if it's unexplored. If it's explored by another tile
                        // another route that's at least as fast exists from the center 
                        // to that tile, and it's already in the queue.
                        if (neighbor.GetColor() == 0)
                        {
                            // if not, use the color to signify how much further we
                            // need to explore from that tile
                            neighbor.SetColor(depth - 1);

                            // and add it to the queue
                            tileQueue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }
}
