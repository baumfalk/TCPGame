using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

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
        private Queue<String> blockingCommands;

        // a queue of immediate commands (like getting inventory information, which can be
        // done at any time, even when blocking commands are queued). You may want to limit
        // the number of immediate commands that will be handled in a tick, but in theory
        // they should just be handled and the results returned on the next tick.
        private Queue<String> immediateCommands;

        public Player(Creature body)
        {
            this.body = body;

            blockingCommands = new Queue<String>();
            immediateCommands = new Queue<String>();
        }

        public void addBlockingCommand(String command)
        {
            blockingCommands.Enqueue(command);
        }

        public bool hasNextBlockingCommand()
        {
            return blockingCommands.Count > 0;
        }

        public String getNextBlockingCommand()
        {
            if (hasNextBlockingCommand())
            {
                return blockingCommands.Dequeue();
            }
            else
            {
                return "";
            }
        }

        public void addImmediateCommand(String command)
        {
            immediateCommands.Enqueue(command);
        }

        public int immediateCommandCount()
        {
            return immediateCommands.Count;
        }

        public bool hasImmediateCommands()
        {
            return (immediateCommandCount() > 0);
        }

        public String getNextImmediateCommand()
        {
            if (hasImmediateCommands())
            {
                return immediateCommands.Dequeue();
            }
            else
            {
                return "";
            }
        }

        // body can be requested, but not changed
        public Creature getBody()
        {
            return body;
        }
    }
}
