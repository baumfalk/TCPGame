using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World;

using TCPGameServer.Control;

namespace TCPGameServer.Control.IO
{
    class InputHandler
    {
        public static void Handle(List<String> commands, Player player, User user)
        {
            // if there are no commands to handle, don't handle commands
            if (commands.Count == 0) return;

            // loop through all the commands that need to be handled
            foreach (String command in commands)
            {
                // for now we'll just add every command to the log
                Output.Print(command + " received from user");

                // shutdown and log commands can be given irrespective of state. At
                // some point these need to be behind the login, but not during
                // development
                if (command.Equals("shutdown"))
                {
                    Output.Print("Shutdown command received...");

                    // unset the running flag, server will shut down on the next cycle
                    Controller.Stop();
                }
                else if (command.Equals("log"))
                {
                    Output.Print("Sending log to user");

                    foreach (string message in Output.GetLog())
                    {
                        player.AddMessage("LOG: " + message);
                    }
                }
                else if (command.Equals("quit"))
                {
                    user.Remove();
                }
                else
                {
                    // switch on commandstate of the player. We don't want players
                    // to be able to move before logging in, or similar things. Send
                    // the command on to the proper method.
                    switch (player.GetCommandState())
                    {
                        case Player.COMMANDSTATE_IDLE:
                            HandleIdle(command, player);
                            break;
                        case Player.COMMANDSTATE_LOGIN:
                            HandleLogin(command, player);
                            break;
                        case Player.COMMANDSTATE_NORMAL:
                            HandleNormal(command, player);
                            break;
                    }
                }
            }
        }

        // Users shouldn't be able to send anything while idle. Just in case, it will generate
        // a message to the log.
        private static void HandleIdle(String command, Player player)
        {
            Output.Print("user is idle but sending commands");
        }

        // TODO: make login dynamic, reading from a file
        private static void HandleLogin(String command, Player player)
        {
            // place the player on the map
            if (command.Equals("geerten")) player.AddImmediateCommand("PLAYER,PLACE,start,0");
            else if (command.Equals("jetze")) player.AddImmediateCommand("PLAYER,PLACE,start,1");
            else player.AddImmediateCommand("PLAYER,PLACE,start,2");

            // set the player's name to whatever he used to log in
            player.SetName(command);
            // tell the model the player is logged in
            player.AddImmediateCommand("LOGIN,COMPLETE");

            // look around
            addLook(true, true, player);
        }

        // handle "normal" logins. This should probably be split at some point, like the actionhandlers in
        // the model, but for now there aren't many commands.
        private static void HandleNormal(String command, Player player)
        {
            // something is a movement command if it can be parsed to a direction
            bool isMovementCommand = Directions.FromShortString(command) > -1 || Directions.FromString(command) > -1;

            // if something is a movement command, move the player in the appropriate direction
            if (isMovementCommand)
            {
                // we know it's a movement command. Try to parse as short string, if
                // unsuccesful, parse as long string
                int direction = Directions.FromShortString(command);
                if (direction == -1) direction = Directions.FromString(command);

                player.AddBlockingCommand("MOVE," + direction);

                addLook(true, true, player);
            } // look if the command is to look
            else if (command.Equals("l") || command.Equals("look"))
            {
                addLook(false, true, player);
            }
            else if (command.StartsWith("say"))
            {
                // split the string in at most three pieces
                string[] splittedString = command.Split(new char[]{' '},3);
               
                // no recipient specified. Assume it was meant for everybody
                if (splittedString.Length == 2)
                {
                    player.AddImmediateCommand("SAY,ALL," + splittedString[1]);
                }
                // recipient specified
                else if (splittedString.Length == 3)
                {
                    // meant for everyone
                    if (splittedString[1].Equals("all"))
                    {
                        player.AddImmediateCommand("SAY,ALL," + splittedString[2]);

                    }
                    // meant for a specific recipient
                    else
                    {
                        player.AddImmediateCommand("SAY," + splittedString[1] + "," + splittedString[2]);
                    }
                }
            }
        }

        private static void addLook(bool includeTiles, bool includePlayer, Player player)
        {
            player.AddImmediateCommand("LOOK,TILES_" + ((includeTiles) ? "INCLUDED" : "EXCLUDED") + ",PLAYER_" + ((includePlayer) ? "INCLUDED" : "EXCLUDED"));
        }
    }
}