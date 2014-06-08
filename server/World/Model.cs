using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using TCPGameServer.World.ActionHandling;
using TCPGameServer.World.Map;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World
{
    public class Model
    {
        // the map. Loads and unloads areas and tiles when needed
        private Map.World theWorld = new Map.World();

        // the list of player objects
        private List<Player> players;

        // handles actions by players
        private ActionHandler actionHandler;

        // commands (mostly messages) originating from the model
        private Queue<String[]> modelCommands;

        // constructor initializes fields and creates the world
        public Model() {
            players = new List<Player>();

            modelCommands = new Queue<String[]>();

            actionHandler = new ActionHandler(this);

            createWorld();
        }

        public void doUpdate(int tick)
        {
            // remove players that have been disconnected since last update
            RemoveDisconnectedPlayers();

            // handle blocking commands (like movement)
            HandleBlockingCommands(tick);

            // handle immediate commands (like vision)
            HandleImmediateCommands(tick);

            // handle model commands (like "player has logged on" messages)
            HandleModelCommands(tick);

            // unload areas that have been inactive
            UnloadInactiveAreas(tick);
        }

        // adds all disconnected players to a list and removes them from the
        // player list
        private void RemoveDisconnectedPlayers()
        {
            // create list to hold the disconnected players
            List<Player> disconnectedPlayers = new List<Player>();

            // loop through all players, if one is disconnected, add him to
            // the list and output a message saying so to the log and to the other players
            foreach (Player player in players)
            {
                if (player.IsDisconnected())
                {
                    Output.Print(player.GetName() + " has disconnected");

                    AddModelCommand(new String[] { "SAY", player.GetName() + " has disconnected" });

                    disconnectedPlayers.Add(player);
                }
            }

            // remove all disconnected players from the player list
            foreach (Player disconnected in disconnectedPlayers)
            {
                removePlayer(disconnected);
            }
        }

        // handles blocking commands. Only one blocking command per player
        // is handled per tick
        private void HandleBlockingCommands(int tick)
        {
            foreach (Player player in players)
            {
                // handle one blocking command per tick
                if (player.HasNextBlockingCommand())
                {
                    String [] cmdAndParameters = player.GetNextBlockingCommand();

                    actionHandler.Handle(player, cmdAndParameters, tick);
                }
            }
        }

        // handles immediate commands. All immediate commands per player
        // are handled each tick
        private void HandleImmediateCommands(int tick)
        {
            foreach (Player player in players)
            {
                // handle all immediate commands
                while (player.HasImmediateCommands())
                {
                    String [] command = player.GetNextImmediateCommand();
                    
                    actionHandler.Handle(player, command, tick);
                }
            }
        }

        // handles model commands. These are like immediate commands, but
        // have no player sending them
        private void HandleModelCommands(int tick)
        {
            while (modelCommands.Count > 0)
            {
                String[] cmdAndParameters = modelCommands.Dequeue();

                actionHandler.Handle(null, cmdAndParameters, tick);
            }
        }

        // unloads inactive areas
        private void UnloadInactiveAreas(int tick)
        {
            // unload inactive areas every 10 minutes
            if (tick % 6000 == 0)
            {
                foreach (Player player in players)
                {
                    // set the areas players are in to be active, so they
                    // don't get unloaded when people are standing still
                    player.GetBody().GetPosition().GetArea().SetActive();
                }

                theWorld.UnloadInactiveAreas();
            }
        }

        // add a model command
        public void AddModelCommand(String[] command)
        {
            modelCommands.Enqueue(command);
        }

        // the player list can be added to and a copy can be retrieved. Removal
        // can only be done internally.
        public void addPlayer(Player player)
        {
            players.Add(player);
        }
        public List<Player> getCopyOfPlayerList()
        {
            return new List<Player>(players);
        }
        private void removePlayer(Player player)
        {
            players.Remove(player);
        }

        // gets a tile in the model by area and ID
        public Tile GetTile(String area, int TileID)
        {
            return theWorld.GetTile(area, TileID);
        }

        // creating a world is just a matter of creating the object at the moment
        private void createWorld()
        {
            Map.World world = new Map.World();
        }
    }
}
