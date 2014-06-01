using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World;

namespace TCPGameServer.Server.InputHandling
{
    class InputHandler
    {
        

        //outputData.Add("Tile,Detected," + tile.getX() + "," + tile.getY() + "," + tile.getZ() + "," + tile.getRepresentation());3

        public InputHandler(Model world)
        {
            
        }

        /*
        private List<String> getInitData(Player player)
        {
            List<String> outputData = new List<String>();

            Tile playerPosition = player.getBody().getPosition();

            addPlayerLocation(playerPosition, outputData);

            world.getSurroundingTiles(playerPosition, outputData, 5);

            return outputData;
        }

        private void addPlayerLocation(Tile playerPosition, List<String> outputData)
        {
            outputData.Add("Player,Position," + playerPosition.getX() + "," + playerPosition.getY() + "," + playerPosition.getZ());
        }

        private List<String> handleInput(List<String> userInput)
        {
            List<String> outputData = new List<String>();

            bool playerHasMoved = false;

            foreach (String input in userInput)
            {
                int direction = Directions.fromShortString(input);

                playerHasMoved = (direction != -1 && movePlayer(direction, outputData));
            }

            if (playerHasMoved) addPlayerLocation(outputData);

            world.getSurroundingTiles(outputData, 5);

            return outputData;
        }

        private bool movePlayer(Player player, int direction, List<String> outputData)
        {
            Tile playerPosition = player.getBody().getPosition();

            if (playerPosition.hasNeighbor(direction))
            {
                Tile neighbor = playerPosition.getNeighbor(direction);

                if (neighbor.isPassable() && !neighbor.hasOccupant())
                {
                    playerPosition.vacate();
                    player.getBody().setPosition(neighbor);
                    return true;
                }
            }
            return false;
        }*/
    }
}
