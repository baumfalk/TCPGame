using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo
{
    // commands that the server should be able to handle from the client during login
    public enum ServerLoginCommandTypes { Name, Password };

    // commands that the server should be able to receive from the client at any time,
    // which deal directly with server functions, not with a specific player
    public enum ServerCommandTypes { Shutdown, Reset, Log, Quit };

    // commands that the server should be able to receive from the client after login
    // which deal with actions the player can take
    public enum ActionCommandTypes { Move, Look, Say, Whisper, Teleport, TileData };

    // commands that the client should be able to handle internally
    public enum InternalClientCommandTypes { Connect, Zoom };

    // commands that the client should be able to handle from the server during login
    public enum ClientLoginCommandTypes { NameRequest, InvalidName, PasswordRequest, WrongPassword }

    // commands that the client should be able to handle from the server after login
    public enum ClientCommandTypes { Quit, Message, Login, Player, Tile, Wholist };
}
