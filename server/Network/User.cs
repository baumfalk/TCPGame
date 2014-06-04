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
        private NetworkStream stream;
        private String remoteIP;

        private InputHandler handler;
        private Player player;

        private Queue<String> messageQueue;

        private String inputBuffer;

        private bool connected;

        public User(Controller control, TcpClient client)
        {
            this.control = control;

            this.client = client;
            stream = client.GetStream();
            remoteIP = client.Client.RemoteEndPoint.ToString();

            connected = true;

            inputBuffer = "";

            messageQueue = new Queue<String>();

            Creature playerBody = new Creature("player");

            player = new Player(playerBody);

            player.setCommandState(Player.COMMANDSTATE_LOGIN);

            handler = new InputHandler(player);

            addMessage("LOGIN,MESSAGE,please input your character name");

            startReading();

            control.registerPlayer(player);
        }

        public bool isConnected()
        {
            return connected;
        }

        public void Disconnect()
        {
            client.Close();

            player.SetDisconnected(true);

            connected = false;

            Controller.Print("user " + remoteIP + " has disconnected");
        }

        private void startReading()
        {
            // if the client isn't connected, don't read.
            if (!connected) return;

            try
            {
                byte[] buffer = new byte[1024];

                // begin reading. When data arrives, call dataReceived
                stream.BeginRead(buffer, 0, 1024, dataReceived, buffer);
            }
            catch (IOException e)
            {
                Controller.Print("can't begin reading from stream of user " + remoteIP);
                Controller.Print(e.Message);

                if (connected) Disconnect();
            }
        }

        // gets called when data is received
        private void dataReceived(IAsyncResult data)
        {
            // if the client isn't connected, we can't receive anything.
            if (!connected) return;

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

                // and resume reading
                startReading();
            }
            catch (IOException e)
            {
                Controller.Print("can't read during dataReceived for user " + remoteIP);
                Controller.Print(e.Message);

                if (connected) Disconnect();
            }
        }

        // data should be sent with separate lines separated by semicolons. It will be passed
        // to the controller as a list.
        public void HandleInput()
        {
            // no data = no handling
            if (inputBuffer.Equals("")) return;

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
        }

        public void Remove()
        {
            if (connected) Disconnect();
        }

        public void addMessage(String message)
        {
            messageQueue.Enqueue(message);
        }

        public void sendMessages(int tick)
        {
            while (player.hasMessages())
            {
                messageQueue.Enqueue(player.getMessage());
            }

            if (!Controller.headless) Controller.Print("messageQueue count = " + messageQueue.Count);

            if (messageQueue.Count == 0) return;

            String message = "(" + tick + ");" + MessageFormatting.formatCollection(messageQueue);

            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            if (!Controller.headless) Controller.Print("sending " + message + "(" + message.Length + ") to client at " + remoteIP);

            // if the client isn't connected, don't send.
            if (!connected) return;
            try
            {
                stream.BeginWrite(messageInBytes, 0, messageInBytes.Length, messageSent, null);

            }
            catch (IOException e)
            {
                Controller.Print("exception trying to begin write to " + remoteIP);
                Controller.Print(e.Message);

                if (connected) Disconnect();
            }
            
        }

        private void messageSent(IAsyncResult sent)
        {
            // if the client isn't connected, don't call endWrite.
            if (!connected) return;

            try
            {
                stream.EndWrite(sent);
            }
            catch (IOException e)
            {
                Controller.Print("exception trying to end write to " + remoteIP);
                Controller.Print(e.Message);

                if (connected) Disconnect();
            }
        }
    }
}
