using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;

using System.Diagnostics;

namespace TCPGameClient.Control
{
    class NetConnector
    {
        // the client controller
        private Controller control;
        // a TCP client we'll connect to the server
        private TcpClient client;
        // the client's networkstream
        private NetworkStream stream;

        // set to false to stop sending/receiving data and disconnect
        private Boolean bRunning;

        // we may not get full messages, if we get parts they go in here.
        private String message = "";

        IPAddress IP;
        int port;

        public NetConnector(Controller control, String IP, int port)
        {
            // assign controller
            this.control = control;

            // start running
            bRunning = true;

            // create the client
            client = new TcpClient();

            this.IP = IPAddress.Parse(IP);
            this.port = port;
        }

        public void Connect()
        {
            try
            {
                // connect the client to the server. connectionMade will be called when connected
                client.BeginConnect(IP, port, new AsyncCallback(connectionMade), null);
            }
            catch (IOException e)
            {
                Debug.Print("exception when trying to connect");
                Debug.Print(e.Message);

                Disconnect();
            }
        }

        // disconnect the client
        public void Disconnect()
        {
            bRunning = false;
        }

        // when a connection has been made we start listening for incoming data. The client is
        // fairly passive and waits for the server to initiate contact
        private void connectionMade(IAsyncResult newConnection)
        {
            try
            {
                // finish connecting
                client.EndConnect(newConnection);
            }
            catch (IOException e)
            {
                Debug.Print("exception on finishing connection");
                Debug.Print(e.Message);

                Disconnect();
            }

            // set stream to be the client's networkstream
            stream = client.GetStream();

            // start listening for data
            startReading();
        }

        // starts listening for data if brunning is true, otherwise it closes the connection
        private void startReading()
        {
            if (bRunning)
            {
                try
                {
                    byte[] buffer = new byte[1024];

                    // begin reading. When data arrives, call dataReveived
                    stream.BeginRead(buffer, 0, 1024, dataReceived, buffer);
                }
                catch (IOException e)
                {
                    Debug.Print("can't begin reading from stream");
                    Debug.Print(e.Message);

                    Disconnect();
                }
            }
            else
            {
                // close the client
                client.Close();
            }
        }

        // gets called when data is received
        private void dataReceived(IAsyncResult data)
        {
            // if the disconnect command has been received between starting to listen and
            // getting data, we don't handle it but close the client and return
            if (!bRunning)
            {
                client.Close();
                return;
            }

            // a data buffer
            byte[] dataBuffer = new byte[1024];

            // finish the asynchronous reading
            try
            {
                // since this method was called, there's some data in the buffer
                dataBuffer = (byte[]) data.AsyncState;
                // number of bytes read
                int numBytes = stream.EndRead(data);

                // add the new data to the message buffer
                message = String.Concat(message, Encoding.ASCII.GetString(dataBuffer, 0, numBytes));

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
                Debug.Print("can't read during dataReceived");
                Debug.Print(e.Message);
            }
        }

        // data should be sent with separate lines separated by semicolons. It will be passed
        // to the controller as a list.
        private void splitAndHandle()
        {
            // split the message into separate strings
            String[] splitMessage = message.Split(';');

            // put them in a list
            List<String> messageList = new List<String>(splitMessage);

            // size of the list
            int size = messageList.Count;

            // if the last character is a semicolon, we'll have an empty string as "overflow",
            // if it's not, there will be actual overflow in the message string, which will be
            // concatenated to in the next pass
            message = messageList[size - 1];

            // cut off the last member of the list (since it's the overflow)
            messageList = messageList.GetRange(0, size - 1);

            // send the upadtes to the controller
            control.doUpdate(messageList);

            // get ready for a new batch of data from the server
            startReading();
        }

        // sends data, splitting lines with semicolons
        public void sendData(String data)
        {
            // replace semicolons because they have a special function
            data = data.Replace(';', ':');

            // add a semicolon to the end to mark the end of the message
            data += ";";
            
            // convert the message to a byte array
            byte[] toSend = Encoding.ASCII.GetBytes(data.ToString());
            try
            {
                // send the byte array to the server
                stream.Write(toSend, 0, toSend.Length);
            }
            catch (IOException e)
            {
                Debug.Print("can't write during SendData");
                Debug.Print(e.Message);
            }
        }

        
    }
}
