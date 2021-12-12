using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ViewModels;

namespace LocalMessengerClient.ViewModels
{
    public class MainWindow_ViewModel : ViewModelBase
    {
        private Thread ClientReading;
        private TcpClient m_Client;
        private NetworkStream m_Stream;
        private bool isClientOpened;
        private bool clientConnection;
        private string clientIP;
        private int clientPort;
        private string clientOutput;
        private string allMessasges;
        private string iD;
        private string password;

        public event EventHandler DataReceived;

        public bool IsClientOpened { get => isClientOpened; set { isClientOpened = value; RaisePropertyChanged(); } }
        public bool ClientConnection { get => clientConnection; set { clientConnection = value; RaisePropertyChanged(); } }
        public string ClientIP { get => clientIP; set { clientIP = value; RaisePropertyChanged(); } }
        public int ClientPort { get => clientPort; set { clientPort = value; RaisePropertyChanged(); } }
        public string ClientOutput { get => clientOutput; set { clientOutput = value; RaisePropertyChanged(); } }
        public string AllMessasges { get => allMessasges; set { allMessasges = value; RaisePropertyChanged(); } }
        public string ID { get => iD; set { iD = value; RaisePropertyChanged(); } }
        public string Password { get => password; set { password = value; RaisePropertyChanged(); } }

        public byte[] Bytes { get; set; } = new byte[256];
        public string MSG { get; set; } = null;

        public MainWindow_ViewModel()
        {
            DataReceived += new EventHandler(DataReceivedHandle);

            ClientIP = "127.0.0.1";
            ClientPort = 5000;
        }

        internal void Connect_button_Click()
        {
            ClientOpenClose();
        }

        internal void SignIn_button_Click()
        {

        }

        internal void SignOut_button_Click()
        {

        }

        internal void ClientOpenClose()
        {
            IsClientOpened = !IsClientOpened;

            if (IsClientOpened)
            {

                ClientReading = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        byte[] buffer = new byte[256];
                        //string data = "";

                        while (IsClientOpened)
                        {
                            try
                            {
                                ClientConnection = false;

                                m_Client = new TcpClient(ClientIP, ClientPort);
                                m_Stream = m_Client.GetStream();

                                ClientConnection = true;

                                int len = 0;
                                while ((len = m_Stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    MSG = ReadByteToString(Bytes, len);

                                    EventHandler handler = DataReceived;
                                    if (null != handler) handler(this, EventArgs.Empty);
                                }

                                Thread.Sleep(50);
                            }
                            catch (SocketException) { }
                            catch (IOException) { }
                            catch (ThreadAbortException) { }
                            catch (Exception ex)
                            {
                                ClientOutput += ex.Message + "\n";
                            }
                        }
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        ClientOutput += ex.Message + "\n";
                    }
                }));

                ClientReading.Start();
            }
            else
            {
                ClientClose();
            }
        }

        internal void ClientClose()
        {
            IsClientOpened = false;
            ClientConnection = false;
            if (m_Stream != null) m_Stream.Close();
            if (m_Client != null) m_Client.Close();
            if (ClientReading != null && ClientReading.IsAlive) ClientReading.Abort();
        }

        private string ReadByteToString(byte[] data, int len)
        {
            return Encoding.Default.GetString(data, 0, len);
        }

        private void DataReceivedHandle(object sender, EventArgs e)
        {
            string msg = (sender as MainWindow_ViewModel).MSG;
            DataLog(msg);

            //example "CODE=SEND;MSG=JesusFuck"
            //example "CODE=SIGNIN;ID=babo;PASSWORD=1234"
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict = msg.Split(';').Select(a => a.Split('=')).ToDictionary(a => a[0], a => a[1]);

            if (dict["CODE"] == "SIGNIN")
            {
                SignIn(dict["CONFIRMED"]);
            }
            else if (dict["CODE"] == "SIGNOUT")
            {
                SignOut(dict["CONFIRMED"]);
            }
            else if (dict["CODE"] == "SIGNUP")
            {
                SignUp(dict["CONFIRMED"]);
            }
            else if (dict["CODE"] == "OPENCHAT")
            {
                OpenChat(dict["CONFIRMED"]);
            }
            else if (dict["CODE"] == "CLOSECHAT")
            {
                CloseChat(dict["CONFIRMED"]);
            }
            else if (dict["CODE"] == "SENDMSG")
            {
                //SendMSG(dict["ID"], dict["TargetID"], dict["MSG"]);
            }
            else if (dict["CODE"] == "RECEIVEMSG")
            {
                ReceiveMSG(dict["FROMID"], dict["MSG"]);
            }
        }

        private void SignIn(string confirmed)
        {

        }

        private void SignOut(string confirmed)
        {

        }

        private void SignUp(string confirmed)
        {

        }

        private void OpenChat(string confirmed)
        {

        }

        private void CloseChat(string confirmed)
        {

        }

        private void SendMSG()
        {

        }

        private void ReceiveMSG(string fromId, string msg)
        {

        }

        private void DataLog(string msg)
        {
            AllMessasges = AllMessasges + "← [" + DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss:fff") + "] Msg=" + msg + "\n";
        }
    }
}
