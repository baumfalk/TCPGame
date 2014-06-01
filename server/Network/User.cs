using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.IO;

using TCPGameServer.World;
using TCPGameServer.Network.InputHandling;

namespace TCPGameServer.Network
{
    public class User
    {
        private Controller control;

        private TcpClient client;

        private InputHandler handler;
        private Player player;

        private Queue<String> messageQueue;

        private String inputBuffer;

        public User(Controller control, TcpClient client)
        {
            this.control = control;

            this.client = client;

            messageQueue = new Queue<String>();

            Creature playerBody = new Creature("player");

            player = new Player(playerBody);

            player.setCommandState(Player.COMMANDSTATE_LOGIN);

            handler = new InputHandler(player);

            addMessage("LOGIN,MESSAGE,please input your character name");

            startReading();
        }

        public bool isConnected()
        {
            bool connected = client.Connected;

            if (!connected)
            {
                player.SetDisconnected(true);

                control.outputMessage("user " + client.Client.RemoteEndPoint.ToString() + " has disconnected");
            }
            return client.Connected;
        }

        private void startReading()
        {
            Stream stream = client.GetStream();

            try
            {
                byte[] buffer = new byte[1024];

                // begin reading. When data arrives, call dataReceived
                stream.BeginRead(buffer, 0, 1024, dataReceived, buffer);
            }
            catch (IOException e)
            {
                control.outputMessage("can't begin reading from stream of user " + client.Client.RemoteEndPoint.ToString());
                control.outputMessage(e.Message);
            }
        }

        // gets called when data is received
        private void dataReceived(IAsyncResult data)
        {
            NetworkStream stream = client.GetStream();

            // a data buffer
            byte[] dataBuffer = new byte[1024];

            // finish the asynchronous reading
            try
            {
                // since this method was called, there's some data in the buffer
                dataBuffer = (byte[])data.AsyncState;
                // number of bytes read
                int numBytes = stream.EndRead(data);

                // add the new data to the message buffer
                inputBuffer = String.Concat(inputBuffer, Encoding.ASCII.GetString(dataBuffer, 0, numBytes));

                // if there's still available data, read it as well with another call.
                if (stream.DataAvailable)
                {
                    startReading();
                }
                else
                {
                    // otherwise split the received data into commands and pass it on
                    splitAndHandle();
                }
            }
            catch (IOException e)
            {
                control.outputMessage("can't read during dataReceived for user " + client.Client.RemoteEndPoint.ToString());
                control.outputMessage(e.Message);
            }
        }

        // data should be sent with separate lines separated by semicolons. It will be passed
        // to the controller as a list.
        private void splitAndHandle()
        {
            // split the message into separate strings
            String[] splitMessage = inputBuffer.Split(';');

            // put them in a list
            List<String> inputList = new List<String>(splitMessage);

            // size of the list
            int size = inputList.Count;

            // if the last character is a semicolon, we'll have an empty string as "overflow",
            // if it's not, there will be actual overflow in the message string, which will be
            // concatenated to in the next pass
            inputBuffer = inputList[size - 1];

            // cut off the last member of the list (since it's the overflow)
            inputList = inputList.GetRange(0, size - 1);

            // send the updates to the input handler
            handler.Handle(inputList);

            // get ready for a new batch of data from the server
            startReading();
        }

        public void addMessage(String message)
        {
            messageQueue.Enqueue(message);
        }

        public void sendMessages()
        {
            if (messageQueue.Count == 0) return;

            NetworkStream stream = client.GetStream();

            String message = MessageFormatting.formatCollection(messageQueue);

            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            control.outputMessage("sending " + message + "(" + message.Length + ") to client at " + client.Client.RemoteEndPoint.ToString());
            {
                try
                {
                    stream.BeginWrite(messageInBytes, 0, messageInBytes.Length, messageSent, null);
                    
                }
                catch (IOException e)
                {
                    control.outputMessage("exception trying to begin write to " + client.Client.RemoteEndPoint.ToString());
                    control.outputMessage(e.Message);
                }
            }
        }

        private void messageSent(IAsyncResult sent)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                stream.EndWrite(sent);
            }
            catch (IOException e)
            {
                control.outputMessage("exception trying to end write to " + client.Client.RemoteEndPoint.ToString());
                control.outputMessage(e.Message);
            }
        }
    }
}
