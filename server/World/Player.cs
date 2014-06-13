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
        public const int COMMANDSTATE_PLACEMENT = 2;
        public const int COMMANDSTATE_NORMAL = 3;
        public const int COMMANDSTATE_DISCONNECTED = 4;

        // flag to show a player is disconnected
        private bool disconnected = false;

        // blocking queue delay
        private int blockDelay = 0;

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

        // remove the player from the world
        public void Remove()
        {
            // if the player was on the map, remove him
            if (body.GetPosition() != null)
            {
                body.GetPosition().Vacate();
            }
            // set the player's disconnected state to true
            SetDisconnected(true);
        }

        // disconnected state can be set and retrieved
        public void SetDisconnected(bool disconnected)
        {
            this.disconnected = disconnected;

            // if disconnected, set the commandstate to disconnected, if not,
            // set it back to normal
            if (disconnected) commandState = COMMANDSTATE_DISCONNECTED;
            else commandState = COMMANDSTATE_NORMAL;
        }
        public bool IsDisconnected()
        {
            return disconnected;
        }

        // blocking delay is handled before the next item in the blocking command
        // queue. Allows for commands to take longer than a single tick.
        public void AddBlockingDelay(int ticks)
        {
            blockDelay += ticks;
        }

        // blocking commands are handled one per tick. If blockDelay is positive, a delay
        // command is handled instead of the command in the queue.
        public bool HasNextBlockingCommand()
        {
            // a blocking command is available if there are commands in the queue,
            // blockdelay is positive, or both
            return blockingCommands.Count + blockDelay > 0;
        }
        public void AddBlockingCommand(String[] cmdAndParameters)
        {
            // put the command in the queue
            blockingCommands.Enqueue(cmdAndParameters);
        }
        public String[] GetNextBlockingCommand()
        {
            // if the blockDelay is positive, return a delay command
            if (blockDelay > 0)
            {
                blockDelay--;
                return new String[] { "DELAY" };
            }

            // otherwise return the next command in the queue. Return null if no such
            // command exists, although it should never happen
            if (HasNextBlockingCommand())
            {
                return blockingCommands.Dequeue();
            }
            else
            {
                Output.Print("(" + name + ") game tried to dequeue command while blocking queue was empty");

                return null;
            }
        }


        // immediate commands are all handled each tick. 
        public bool HasImmediateCommands()
        {
            // simply check if there are items in the queue
            return (immediateCommands.Count > 0);
        }
        public void AddImmediateCommand(String [] cmdAndParameters)
        {
            // add the command from the queue
            immediateCommands.Enqueue(cmdAndParameters);
        }
        public String[] GetNextImmediateCommand()
        {
            // return the next command in the queue. Return null if no such
            // command exists, although it should never happen
            if (HasImmediateCommands())
            {
                return immediateCommands.Dequeue();
            }
            else
            {
                Output.Print("(" + name + ") game tried to dequeue command while immediate queue was empty");

                return null;
            }
        }

        // messages are sent across the connection to the player
        public bool HasMessages()
        {
            return (messages.Count > 0);
        }
        public void AddMessage(String message, int tick)
        {
            messages.Enqueue(tick + "," + message);
        }
        public Queue<String> GetMessages()
        {
            return messages;
        }

        // command state regulates which commands are valid at any time. The constants
        // at the top of this file should be used.
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

        // sets or gets the player's name
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
