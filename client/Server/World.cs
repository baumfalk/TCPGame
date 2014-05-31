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

        private Creature body;
        private Player thePlayer;

        private Controller user;

        private Tile[,] tiles = new Tile[40, 8];

        private int numCommand;

        public World() {
            for (int x = 0; x < 40; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    String type;

                    if (x == 0 || x == 39 || y == 0 || y == 7)
                    {
                        type = "wall";
                    }
                    else
                    {
                        type = "floor";
                    }

                    tiles[x, y] = new Tile(type, type, x, y, 0);

                    if (x != 0) tiles[x, y].link(Directions.WEST, tiles[x - 1, y]);
                    if (y != 0) tiles[x, y].link(Directions.NORTH, tiles[x, y - 1]);
                }
            }

            body = new Creature("player");
            
            tiles[4, 3].setOccupant(body);

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
                Debug.Print("input: ." + input + ".");

                int direction = Directions.fromShortString(input);

                playerHasMoved = (direction != -1 && movePlayer(direction, outputData));
            }

            if (playerHasMoved) addPlayerLocation(outputData);

            getSurroundingTiles(outputData, 5);

            return outputData;
        }

        private bool movePlayer(int direction, List<String> outputData)
        {
            Debug.Print("move player");

            Tile playerPosition = thePlayer.getBody().getPosition();

            if (playerPosition.hasNeighbor(direction))
            {
                Tile neighbor = playerPosition.getNeighbor(direction);

                if (neighbor.isPassable() && !neighbor.hasOccupant())
                {
                    playerPosition.vacate();
                    thePlayer.getBody().setPosition(neighbor);
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
            outputData.Add(numCommand++ + ",Player,Position," + thePlayer.getBody().getPosition().getX() + "," + thePlayer.getBody().getPosition().getY());
        }

        private void getSurroundingTiles(List<String> outputData, int depth)
        {
            List<Tile> tilesToSend = new List<Tile>();
            Tile playerPosition = thePlayer.getBody().getPosition();
            
            playerPosition.setColor(depth);

            Queue<Tile> tileQueue = new Queue<Tile>();
            tileQueue.Enqueue(playerPosition);

            BFS_To_Depth(tileQueue, tilesToSend);

            foreach (Tile tile in tilesToSend)
            {
                // set color back to unexplored status
                tile.setColor(0);

                // add tile to output
                outputData.Add(numCommand++ + ",Tile,Detected," + tile.getX() + "," + tile.getY() + "," + tile.getZ() + "," + tile.getRepresentation());

                // je kan hier ook naar occupants kijken. Doe ik nog niet.
            }
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

        public void registerUser(Controller user)
        {
            Debug.Print("user registered");

            this.user = user;

            thePlayer = new Player(body, user);

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
