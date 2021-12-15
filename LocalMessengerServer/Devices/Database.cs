using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace LocalMessengerServer.Devices
{
    public class Database
    {
        public SQLiteX64 lite;
        public string Status { get; set; }
        public DataTable DT { get; set; }

        public Database()
        {
            lite = new SQLiteX64();
            lite.ConnectDB(AppDomain.CurrentDomain.BaseDirectory + "database.db");
        }

        public void CreateUsersTable()
        {
            try
            {
                string sql = "DROP TABLE IF EXISTS USERS;";
                lite.ExecuteNonQuery(sql);

                StringBuilder sb = new StringBuilder();
                sb.Append("CREATE TABLE IF NOT EXISTS USERS ");
                sb.Append("( ");
                sb.Append("UID INTEGER PRIMARY KEY AUTOINCREMENT, ");
                sb.Append("PASSWORD TEXT NOT NULL, ");
                sb.Append("NAME TEXT NOT NULL, ");
                sb.Append("WARNING INTEGER DEFAULT 0, ");
                sb.Append("ISBANNED INTEGER DEFAULT 0, ");
                sb.Append("REGDATE DATE DEFAULT(datetime('now', 'localtime')) ");
                sb.Append(");");

                lite.ExecuteNonQuery(sb.ToString());
                Status = lite.Status;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SignUp(string id, string password)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO USERS(ID, PASSWORD, NAME) ");
                sb.Append("VALUES('" + id + "', '" + password + "', '');");

                lite.ExecuteNonQuery(sb.ToString());
                Status = lite.Status;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckIDDuplication(string id)
        {
            try
            {
                string qeury = "SELECT COUNT(*) FROM USERS WHERE ID = '" + id + "';";
                lite.ExecuteDataTable(qeury);

                string result = lite.Dt.DefaultView[0].Row.ItemArray[0].ToString();

                if (int.Parse(result) == 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool SignIn(string id, string password)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT COUNT(*) ");
                sb.Append("FROM USERS ");
                sb.Append("WHERE ID = '" + id + "' AND PASSWORD = '" + password + "';");

                lite.ExecuteDataTable(sb.ToString());

                //var results = from users in lite.Dt.AsEnumerable()
                //              select users.Field<int>("OKNG");

                string result = lite.Dt.DefaultView[0].Row.ItemArray[0].ToString();

                if (int.Parse(result) == 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateWarningPlus(string id)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE USERS ");
                sb.Append("SET WARNING = (SELECT WARNING FROM USERS WHERE ID = '" + id.ToString() + "') + 1 ");
                sb.Append("WHERE ID = '" + id.ToString() + "';");

                lite.ExecuteNonQuery(sb.ToString());
                Status = lite.Status;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateWarningMinus(string id)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE USERS ");
                sb.Append("SET WARNING = CASE WHEN(SELECT WARNING FROM USERS WHERE ID = '" + id + "') ");
                sb.Append("IS NOT 0 THEN(SELECT WARNING FROM USERS WHERE ID = '" + id + "') - 1 ");
                sb.Append("ELSE 0 END ");
                sb.Append("WHERE ID = '" + id + "';");


                lite.ExecuteNonQuery(sb.ToString());
                Status = lite.Status;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void BanUnban(string id)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE USERS ");
                sb.Append("SET ISBANNED = CASE WHEN(SELECT ISBANNED FROM USERS WHERE ID = '" + id + "') ");
                sb.Append("IS FALSE THEN TRUE ");
                sb.Append("ELSE FALSE END ");
                sb.Append("WHERE UID = '" + id + "';");


                lite.ExecuteNonQuery(sb.ToString());
                Status = lite.Status;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable SelectRecords(string id = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM USERS ");
            sb.Append("WHERE 1=1 ");

            if (id != null) sb.Append("AND ID LIKE '%" + id + "%' ");
            
            sb.Append("ORDER BY REGDATE DESC;");

            lite.ExecuteDataTable(sb.ToString());
            Status = lite.Status;

            return lite.Dt;
        }

        internal void ExportCSV(string start, string end, bool ok, bool ng, string cellIDSearch)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string path = dialog.SelectedPath + @"\CSV\";
            Directory.CreateDirectory(path);

            FileStream fs = new FileStream(path + "Database_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT * FROM USERS;");

                lite.ExecuteDataTable(sb.ToString());

                string line = string.Join(",", lite.Dt.Columns.Cast<object>());
                sw.WriteLine(line);

                foreach (DataRow row in lite.Dt.Rows)
                {
                    line = string.Join(",", row.ItemArray.Cast<object>());
                    sw.WriteLine(line);
                }

                Status = lite.Status + ". Export as CSV Completed";
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                throw new Exception(ex.Message);
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }

    }

    public class SQLiteX64
    {
        private SQLiteConnection conn;
        public string Status { get; set; }
        public int AffectedRows { get; set; }
        public bool Connection { get; set; }
        public DataTable Dt { get; set; }
        public DataSet Ds { get; set; }

        public SQLiteX64()
        {
            conn = null;
            Status = "";
            AffectedRows = 0;
            Connection = false;
            Dt = new DataTable();
            Ds = new DataSet();
        }

        public void ConnectDB(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    conn = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                }
                else
                {
                    SQLiteConnection.CreateFile(path);
                    conn = new SQLiteConnection("Data Source=" + path + ";Version=3;");
                }

                Connection = true;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                Connection = false;
            }

            ConnectionTest();
        }

        public void ConnectionTest()
        {
            try
            {
                conn.Open();
                Connection = true;
                Status = "Connection Successful";
            }
            catch (Exception ex)
            {
                Connection = false;
                Status = ex.Message;
            }
            finally
            {
                conn.Close();
            }
        }

        public void ExecuteNonQuery(string sql)
        {
            try
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand(sql, conn);//????
                int result = command.ExecuteNonQuery();

                AffectedRows = result;
                Status = "Affected rows : " + result.ToString();
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                conn.Close();
            }
        }

        public void ExecuteDataTable(string sql)
        {
            try
            {
                conn.Open();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, conn);
                Dt = new DataTable();
                adapter.Fill(Dt);
                Status = "Query Executed";
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                conn.Close();
            }

        }

        public void ExecuteDataSet(string sql)
        {
            try
            {
                conn.Open();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, conn);
                Ds = new DataSet();
                adapter.Fill(Ds, "LoadDataBinding");
                Status = "Query Executed";
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                conn.Close();
            }
        }

        public int ExecuteToGetCount(string sql)
        {
            try
            {
                conn.Open();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, conn);
                Dt = new DataTable();
                adapter.Fill(Dt);

                int count = (int)Dt.Rows[0].Field<Int64>(0);

                Status = "Query Executed";

                return count;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                return 0;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
