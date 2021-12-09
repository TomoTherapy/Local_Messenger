using LocalMessengerServer.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LocalMessengerServer.ViewModels
{
    public class MainWindow_ViewModel : ViewModelBase
    {
        private bool m_IsServerOpened;
        private bool m_ServerConnection;
        private string m_ServerOutput;
        private string m_ServerSendMsg;

        private TcpListener m_Server;
        private TcpClient m_Client;
        private Database m_Database;

        private Xml.XmlParser m_XmlParser;
        private Thread ServerListening;
        private string allMessasges;

        //public ObservableCollection<User> UserList { get; set; }
        public List<Connection> ConnectionList { get; set; }
        public List<Chat> ChatList { get; set; }
        public ObservableCollection<Connection> ConnectionListObs { get; set; }

        #region Server Properties
        public string[] ServerItems { get; set; } = { "0.0.0.0", "127.0.0.1" };
        public string ServerIP { get; set; }
        public int ServerPort { get => m_XmlParser.SavedData.ServerPort; set { if (m_XmlParser.SavedData.ServerPort != value) { m_XmlParser.SavedData.ServerPort = value; RaisePropertyChanged(); } } }
        public bool ShowTimeStamp { get => m_XmlParser.SavedData.ShowTimeStamp; set { m_XmlParser.SavedData.ShowTimeStamp = value; RaisePropertyChanged(); } }
        public bool IsServerOpened { get => m_IsServerOpened; set { if (m_IsServerOpened != value) { m_IsServerOpened = value; RaisePropertyChanged(); } } }
        public bool ServerConnection { get => m_ServerConnection; set { if (m_ServerConnection != value) { m_ServerConnection = value; RaisePropertyChanged(); } } }
        public string ServerOutput { get => m_ServerOutput; set { if (m_ServerOutput != value) { m_ServerOutput = value; RaisePropertyChanged(); } } }
        public string ServerSendMsg { get => m_ServerSendMsg; set { if (m_ServerSendMsg != value) { m_ServerSendMsg = value; RaisePropertyChanged(); } } }
        public string AllMessasges { get => allMessasges; set { allMessasges = value; RaisePropertyChanged(); } }
        public int UserCount { get => ConnectionList.Count; set { } }
        #endregion


        public MainWindow_ViewModel()
        {
            m_Database = new Database();
            ConnectionList = new List<Connection>();
            m_XmlParser = ((App)Application.Current).m_XmlParser;
            ServerIP = ServerItems[0];
        }

        internal void Window_Closing()
        {
            foreach (var connection in ConnectionList)
            {
                connection.KillCode();
            }
        }

        internal void ServerStart_button_Click()
        {
            ServerOpenClose(ServerIP);
        }

        #region Server Methods
        internal void ServerOpenClose(string ip)
        {
            IsServerOpened = !IsServerOpened;

            if (IsServerOpened)
            {

                ServerListening = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        m_Server = new TcpListener(IPAddress.Parse(ip), ServerPort);
                        m_Server.Start();
                        int uid = 0;

                        while (IsServerOpened)
                        {
                            try
                            {
                                m_Client = m_Server.AcceptTcpClient();

                                ConnectionList.Add(new Connection() { UID = uid });
                                ConnectionList.Last().Client = m_Client;
                                ConnectionList.Last().Stream = ConnectionList.Last().Client.GetStream();
                                ConnectionList.Last().DataReceived += new EventHandler(DataReceivedHandle);
                                ConnectionList.Last().Disconnected += new EventHandler(DisconnectedHandle);

                                ConnectionList.Last().StreamThread = new Thread(new ThreadStart(ConnectionList.Last().ConnectionThreadMethod));
                                ConnectionList.Last().StreamThread.Start();
                                ConnectionListRefresh();
                                uid++;
                            }
                            catch (SocketException) { }
                            catch (IOException) { }
                            catch (Exception ex)
                            {
                                ServerOutput += ex.Message + "\n";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ServerOutput += ex.Message + "\n";
                        IsServerOpened = false;
                    }
                }));

                ServerListening.Start();
            }
            else
            {
                ServerClose();
            }
        }


        private void SignIn(string id, string password)
        {
            if(m_Database.SignIn(id, password))//성공 true 실패 false
            {
                ConnectionList.Single(a => a.ID == id).SignIned = true;
            }
        }


        private void DataReceivedHandle(object sender, EventArgs e)
        {
            string msg = (sender as Connection).MSG;
            int SerialNum = (sender as Connection).UID;
            int UID = (sender as Connection).UID;
            DataLog(msg, SerialNum, UID);

            //example "CODE=SEND;MSG=JesusFuck"
            //example "CODE=SIGNIN;ID=babo;PASSWORD=1234"

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict = msg.Split(';').Select(a => a.Split('=')).ToDictionary(a => a[0], a => a[1]);

            if (dict["CODE"] == "SIGNIN")
            {
                SignIn(dict["ID"], dict["PASSWORD"]);
            }
            else if (dict["CODE"] == "OPENCHAT")
            {

            }
            else if (dict["CODE"] == "CLOSECHAT")
            {

            }
            else if (dict["CODE"] == "SIGNOUT")
            {

            }
        }

        private void DisconnectedHandle(object sender, EventArgs e)
        {
            int uid = (sender as Connection).UID;
            ConnectionList.Remove(ConnectionList.Single(a => a.UID == uid));
            ConnectionListRefresh();
        }

        internal void ServerClose()
        {
            foreach (var connection in ConnectionList)
            {
                connection.KillCode();
            }

            IsServerOpened = false;
            ServerConnection = false;
            if (m_Server != null) m_Server.Stop();
            if (ServerListening != null && ServerListening.IsAlive) ServerListening.Join();
        }

        private void ConnectionListRefresh()
        {
            
        }

        private void DataLog(string msg, int serial, int uid)
        {
            AllMessasges = AllMessasges + "\n[" + DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss:fff") + "] Serial=" + serial + "/UID=" + uid + "/Msg=" + msg;
        }
        #endregion
    }

    public class Connection
    {
        public int UID { get; set; }
        public string ID { get; set; }
        public bool SignIned { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public Thread StreamThread { get; set; }
        public byte[] Bytes { get; set; } = new byte[256];
        public string MSG { get; set; } = null;

        public event EventHandler DataReceived;
        public event EventHandler Disconnected;

        public void ConnectionThreadMethod()
        {
            try 
            { 
                int len = 0;
                while ((len = Stream.Read(Bytes, 0, Bytes.Length)) != 0)
                {
                    MSG = ReadByteToString(Bytes, len);

                    EventHandler handler1 = DataReceived;
                    if (null != handler1) handler1(this, EventArgs.Empty);
                }
            }
            finally
            {
                SignIned = false;
                Stream.Close();
                Client.Close();

                EventHandler handler2 = Disconnected;
                if (null != handler2) handler2(this, EventArgs.Empty);
            }
        }

        private string ReadByteToString(byte[] data, int len)
        {
            return Encoding.Default.GetString(data, 0, len);
        }

        public void KillCode()
        {
            SignIned = false;
            if (Stream != null) Stream.Close();
            if (Client != null) Client.Close();
        }
    }

    public class Chat
    {
        public int UID { get; set; }
        public string User1 { get; set; }
        public string User2 { get; set; }
        public List<Message> Chats { get; set; }
    }

    public class Message
    {
        public string ID { get; set; }
        public string MSG { get; set; }
    }

    public class User
    {
        public string Alias { get; set; }
        public string UID { get; set; }
        public bool Connected { get; set; }
        public bool Banned { get; set; }

        public User()
        {
            Alias = "";
            UID = "";
            Connected = false;
            Banned = false;
        }
    }
}
