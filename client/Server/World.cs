using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Timers;

using System.Diagnostics;

using TCPGameClient.Control;

namespace TCPGameClient.Server
{
    class World
    {
        private Timer tmTick;

        private Player thePlayer;

        private Controller user;

        private Tile[,] tiles = new Tile[10, 8];

        private int numCommand;

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

            thePlayer = new Player("player");
            
            tiles[4, 3].setOccupant(thePlayer);

            tmTick = new Timer(100);
            tmTick.Elapsed += tmTick_Elapsed;
        }

        void tmTick_Elapsed(object sender, ElapsedEventArgs e)
        {
            numCommand = 0;

            // vraag input;
            List<String> outputData = handleInput(user.getInput());

            // sorteer. Zou goed moeten zijn, maar better safe dan sorry.
            outputData.Sort();

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

            if (playerHasMoved) addPlayerLocation(outputData);

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
                    return true;
                }
            }
            return false;
        }

        
        private List<String> getInitData()
        {
            List<String> outputData = new List<String>();

            addPlayerLocation(outputData);

            getSurroundingTiles(outputData, 5);

            return outputData;
        }

        private void addPlayerLocation(List<String> outputData)
        {
            outputData.Add(numCommand++ + ",Player,Position," + thePlayer.getPosition().getX() + "," + thePlayer.getPosition().getY());
        }

        private void getSurroundingTiles(List<String> outputData, int depth)
        {
            List<Tile> tilesToSend = new List<Tile>();

            thePlayer.getPosition().setColor(5);

            IterativeDeepeningTiles(thePlayer.getPosition(), tilesToSend);

            foreach (Tile tile in tilesToSend)
            {
                // set color back to unexplored status
                tile.setColor(0);

                // add tile to output
                outputData.Add(numCommand++ + ",Tile,Detected," + tile.getX() + "," + tile.getY() + "," + tile.getRepresentation());

                // je kan hier ook naar occupants kijken. Doe ik nog niet.
            }
        }

        private void IterativeDeepeningTiles(Tile startingTile, List<Tile> tilesToSend)
        {
            // add tiles that should be displayed here and it works
        }

        public void registerUser(Controller user)
        {
            Debug.Print("user registered");

            this.user = user;

            List<String> initData = getInitData();

            user.doUpdate(initData);

            //tmTick.Start();
        }

        public void removeUser(Controller user)
        {
            this.user = null;
            tmTick.Stop();
        }
    }
}
