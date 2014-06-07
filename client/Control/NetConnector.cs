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
        private String inputBuffer = "";

        // connection information
        IPAddress IP;
        int port;

        public NetConnector(Controller control)
        {
            // assign controller
            this.control = control;
        }

        public void Connect(String IP, int port)
        {
            // create the client
            client = new TcpClient();

            // parse the IP and port
            bool validIP =  IPAddress.TryParse(IP, out this.IP);
            this.port = port;

            // don't try to connect if already connected.
            if (bRunning) return;

            try
            {
                // connect the client to the server. connectionMade will be called when connected
                client.BeginConnect(IP, port, new AsyncCallback(ConnectionMade), null);

                // set flag to show connector is running
                bRunning = true;
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
        private void ConnectionMade(IAsyncResult newConnection)
        {
            try
            {
                // finish connecting
                client.EndConnect(newConnection);

                // set stream to be the client's networkstream
                stream = client.GetStream();
            }
            catch (IOException e)
            {
                Debug.Print("exception on finishing connection");
                Debug.Print(e.Message);

                Disconnect();
            }
            catch (SocketException e)
            {
                Debug.Print("failure to connect");
                Debug.Print(e.Message);

                Disconnect();
            }

            // start listening for data
            StartReading();
        }

        // starts listening for data if brunning is true, otherwise it closes the connection
        private void StartReading()
        {
            if (bRunning)
            {
                try
                {
                    byte[] buffer = new byte[1024];

                    // begin reading. When data arrives, call dataReveived
                    stream.BeginRead(buffer, 0, 1024, DataReceived, buffer);
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
        private void DataReceived(IAsyncResult data)
        {
            // if the client isn't connected, we can't receive anything.
            if (!bRunning)
            {
                client.Close();
                return;
            } 

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

                // handle what you can
                SendInput();

                // and resume reading
                StartReading();
            }
            catch (IOException e)
            {
                Debug.Print("can't read during dataReceived");
                Debug.Print(e.Message);

                if (bRunning) Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                Debug.Print("client was disposed while reading");
                Debug.Print(e.Message);
            }
        }

        private void SendInput()
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

            // update using the list
            control.DoUpdate(inputList);
        }

        // sends data, splitting lines with semicolons
        public void SendData(String data)
        {
            // if we're not connected, we can't send data
            if (!bRunning) return;

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
