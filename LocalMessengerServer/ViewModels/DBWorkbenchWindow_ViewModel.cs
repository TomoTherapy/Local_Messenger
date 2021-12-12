using LocalMessengerServer.Devices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels;

namespace LocalMessengerServer.ViewModels
{
    public class DBWorkbenchWindow_ViewModel : ViewModelBase
    {
        private Database m_Database;
        private string query;
        private string status;
        private DataView dataview;

        public string Query { get => query; set { query = value; RaisePropertyChanged(); } }
        public string Status { get => status; set { status = value; RaisePropertyChanged(); } }
        public DataView Dataview { get => dataview; set { dataview = value; RaisePropertyChanged(); } }

        public DBWorkbenchWindow_ViewModel()
        {
            m_Database = ((App)Application.Current).m_Database;

            Query = "SELECT name FROM sqlite_master WHERE type = 'table';";
        }

        internal void AllTable_button_Click()
        {
            Query = "SELECT name FROM sqlite_master WHERE type = 'table';";
        }

        internal void SelectAll_button_Click()
        {
            Query = "SELECT * FROM USERS;";
        }

        internal void InsertInto_button_Click()
        {
            Query = "INSERT INTO USERS (ID, PASSWORD, NAME) VALUES ('JesusChrist', '1234', 'Jesus Christ'); ";
        }

        internal void DropTable_button_Click()
        {
            Query = "DROP TABLE IF EXIST USERS;";
        }

        internal void CreateTable_button_Click()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE IF NOT EXISTS USERS( \n");
            sb.Append("UID INTEGER PRIMARY KEY AUTOINCREMENT, \n");
            sb.Append("ID TEXT UNIQUE, \n");
            sb.Append("PASSWORD TEXT NOT NULL, \n");
            sb.Append("NAME TEXT NOT NULL, \n");
            sb.Append("WARNING INTEGER DEFAULT 0, \n");
            sb.Append("ISBANNED BOOLEAN NOT NULL DEFAULT FALSE, \n");
            sb.Append("REGDATE DATE DEFAULT(datetime('now','localtime')) \n");
            sb.Append("); ");

            Query = sb.ToString();
        }

        internal void Update_button_Click()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE USERS \n");
            sb.Append("SET WARNING = CASE WHEN(SELECT WARNING FROM USERS WHERE UID = 0) \n");
            sb.Append("IS NOT 0 THEN(SELECT WARNING FROM USERS WHERE UID = 0) - 1 \n");
            sb.Append("ELSE 0 END \n");
            sb.Append("WHERE UID = 0; \n");

            Query = sb.ToString();
        }

        internal void QueryExecute_btn_Click()
        {
            if (Query.ToUpper().Contains("SELECT"))
            {
                m_Database.lite.ExecuteDataTable(Query);
                Dataview = m_Database.lite.Dt.DefaultView;
            }
            else
            {
                m_Database.lite.ExecuteNonQuery(Query);
            }

            Status = m_Database.lite.Status;
        }
    }
}
