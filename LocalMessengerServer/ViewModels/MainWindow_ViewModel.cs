using LocalMessengerServer.Controls;
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
using ViewModels;

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
        private DBWorkbenchWindow DBWorkbench;

        private Xml.XmlParser m_XmlParser;
        private Thread ServerListening;
        private string allMessasges;
        private List<Connection> connectionListDisplay;
        private Connection selectedConnection;

        //public ObservableCollection<User> UserList { get; set; }
        public List<Connection> ConnectionList { get; set; }
        public List<Connection> ConnectionListDisplay { get => connectionListDisplay; set { connectionListDisplay = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(UserCount)); } }
        public Connection SelectedConnection { get => selectedConnection; set => selectedConnection = value; }
        public List<Chat> ChatList { get; set; }

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
        public int UserCount { get { if (ConnectionListDisplay == null) return 0; else return ConnectionListDisplay.Count; } set { } }
        #endregion

        public MainWindow_ViewModel()
        {
            m_Database = ((App)Application.Current).m_Database;
            m_XmlParser = ((App)Application.Current).m_XmlParser;
            ServerIP = ServerItems[0];
            ConnectionList = new List<Connection>();
        }

        internal void Window_Closing()
        {
            ServerClose();
        }

        internal void Warning_button_Click()
        {
            m_Database.UpdateWarningPlus(SelectedConnection.ID);
        }

        internal void Block_button_Click()
        {
            m_Database.BanUnban(SelectedConnection.ID);
        }

        internal void DBWorkbench_button_Click()
        {
            if (DBWorkbench == null || DBWorkbench.IsLoaded == false)
            {
                DBWorkbench = new DBWorkbenchWindow();
                DBWorkbench.Show();
            }
        }

        internal void ServerStart_button_Click()
        {
            ServerOpenClose(ServerIP);
        }

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
                                DataLog("Connection Established", uid);
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

        private void DataReceivedHandle(object sender, EventArgs e)
        {
            string msg = (sender as Connection).MSG;
            int UID = (sender as Connection).UID;
            DataLog(msg, UID);

            //example "CODE=SEND;MSG=JesusFuck"
            //example "CODE=SIGNIN;ID=babo;PASSWORD=1234"

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict = msg.Split(';').Select(a => a.Split('=')).ToDictionary(a => a[0], a => a[1]);

            if (dict["CODE"] == "SIGNIN")
            {
                SignIn(UID, dict["ID"], dict["PASSWORD"]);
                SendUserList();
            }
            else if (dict["CODE"] == "SIGNOUT")
            {
                SignOut(UID, dict["ID"]);
            }
            else if (dict["CODE"] == "SIGNUP")
            {
                SignUp(UID, dict["ID"], dict["PASSWORD"]);
                //SignIn(UID, dict["ID"], dict["PASSWORD"]);
                //SendUserList();
            }
            else if (dict["CODE"] == "OPENCHAT")
            {
                OpenChat(UID, dict["ID"], dict["TARGETID"]);
            }
            else if (dict["CODE"] == "CLOSECHAT")
            {
                CloseChat(UID, dict["TARGETID"]);
            }
            else if (dict["CODE"] == "SENDUSERLIST")
            {
                SendUserList();
            }
            else if (dict["CODE"] == "SENDMSG")
            {
                SendMSG(UID, dict["TARGETID"], dict["MSG"]);
            }
        }

        private void SignIn(int uid, string id, string password)
        {
            if (m_Database.SignIn(id, password))//성공 true 실패 false
            {
                ConnectionList.Single(a => a.UID == uid).IsSignedIn = true;
                ConnectionList.Single(a => a.UID == uid).ID = id;
                ConnectionListRefresh();

                StreamWriteMSG(uid, "CODE=SIGNIN;CONFIRMED=1");
            }
            else
            {
                StreamWriteMSG(uid, "CODE=SIGNIN;CONFIRMED=0");
            }
        }

        private void SignOut(int uid, string id)
        {
            ConnectionList.Single(a => a.UID == uid).IsSignedIn = false;
            ConnectionList.Single(a => a.UID == uid).ID = "";
            StreamWriteMSG(uid, "CODE=SIGNOUT;CONFIRMED=1");
            ConnectionListRefresh();
        }

        private void SignUp(int uid, string id, string password)
        {
            try
            {
                //DB 조회로 중복확인.
                if (m_Database.CheckIDDuplication(id))
                {
                    //있으면 confirm 0
                    StreamWriteMSG(uid, "CODE=SIGNUP;CONFIRMED=0;REASON=DUPLICATION");
                }
                else
                {
                    //없으면 insert, confirm 1
                    m_Database.SignUp(id, password);
                    StreamWriteMSG(uid, "CODE=SIGNUP;CONFIRMED=1");
                }
            }
            catch(Exception ex)
            {
                StreamWriteMSG(uid, "CODE=SIGNUP;CONFIRMED=0;REASON=ERROR");
            }

            ConnectionListRefresh();
        }

        private void OpenChat(int uid, string id, string targetId)
        {
            var connection = ConnectionList.Single(a => a.ID == targetId).IsSignedIn;
            if (connection)
            {
                StreamWriteMSG(uid, "CODE=OPENCHAT;TARGETID=" + targetId + ";CONFIRMED=1");//요청자에게 신호
                StreamWriteMSG(ConnectionList.Single(a => a.ID == targetId).UID, "CODE=OPENCHAT;TARGETID=" + ConnectionList.Single(a => a.ID == id).ID + ";CONFIRMED=1");//받는놈한테 신호
            }
            else
            {
                StreamWriteMSG(uid, "CODE=OPENCHAT;TARGETID=" + targetId + ";CONFIRMED=0");
            }
        }

        private void CloseChat(int uid, string targetId)
        {
            StreamWriteMSG(ConnectionList.Single(a => a.ID == targetId).UID, "CODE=CLOSECHAT;CONFIRMED=1");

            //var connection = ConnectionList.Single(a => a.ID == targetId);
            //if (connection == null || !connection.IsSignedIn)
            //{
            //    StreamWriteMSG(uid, "CODE=CLOSECHAT;TARGETID=" + targetId + ";CONFIRMED=1");
            //}
            //else
            //{
            //    StreamWriteMSG(uid, "CODE=CLOSECHAT;TARGETID=" + targetId + ";CONFIRMED=1");//요청자에게 신호
            //    StreamWriteMSG(ConnectionList.Single(a => a.ID == targetId).UID, "CODE=CLOSECHAT;TARGETID=" + ConnectionList.Single(a => a.ID == id).ID + ";CONFIRMED=1");//받는놈한테 신호
            //}
        }

        private void SendUserList()
        {
            StringBuilder users = new StringBuilder();
            foreach (var connection in ConnectionList)
            {
                if (connection.IsSignedIn)
                    users.Append(connection.ID + ",");
            }

            foreach (var connection in ConnectionList)
            {
                if (connection.IsSignedIn)
                {
                    StreamWriteMSG(connection.UID, "CODE=USERLIST;USERS=" + users.ToString().Substring(0, users.ToString().Length - 1));
                }
            }
        }

        private void SendMSG(int uid, string targetId, string msg)
        {
            var connection = ConnectionList.Single(a => a.ID == targetId);
            if (connection == null || !connection.IsSignedIn)
            {
                //연결체크 후 끊겼으면 각각클라이언트에 메세지 띄우고 창닫게 하기
                
                StreamWriteMSG(uid, "CODE=WARNING;MSG=The friend is disconnected");//요청자에게 신호
                CloseChat(uid, targetId);
                CloseChat(uid, ConnectionList.Single(a => a.UID == uid).ID);
                //StreamWriteMSG(ConnectionList.Single(a => a.ID == targetId).UID, "");//받는놈한테 신호
            }
            else
            {
                //StreamWriteMSG(uid, "");//요청자에게 신호
                StreamWriteMSG(ConnectionList.Single(a => a.ID == targetId).UID, "CODE=RECEIVEMSG;FROMID=" + ConnectionList.Single(a => a.UID == uid).ID + ";MSG=" + msg);//받는놈한테 신호
            }
        }

        private void StreamWriteMSG(int uid, string msg)
        {
            AllMessasges = AllMessasges + "→ [" + DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss:fff") + "] UID : " + uid + " [" + msg + "]\n";

            byte[] buffer = Encoding.Default.GetBytes(msg);

            ConnectionList.Single(a => a.UID == uid).Stream.Write(buffer, 0, buffer.Length);
        }

        private void DisconnectedHandle(object sender, EventArgs e)
        {
            int uid = (sender as Connection).UID;
            ConnectionList.Remove(ConnectionList.Single(a => a.UID == uid));
            DataLog("Connection lost", uid);
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
            if (m_Client != null) m_Client.Close();
            if (m_Server != null) m_Server.Stop();
            if (ServerListening != null && ServerListening.IsAlive) ServerListening.Join();
        }

        private void ConnectionListRefresh()
        {
            ConnectionListDisplay = new List<Connection>(ConnectionList);
        }

        private void DataLog(string msg, int uid)
        {
            AllMessasges = AllMessasges + "← [" + DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss:fff") + "] UID : " + uid + " [" + msg + "]\n";
        }
    }

    public class Connection
    {
        public int UID { get; set; }
        public string ID { get; set; }
        public bool IsSignedIn { get; set; }
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
            catch (Exception ex)
            {

            }
            finally
            {
                IsSignedIn = false;
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
            IsSignedIn = false;
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
