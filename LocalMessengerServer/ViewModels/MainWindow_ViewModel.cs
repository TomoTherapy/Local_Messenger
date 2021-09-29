using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessengerServer.ViewModels
{
    public class MainWindow_ViewModel : ViewModelBase
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public ObservableCollection<string> UserList { get; set; }
        public ObservableCollection<string> ConnectedUserList { get; set; }



        public MainWindow_ViewModel()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 8001);

            TcpClient clientSocket = default(TcpClient);

            int count = 0;

            serverSocket.Start();

            while (true)
            {
                count++;
                clientSocket = serverSocket.AcceptTcpClient();
                handleClinet client = new handleClinet();
                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(count) + " started!");
                client.startClient(clientSocket, count.ToString());
            }

            clientSocket.Close();
            serverSocket.Stop();

        }


        internal void ServerStart_button_Click()
        {
            
        }
    }

    //Class to handle each client request separatly
    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" >> " + "From client-" + clNo + dataFromClient);

                    rCount = Convert.ToString(requestCount);
                    serverResponse = "Server to clinet(" + clNo + ") " + rCount;
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
        }
    }
}
