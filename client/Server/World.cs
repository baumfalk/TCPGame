using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Timers;

// we gebruiken even de tiles enzo uit het model, want stiekem is er helemaal geen server
using TCPGameClient.Model;
using TCPGameClient.Control;

namespace TCPGameClient.Server
{
    class World
    {
        private Timer tmTick;

        private Player thePlayer;

        private Controller user;

        private Tile[,] tiles = new Tile[10, 8];

        public World() {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    String type;

                    if (x == 0 || x == 9 || y == 0 || y == 7)
                    {
                        type = "wall";
                    }
                    else
                    {
                        type = "floor";
                    }

                    tiles[x, y] = new Tile(type, type, x, y);

                    if (x != 0) tiles[x, y].link(3, tiles[x - 1, y]);
                    if (y != 0) tiles[x, y].link(0, tiles[x, y - 1]);
                }
            }

            thePlayer = new Player(null);
            
            tiles[4, 3].setOccupant(thePlayer);

            tmTick = new Timer(100);
            tmTick.Elapsed += tmTick_Elapsed;
        }

        void tmTick_Elapsed(object sender, ElapsedEventArgs e)
        {
            // vraag input;
            List<String> outputData = handleInput(user.getInput());

            // geef output;
            user.doUpdate(outputData);
        }

        private List<String> handleInput(List<String> userInput)
        {
            List<String> outputData = new List<String>();

            bool playerHasMoved = false;

            foreach (String input in userInput) 
            {
                switch (input)
                {
                    case "n":
                        playerHasMoved = movePlayer(0, outputData);
                        break;
                    case "e":
                        playerHasMoved = movePlayer(1, outputData);
                        break;
                    case "s":
                        playerHasMoved = movePlayer(2, outputData);
                        break;
                    case "w":
                        playerHasMoved = movePlayer(3, outputData);
                        break;
                }
            }

            getSurroundingTiles(outputData, 5);

            return outputData;
        }

        private bool movePlayer(int direction, List<String> outputData)
        {
            Tile playerPosition = thePlayer.getPosition();

            if (playerPosition.hasNeighbor(direction))
            {
                Tile neighbor = playerPosition.getNeighbor(direction);

                if (neighbor.isPassable() && !neighbor.hasOccupant())
                {
                    playerPosition.vacate();
                    thePlayer.setPosition(neighbor);

                    addPlayerLocation(outputData);

                    return true;
                }
            }
            return false;
        }

        private List<String> getInitData()
        {
            List<String> initData = new List<String>();

            addPlayerLocation(initData);

            getSurroundingTiles(initData, 5);

            return initData;
        }

        private void addPlayerLocation(List<String> outputData)
        {
            outputData.Add("Player,Position," + thePlayer.getPosition().getX() + "," + thePlayer.getPosition().getY());
        }

        private void getSurroundingTiles(List<String> outputData, int depth)
        {
            List<Tile> tilesToSend = new List<Tile>();

            IterativeDeepeningTiles(thePlayer.getPosition(), depth, tilesToSend);

            foreach (Tile tile in tilesToSend)
            {
                // set color back to unexplored status
                tile.setColor(0);
            }
        }

        private void IterativeDeepeningTiles(Tile startingTile, int depth, List<Tile> tilesToSend)
        {
            // if the tile has been explored, don't do it again
            if (startingTile.getColor() == 1) return;

            // this tile has been explored
            startingTile.setColor(1);

            // this tile should be sent
            tilesToSend.Add(startingTile);

            // don't continue if we have no depth left
            if (depth == 0) return;

            // check if there's a neighbor in each direction, and recursively call this method
            for (int direction = 0; direction < 4; direction++)
            {
                if (startingTile.hasNeighbor(direction))
                {
                    IterativeDeepeningTiles(startingTile.getNeighbor(direction), depth - 1, tilesToSend);
                }
            }
        }

        public void registerUser(Controller user)
        {
            this.user = user;

            List<String> initData = getInitData();

            user.doUpdate(initData);

            tmTick.Start();
        }

        public void removeUser(Controller user)
        {
            this.user = null;
            tmTick.Stop();
        }
    }
}
