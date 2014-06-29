using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TCPGameServer.World;
using TCPGameServer.World.Players;
using TCPGameServer.World.Players.PlayerFiles;

using TCPGameServer.Control;
using TCPGameServer.Control.Output;

namespace TCPGameServer.Control.Input
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
                // shutdown and log commands can be given irrespective of state. At
                // some point these need to be behind the login, but not during
                // development
                if (command.Equals("shutdown"))
                {
                    Log.Print("Shutdown command received...");

                    // unset the running flag, server will shut down on the next cycle
                    Controller.Stop();
                }
                else if (command.Equals("reset"))
                {
                    // reset the map
                    player.AddImmediateCommand(new String[] { "RESET" });
                }
                else if (command.Equals("log"))
                {
                    Log.Print("Sending log to user");

                    // don't want to iterate over a collection in use, so copy it
                    String[] log = Log.GetLog().ToArray();

                    foreach (string message in log)
                    {
                        // sent with minvalue as tick argument, since it never needs to
                        // be handled by the client.
                        player.AddMessage("MESSAGE,SERVER,LOG: " + message, int.MinValue);
                    }
                } // quit command
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
                        case Player.CommandState.Idle:
                            HandleIdle(command, player);
                            break;
                        case Player.CommandState.Login:
                            HandleLogin(command, player);
                            break;
                        case Player.CommandState.Password:
                            HandlePassword(command, player);
                            break;
                        case Player.CommandState.Normal:
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
            Log.Print(player.GetName() + " is idle but sending commands");
        }

        // TODO: make login dynamic, reading from a file
        private static void HandleLogin(String command, Player player)
        {
            // nothing but letters allowed in names
            Regex rgx = new Regex("[^a-zA-Z]");
            command = rgx.Replace(command, "");

            // set the player's name to whatever he used to log in
            player.SetName(command);

            if (command.Equals("admin"))
            {
                DoLogin(player, "x0y0z0", "0");
            }

            HeaderData header;

            if (PlayerFile.Exists(command) && !PlayerFile.IsStub(command))
            {
                player.AddMessage("MESSAGE,LOGIN,Please input your password", int.MinValue);
                
                header = PlayerFile.ReadHeader(command);

                player.SetCommandState(Player.CommandState.Password);
            }
            else
            {
                PlayerFileData newStub = CreateStub(command);

                header = newStub.header;

                player.AddMessage("MESSAGE,LOGIN,New account " + command + ", please input a password", int.MinValue);

                player.SetCommandState(Player.CommandState.Password);
            }

            // salt opsturen
            player.AddMessage("LOGIN,SALT," + Convert.ToBase64String(header.salt), int.MinValue);
        }

        private static void HandlePassword(String command, Player player)
        {
            if (!PlayerFile.IsStub(player.GetName()))
            {
                HeaderData header = PlayerFile.ReadHeader(player.GetName());

                String password = header.password;

                if (PasswordHashing.VerifyPassword(password, command))
                {
                    DoLogin(player, header.area, header.tileIndex.ToString());
                }
                else
                {
                    player.AddMessage("MESSAGE,LOGIN,Password incorrect, please try again", int.MinValue);
                    player.AddMessage("LOGIN,SALT," + Convert.ToBase64String(header.salt), int.MinValue);
                }
            }
            else
            {
                PlayerFileData playerFile = PlayerFile.Read(player.GetName());

                playerFile.header.password = command;

                PlayerFile.Write(playerFile, player.GetName());

                DoLogin(player, playerFile.header.area, playerFile.header.tileIndex.ToString());
            }
        }

        private static PlayerFileData CreateStub(String name)
        {
            PlayerFileData playerFile = new PlayerFileData();

            playerFile.header = new HeaderData();
            playerFile.header.name = name;
            playerFile.header.salt = PasswordHashing.generateSalt();
            playerFile.header.password = "";
            playerFile.header.area = "x0y0z0";
            playerFile.header.tileIndex = 12;

            PlayerFile.Write(playerFile, name);

            return playerFile;
        }

        private static void DoLogin(Player player, String area, String tileIndex)
        {
            player.AddImmediateCommand(new String[] { "PLAYER", "PLACE", area, tileIndex });
            player.AddImmediateCommand(new String[] { "LOGIN", "COMPLETE" });
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

                player.AddBlockingCommand(new String[] { "MOVE", "" + direction });
            } // look if the command is to look
            else if (command.ToLower().Equals("l") || command.ToLower().Equals("look"))
            {
                player.AddBlockingCommand(new String[] { "LOOK", "TILES_INCLUDED", "PLAYER_INCLUDED" });
            }
            else if (command.ToLower().StartsWith("say")) // formay say *message*
            {
                // split the string in two
                string[] splittedString = command.Split(new char[] { ' ' }, 2);
               
                // invalid command
                if (splittedString.Length < 2) return;

                player.AddImmediateCommand(new String[] { "SAY", splittedString[1] });
            }
            else if (command.ToLower().StartsWith("whisper") || command.ToLower().StartsWith("tell")) // format: whisper *recipient* *message*
            {
                // split the string in three
                string[] splittedString = command.Split(new char[] { ' ' }, 3);

                // invalid command
                if (splittedString.Length < 3) return;

                player.AddImmediateCommand(new String[] { "WHISPER", splittedString[1], splittedString[2] });
            }
            else if (command.ToLower().Equals("tiledata"))
            {
                TCPGameServer.World.Map.Tile tile = player.GetBody().GetPosition();

                String output = "MESSAGE,TILEDATA," +
                    "tile ID = " + tile.GetID() + "\r\n" +
                    "tile links: " + tile.GetLinkText(Directions.NORTH) + ", " +
                                     tile.GetLinkText(Directions.EAST) + ", " +
                                     tile.GetLinkText(Directions.UP) + ", " +
                                     tile.GetLinkText(Directions.SOUTH) + ", " +
                                     tile.GetLinkText(Directions.WEST) + ", " +
                                     tile.GetLinkText(Directions.DOWN) + "\r\n" +
                    "tile location: " + tile.GetLocation();


                player.AddMessage(output.Replace(";", ":"), int.MinValue);
            }
            else if (command.ToLower().StartsWith("go"))
            {
                string[] splittedString = command.Split(' ');

                if (splittedString.Length == 2) player.AddBlockingCommand(new String[] { "MOVE", "TELEPORT", player.GetBody().GetPosition().GetArea().GetName(), splittedString[1] });
                if (splittedString.Length == 3) player.AddBlockingCommand(new String[] { "MOVE", "TELEPORT", splittedString[1], splittedString[2] });
            }
        }
    }
}