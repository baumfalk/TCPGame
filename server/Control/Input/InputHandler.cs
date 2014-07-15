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
                    user.Disconnect();
                }
                else
                {
                    // switch on commandstate of the user. We don't want players
                    // to be able to move before logging in, or similar things. Send
                    // the command on to the proper method.
                    switch (user.GetLoginState())
                    {
                        case User.LoginState.NotStarted:
                            HandleNotStarted(command, user);
                            break;
                        case User.LoginState.Login:
                            HandleLogin(command, user);
                            break;
                        case User.LoginState.Password:
                            HandlePassword(command, user);
                            break;
                        case User.LoginState.Finished:
                            HandleNormal(command, player);
                            break;
                    }
                }
            }
        }

        // Users shouldn't be able to send anything while idle. Just in case, it will generate
        // a message to the log.
        private static void HandleNotStarted(String command, User user)
        {
            Log.Print("A user is idle but sending commands");
        }

        private static void HandleLogin(String command, User user)
        {
            // nothing but letters allowed in names
            Regex rgx = new Regex("[^a-zA-Z]");
            command = rgx.Replace(command, "").ToLower();

            // don't accept empty string as a name
            if (command.Equals(""))
            {
                user.AddMessage("MESSAGE,LOGIN,Name invalid", int.MinValue);
            }
            else
            {
                // save the name in the login info
                user.GetLoginInfo().name = command;

                HeaderData header;

                // if a player file exists, and it's not a stub, ask for the password and read the header to get
                // the salt
                if (PlayerFile.Exists(command) && !PlayerFile.IsStub(command))
                {
                    user.AddMessage("MESSAGE,LOGIN,Please input your password", int.MinValue);

                    header = PlayerFile.ReadHeader(command);

                    user.GetLoginInfo().newUser = false;
                    user.GetLoginInfo().salt = header.salt;

                    user.SetLoginState(User.LoginState.Password);
                }
                else
                {
                    user.GetLoginInfo().newUser = true;
                    user.GetLoginInfo().salt = PasswordHashing.generateSalt();

                    user.AddMessage("MESSAGE,LOGIN,New account " + command + ", please input a password", int.MinValue);

                    user.SetLoginState(User.LoginState.Password);
                }

                // send the salt
                user.AddMessage("LOGIN,SALT," + Convert.ToBase64String(user.GetLoginInfo().salt), int.MinValue);
            }
        }

        private static void HandlePassword(String command, User user)
        {
            String name = user.GetLoginInfo().name;

            if (!user.GetLoginInfo().newUser)
            {
                HeaderData header = PlayerFile.ReadHeader(name);

                String password = header.password;

                if (PasswordHashing.VerifyPassword(command, password))
                {
                    user.GetLoginInfo().areaName = header.area;
                    user.GetLoginInfo().tileIndex = header.tileIndex.ToString();

                    user.CompleteLogin();
                }
                else
                {
                    user.AddMessage("MESSAGE,LOGIN,Password incorrect, please try again", int.MinValue);
                    user.AddMessage("LOGIN,SALT," + Convert.ToBase64String(header.salt), int.MinValue);
                }
            }
            else
            {
                PlayerFileData playerFile = CreatePlayerFile(name, user.GetLoginInfo().salt);

                playerFile.header.password = PasswordHashing.Rehash(command);

                PlayerFile.Write(playerFile, name);

                user.GetLoginInfo().areaName = playerFile.header.area;
                user.GetLoginInfo().tileIndex = playerFile.header.tileIndex.ToString();

                user.CompleteLogin();
            }
        }

        private static PlayerFileData CreatePlayerFile(String name, byte[] salt)
        {
            PlayerFileData playerFile = new PlayerFileData();

            playerFile.header = new HeaderData();
            playerFile.header.name = name;
            playerFile.header.salt = salt;
            playerFile.header.password = "";
            playerFile.header.area = "x0y0z0";
            playerFile.header.tileIndex = 12;

            PlayerFile.Write(playerFile, name);

            return playerFile;
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
                player.AddBlockingCommand(new String[] { "LOOK", "TILES_INCLUDED", "PLAYER_INCLUDED", "REGISTER_NONE" });
            }
            else if (command.ToLower().StartsWith("say")) // format: say *message*
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

                if (splittedString.Length == 2) player.AddBlockingCommand(new String[] { "TELEPORT", player.GetBody().GetPosition().GetArea().GetName(), splittedString[1] });
                if (splittedString.Length == 3) player.AddBlockingCommand(new String[] { "TELEPORT", splittedString[1], splittedString[2] });
            }
        }
    }
}