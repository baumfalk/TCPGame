using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World
{
    // a player is an object registered to a user on the server, which tells the ticker
    // which body is linked to it and which maintains a blocking and a non-blocking command
    // buffer.
    public class Player
    {
        // a body
        private Creature body;

        // a queue of blocking commands (like walking, we don't want players to move infinite
        // distances in zero time if they just send enough commands at once.)
        private Queue<String[]> blockingCommands;

        // a queue of immediate commands (like getting inventory information, which can be
        // done at any time, even when blocking commands are queued). You may want to limit
        // the number of immediate commands that will be handled in a tick, but in theory
        // they should just be handled and the results returned on the next tick.
        private Queue<String[]> immediateCommands;

        // a queue of messages to send to the client, queried by the user linked to this
        // player object every tick.
        private Queue<String> messages;

        // commandState to toggle which commands are legitimate and which are not. (For
        // example: you want to have some sort of login, where direction commands and the
        // like shouldn't be accepted)
        private int commandState = 0;

        // constants for the different command states
        public const int COMMANDSTATE_IDLE = 0;
        public const int COMMANDSTATE_LOGIN = 1;
        public const int COMMANDSTATE_NORMAL = 2;
        public const int COMMANDSTATE_DISCONNECTED = 3;

        // flag to show a player is disconnected
        private bool disconnected = false;

        // unique identifier
        private string name = "anon";

        public Player(Creature body)
        {
            this.body = body;

            body.SetPlayer(this);

            blockingCommands = new Queue<String[]>();
            immediateCommands = new Queue<String[]>();
            messages = new Queue<String>();
        }

        public void Remove()
        {
            body.SetPlayer(null);
            if (body.GetPosition() != null)
            {
                body.GetPosition().Vacate();
            }
            SetDisconnected(true);
        }

        public bool IsDisconnected()
        {
            return disconnected;
        }

        public void SetDisconnected(bool disconnected)
        {
            this.disconnected = disconnected;

            if (disconnected) commandState = COMMANDSTATE_DISCONNECTED;
            else commandState = COMMANDSTATE_NORMAL;
        }

        public void AddBlockingCommand(String[] cmdAndParameters)
        {
            Output.Print("(" + name + ") adding blocking command: " + cmdAndParameters);
            blockingCommands.Enqueue(cmdAndParameters);
        }

        public bool HasNextBlockingCommand()
        {
            return blockingCommands.Count > 0;
        }

        public String[] GetNextBlockingCommand()
        {
            if (HasNextBlockingCommand())
            {
                return blockingCommands.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void AddImmediateCommand(String [] cmdAndParameters)
        {
            Output.Print("(" + name + ") adding immediate command: " + cmdAndParameters[0]);
            immediateCommands.Enqueue(cmdAndParameters);
        }

        public bool HasImmediateCommands()
        {
            return (immediateCommands.Count > 0);
        }

        public String[] GetNextImmediateCommand()
        {
            if (HasImmediateCommands())
            {
                return immediateCommands.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void AddMessage(String message, int tick)
        {
            messages.Enqueue(tick + "," + message);
        }

        public bool HasMessages()
        {
            return (messages.Count > 0);
        }

        public Queue<String> GetMessages()
        {
            return messages;
        }

        public void SetCommandState(int commandState)
        {
            this.commandState = commandState;
        }

        public int GetCommandState()
        {
            return commandState;
        }

        // body can be requested, but not changed
        public Creature GetBody()
        {
            return body;
        }

        public void SetName(string name)
        {
            if (name != "") this.name = name;
        }

        public string GetName()
        {
            return name;
        }
    }
}
