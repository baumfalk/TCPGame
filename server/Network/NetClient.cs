using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.IO;

using TCPGameServer.Control;
using TCPGameServer.Control.IO;

namespace TCPGameServer.Network
{
    public class NetClient
    {
        // a client, the client's network stream, and the IP of the remote end
        private TcpClient client;
        private NetworkStream stream;
        private String remoteIP;

        // holds data that doesn't end with a semicolon until more data is sent which will
        // complete the command.
        // TODO: make cyclic extendable byte buffer
        private String inputBuffer;

        // signifies the connection has been lost.
        private bool connected;

        // initializes and starts reading data
        public NetClient(TcpClient client)
        {
            // fill fields with relevant data
            this.client = client;
            stream = client.GetStream();
            remoteIP = client.Client.RemoteEndPoint.ToString();

            // hopefully we're connected when the client is created
            connected = true;

            // nothing has come in yet
            inputBuffer = "";

            // start asynchronous read
            StartReading();
        }

        // we're connected when the connected flag is set.
        public bool IsConnected()
        {
            return connected;
        }

        // disconnect the client
        public void Disconnect()
        {
            // close the TCP client
            client.Close();

            // set the flag to false
            connected = false;

            // put the disconnect on the output log
            Output.Print("client " + remoteIP + " has disconnected");
        }

        // asynchronous read
        private void StartReading()
        {
            // if the client isn't connected, don't read.
            if (!connected) return;

            try
            {
                byte[] buffer = new byte[1024];

                // begin reading. When data arrives, call dataReceived. First 1024 bytes of data
                // will be in the buffer created here. It is passed in the final argument.
                stream.BeginRead(buffer, 0, 1024, DataReceived, buffer);
            }
            catch (IOException e)
            {
                Output.Print("can't begin reading from stream of user " + remoteIP);
                Output.Print(e.Message);

                if (connected) Disconnect();
            }
        }

        // gets called when data is received
        private void DataReceived(IAsyncResult data)
        {
            // if the client isn't connected, we can't receive anything.
            if (!connected) return;

            // a data buffer, size is pretty arbitrary
            byte[] dataBuffer = new byte[1024];

            // finish the asynchronous read
            try
            {
                // since this method was called, there's some data in the buffer
                dataBuffer = (byte[])data.AsyncState;
                // number of bytes read
                int numBytes = stream.EndRead(data);

                // add the new data to the message buffer
                inputBuffer = String.Concat(inputBuffer, Encoding.ASCII.GetString(dataBuffer, 0, numBytes));

                // and resume reading
                StartReading();
            }
            catch (IOException e)
            {
                Output.Print("can't read during dataReceived for user " + remoteIP);
                Output.Print(e.Message);

                if (connected) Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                Output.Print("client was disposed while reading for user " + remoteIP);
                Output.Print(e.Message);
            }
        }

        public List<String> GetInput()
        {
            // split the message into separate strings
            String[] splitMessage = inputBuffer.Split(';');

            // put them in a list
            List<String> inputList = new List<String>(splitMessage);

            // size of the list
            int size = inputList.Count;

            if (size > 0)
            {
                // if the last character is a semicolon, we'll have an empty string as "overflow",
                // if it's not, there will be actual overflow in the message string, which will be
                // concatenated to in the next pass
                inputBuffer = inputList[size - 1];

                // cut off the last member of the list (since it's the overflow)
                inputList = inputList.GetRange(0, size - 1);
            }

            return inputList;
        }

        // disconnect if we're still connected.
        public void Remove()
        {
            if (connected) Disconnect();
        }

        // format a queue of strings and send them across the link
        public void SendMessages(Queue<String> messagesFromModel)
        {
            // if we have no messages to send, don't try sending messages.
            if (messagesFromModel.Count == 0) return;

            // format the queue to a string with semicolons after each item. Prefix with the tick they were
            // sent.
            String message = MessageFormatting.FormatMessageQueue(messagesFromModel);

            // convert the message to a byte array
            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

            // if the client isn't connected, don't send.
            if (!connected) return;
            try
            {
                // asynchronous write, we're just dumping the entire message at once, and don't bother with
                // limiting block sizes.
                stream.BeginWrite(messageInBytes, 0, messageInBytes.Length, MessageSent, null);
            }
            catch (IOException e)
            {
                Output.Print("exception trying to begin write to " + remoteIP);
                Output.Print(e.Message);

                if (connected) Disconnect();
            }
            
        }

        private void MessageSent(IAsyncResult sent)
        {
            // if the client isn't connected, don't call endWrite.
            if (!connected) return;

            try
            {
                // end the asynchronous write when done.
                stream.EndWrite(sent);
            }
            catch (IOException e)
            {
                Output.Print("exception trying to end write to " + remoteIP);
                Output.Print(e.Message);

                if (connected) Disconnect();
            }
            catch (SocketException e)
            {
                Output.Print("object disposed on write");
                Output.Print(e.Message);

                if (connected) Disconnect();
            }
        }
    }
}
