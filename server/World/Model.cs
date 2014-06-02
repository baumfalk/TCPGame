using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using TCPGameServer.World.ActionHandling;

namespace TCPGameServer.World
{
    public class Model
    {
        private Tile[,,] tiles = new Tile[40, 8, 2];

        private List<Player> players;

        private ActionHandler actionHandler;

        public Model() {
            players = new List<Player>();

            actionHandler = new ActionHandler(this);

            createModel();
        }

        // dit moet onderverdeeld worden in areas oid
        public Tile FindTileAt(int x, int y, int z)
        {
            foreach (Tile tile in tiles)
            {
                if (x == tile.getX() && y == tile.getY() && z == tile.getZ())
                {
                    return tile;
                }
            }

            return null;
        }

        public void doUpdate()
        {
            List<Player> disconnectedPlayers = new List<Player>();

            // er zijn nog geen updates die zonder player interaction gebeuren, maar die moeten hier

            foreach (Player player in players)
            {
                if (player.isDisconnected())
                {
                    Network.Controller.Print("player is disconnected");

                    disconnectedPlayers.Add(player);
                    continue;
                }

                // handle one blocking command per tick
                if (player.hasNextBlockingCommand())
                {
                    String command = player.getNextBlockingCommand();

                    Network.Controller.Print("handling blocking command " + command);

                    actionHandler.Handle(player, command);
                }

                // handle all immediate commands
                while (player.hasImmediateCommands())
                {
                    String command = player.getNextImmediateCommand();

                    Network.Controller.Print("handling immediate command " + command);

                    actionHandler.Handle(player, command);
                }
            }

            foreach (Player disconnected in disconnectedPlayers)
            {
                removePlayer(disconnected);
            }

            foreach (Player player in players)
            {
                if (player.hasMoved())
                {
                    actionHandler.Handle(player, "LOOK,TILES_INCLUDED,PLAYER_INCLUDED");
                    player.setMoved(false);
                }
                else
                {
                    actionHandler.Handle(player, "LOOK,TILES_EXLUDED,PLAYER_INCLUDED");
                }
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

        public List<Tile> getSurroundingTiles(Tile centerTile, int depth)
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
