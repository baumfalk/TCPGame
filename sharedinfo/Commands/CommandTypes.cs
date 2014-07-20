using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Commands
{
    public enum ServerCommandType
    {
        // commands that the server should be able to handle from the client during login
        Name, Password,

        // commands that the server should be able to receive from the client at any time,
        // which deal directly with server functions, not with a specific player
        Shutdown, Reset, Log, Quit,

        // commands that the server should be able to receive from the client after login
        // which deal with actions the player can take
        Move, Look, Say, Whisper, Teleport, TileData
    };

    public enum ClientCommandType
    {
        // commands that the client should be able to handle internally
        Connect, Zoom ,

        // commands that the client should be able to handle from the server during login
        NameRequest, InvalidName, PasswordRequest, WrongPassword ,

        // commands that the client should be able to handle from the server after login
        Quit, Message, Login, Player, Tile, Wholist 
    };
}
