using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Map
{
    class Geography
    {
        public static List<Tile> GetTilesInRange(int range, List<List<Tile>> tilesAtRange)
        {
            List<Tile> toReturn = new List<Tile>();

            tilesAtRange[0][0].SetColor(1);
            toReturn.Add(tilesAtRange[0][0]);

            int exploredRange = tilesAtRange.Count - 1;

            for (int currentRange = 1; currentRange <= range; currentRange++)
            {
                if (currentRange > exploredRange)
                {
                    tilesAtRange.Add(Geography.GetNextLayer(tilesAtRange[currentRange - 1]));
                }

                foreach (Tile tile in tilesAtRange[currentRange])
                {
                    toReturn.Add(tile);
                    if (exploredRange < range) tile.SetColor(1);
                }
            }

            if (exploredRange < range)
            {
                foreach (Tile tile in toReturn)
                {
                    tile.SetColor(0);
                }
            }

            tilesAtRange[0][0].SetColor(0);

            return toReturn;
        }

        private static List<Tile> GetNextLayer(List<Tile> tileFront)
        {
            List<Tile> toReturn = new List<Tile>();

            foreach (Tile tile in tileFront)
            {
                for (int direction = 0; direction < 6; direction++)
                {
                    if (tile.HasNeighbor(direction))
                    {
                        Tile neighbor = tile.GetNeighbor(direction);

                        if (neighbor.GetColor() != 1 && neighbor.IsPassable())
                        {
                            neighbor.SetColor(1);

                            toReturn.Add(neighbor);
                        }
                    }
                }
            }

            return toReturn;
        }

        /*
        // gets the tiles surrounding a tile to a certain range
        private static List<Tile> getSurroundingTiles(Tile centerTile, int depth, bool vision)
        {
            // return buffer
            List<Tile> tilesToSend = new List<Tile>();

            // color is used to signify the depth
            centerTile.SetColor(depth);

            // create a queue for the BFS
            Queue<Tile> tileQueue = new Queue<Tile>();
            tileQueue.Enqueue(centerTile);

            // run BFS, depth is signalled by the color
            BFS_To_Depth(tileQueue, tilesToSend, vision);

            // set color for all tiles back to unexplored status
            foreach (Tile tile in tilesToSend)
            {
                tile.SetColor(0);
            }

            // return result
            return tilesToSend;
        }

        // finds tiles to a certain depth using a queue and tile color
        private static void BFS_To_Depth(Queue<Tile> tileQueue, List<Tile> tilesToSend, bool vision)
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
                if (depth == 0 || vision && !activeTile.IsPassable()) continue;

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
        }*/
    }
}
