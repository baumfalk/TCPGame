﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // a queue of messages to send to the client, queried by the user linked to this
        // player object every tick.
        private Queue<String> messages;

        // commandState to toggle which commands are legitimate and which are not. (For
        // example: you want to have some sort of login, where direction commands and the
        // like shouldn't be accepted)
        private int commandState = 0;

        public const int COMMANDSTATE_IDLE = 0;
        public const int COMMANDSTATE_LOGIN = 1;
        public const int COMMANDSTATE_NORMAL = 2;

        private bool disconnected = false;

        private bool moved = false;

        public Player(Creature body)
        {
            this.body = body;

            body.setPlayer(this);

            blockingCommands = new Queue<String>();
            immediateCommands = new Queue<String>();
            messages = new Queue<String>();
        }

        public bool hasMoved()
        {
            return moved;
        }

        public void setMoved(bool moved)
        {
            this.moved = moved;
        }

        public bool isDisconnected()
        {
            return disconnected;
        }

        public void SetDisconnected(bool disconnected)
        {
            this.disconnected = disconnected;
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
            if (!Network.Controller.headless) ServerOutputWindow.Print("adding immediate command: " + command);
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

        public void addMessage(String message)
        {
            messages.Enqueue(message);
        }

        public bool hasMessages()
        {
            return (messages.Count > 0);
        }

        public String getMessage()
        {
            if (hasMessages())
            {
                return messages.Dequeue();
            }
            else
            {
                return "";
            }
        }

        public void setCommandState(int commandState)
        {
            this.commandState = commandState;
        }

        public int getCommandState()
        {
            return commandState;
        }

        // body can be requested, but not changed
        public Creature getBody()
        {
            return body;
        }
    }
}
