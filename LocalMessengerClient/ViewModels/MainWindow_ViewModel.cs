using LocalMessengerClient.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
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
        private bool isSignedIn;
        private List<string> userList;

        public bool IsClientOpened { get => isClientOpened; set { isClientOpened = value; RaisePropertyChanged(); } }
        public bool ClientConnection { get => clientConnection; set { clientConnection = value; RaisePropertyChanged(); } }
        public string ClientIP { get => clientIP; set { clientIP = value; RaisePropertyChanged(); } }
        public int ClientPort { get => clientPort; set { clientPort = value; RaisePropertyChanged(); } }
        public string ClientOutput { get => clientOutput; set { clientOutput = value; RaisePropertyChanged(); } }
        public string AllMessasges { get => allMessasges; set { allMessasges = value; RaisePropertyChanged(); } }
        public string ID { get => iD; set { iD = value; RaisePropertyChanged(); } }
        public string Password { get => password; set { password = value; RaisePropertyChanged(); } }
        public bool IsSignedIn { get => isSignedIn; set { isSignedIn = value; RaisePropertyChanged(); } }
        public List<string> UserList { get => userList; set { userList = value; RaisePropertyChanged(); } }
        public string MSG { get; set; } = null;

        public MainWindow_ViewModel()
        {
            ClientIP = "127.0.0.1";
            ClientPort = 5000;
            IsSignedIn = false;
            UserList = new List<string>();
        }

        internal void SignUp_button_Click()
        {
            SignUpWindow window = new SignUpWindow(this);
            window.ShowDialog();
        }

        internal void Window_Closing()
        {
            ClientClose();
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
                                    MSG = ReadByteToString(buffer, len);
                                    DataReceivedHandle(MSG);
                                }

                                Thread.Sleep(50);
                            }
                            catch (SocketException) { IsSignedIn = false; }
                            catch (IOException) { IsSignedIn = false; }
                            catch (ThreadAbortException) { IsSignedIn = false; }
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
                IsSignedIn = false;
            }
        }

        internal void ClientClose()
        {
            IsSignedIn = false;
            IsClientOpened = false;
            ClientConnection = false;
            if (m_Stream != null) m_Stream.Close();
            if (m_Client != null) m_Client.Close();
            if (ClientReading != null && ClientReading.IsAlive) ClientReading.Abort();
        }

        private string ReadByteToString(byte[] data, int len)
        {
            try
            {
                return Encoding.Default.GetString(data, 0, len);
            }
            catch
            {
                return "";
            }
        }

        private void StreamWrite(string msg)
        {
            if (m_Stream == null || !m_Stream.CanWrite) return;
            byte[] buffer = Encoding.Default.GetBytes(msg);
            m_Stream.Write(buffer, 0, buffer.Length);
        }

        internal void Connect_button_Click()
        {
            ClientOpenClose();
        }

        internal void SignIn_button_Click()
        {
            StreamWrite("CODE=SIGNIN;ID=" + ID + ";PASSWORD=" + Password);
        }

        internal void SignOut_button_Click()
        {
            StreamWrite("CODE=SIGNOUT;ID=" + ID);
        }

        internal void RefreshUserList_button_Click()
        {
            StreamWrite("CODE=SENDUSERLIST");
        }

        internal void DataGridCell_MouseDoubleClick(string id)
        {
            StreamWrite("CODE=OPENCHAT;ID=" + ID + ";TARGETID=" + id);
        }

        private void DataReceivedHandle(string msg)
        {
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
                OpenChat(dict["CONFIRMED"], dict["TARGETID"]);
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
            else if (dict["CODE"] == "USERLIST")
            {
                ReceiveUserList(dict["USERS"]);
            }
        }

        private void SignIn(string confirmed)
        {
            if (confirmed == "1")
            {
                IsSignedIn = true;
                Password = "";
            }
            else IsSignedIn = false;
        }

        private void SignOut(string confirmed)
        {
            if (confirmed.Equals("1")) IsSignedIn = false;
        }

        private AutoResetEvent signUpResetEvent = new AutoResetEvent(false);
        private int signUpConfirm = -1;
        public int SignUpRequest(string id, string password)
        {
            StreamWrite("CODE=SIGNUP;ID=" + id + ";PASSWORD=" + password);
            signUpResetEvent.WaitOne(1000);

            return signUpConfirm;
        }

        private void SignUp(string confirmed)
        {
            signUpConfirm = int.Parse(confirmed);
            signUpResetEvent.Set();
        }

        private void OpenChat(string confirmed, string targetId)
        {
            if (confirmed.Equals("1"))
            {
                //UI thread

            }
            else
            {
                MessageBox.Show("Connection Failed!");
            }
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

        private void ReceiveUserList(string users)
        {
            List<string> userList = users.Split(',').ToList();
            userList.Remove(ID);
            UserList = new List<string>(userList);
        }

        private void DataLog(string msg)
        {
            AllMessasges = AllMessasges + "← [" + DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss:fff") + "] Msg=" + msg + "\n";
        }

    }
}
