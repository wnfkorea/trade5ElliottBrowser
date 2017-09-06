using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WnFTechnicalIndicators;


namespace trade5ElliottBrowser
{
    public enum WnF_DBType
    {
        SQLCE = 0,
        MySQL = 1,
        SQLite = 2
    }


    public class WnFDbConnectionWrapper : IDisposable
    {
        public WnFDbConnectionWrapper(WnF_DBType k, string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Connection String Empty", "connStr");
            connStr = s;
            type = k;
        }

        #region " IDisposable Support "
        /// Keep track of when the object is disposed. 
        protected bool disposed = false;
        /// This method disposes the base object's resources. 
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    /// Insert code to free managed resources.
                    if ((conn != null)) conn.Close();
                }
                /// Insert code to free unmanaged resources. 
            }
            this.disposed = true;
        }

        /// Do not change or add Overridable to these methods. 
        /// Put cleanup code in Dispose(ByVal disposing As Boolean). 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public DbConnection Connection
        {
            get { return conn; }
        }

        public bool InitConnection()
        {
            SetupConnection();
            return (conn != null);
        }

        public StockOHLCV CheckTable(int p, string s, out string sn)
        {
            StockOHLCV lastC = default(StockOHLCV);
            sn = TableName(p, s);
            if (Exists(sn))
                lastC = LastRow(sn);
            else
                CreateTable(sn);
            return lastC;
        }

        public virtual void InsertTable(DataTable dt) { }

        public int AppendTable(DataTable dt)
        {
            if (!DeleteRow(dt.Rows[0])) throw new Exception("DeleteRow failed");
            InsertTable(dt);
            return dt.Rows.Count;
        }

        public virtual int FillRows(int p, string s, int n, ref DataTable dt_in)
        {
            return 0;
        }

        public static WnFDbConnectionWrapper GetWrapper(WnF_DBType k, string s)
        {
            WnFDbConnectionWrapper wrpper = default(WnFDbConnectionWrapper);

            if (k == WnF_DBType.SQLCE)
            {
        retry:
                wrpper = new SqlCeWrapper(k, s);
                if (!wrpper.InitConnection())
                {
                    wrpper = null; Thread.Sleep(10);
                    goto retry;
                }
            }
            else
                throw new NotImplementedException();

            return wrpper;
        }


        protected WnF_DBType type;
        protected string connStr;
        protected DbConnection conn;


        protected virtual void SetupConnection() { }

        protected virtual bool Exists(string cname)
        {
            return false;
        }

        protected virtual StockOHLCV LastRow(string cname)
        {
            return default(StockOHLCV);
        }

        protected virtual void CreateTable(string cname) { }

        protected string TableName(int p, string s)
        {
            string t = null;
            int i = 0;
            if (int.TryParse(s, out i))
                t = "_" + s;
            else
                t = s;
            t = t.Replace(".", "D_").Replace("#", "_S_").Replace("@", "_AT_").Replace("-", "_DS_") + "_" + p;
            return t;
        }

        protected bool DeleteRow(DataRow dr)
        {
            bool b = true;
            string sql = "delete from " + dr.Table.TableName + " where DateTime Like '"
                                        + ((string)dr["DateTime"]).Substring(0, 10) + "%'";

            try
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WnFDbConnectionWrapper.DeleteRow()] Exception: " + ex.Message);
                b = false;
            }
            return b;
        }
    }


    public class SqlCeWrapper : WnFDbConnectionWrapper
    {
        public SqlCeWrapper(WnF_DBType k, string s) : base(k, s)
        {
        }

        protected override void SetupConnection()
        {
            try
            {
                string strds = connStr.Split(';')[0];
                strds = strds.Split('=')[1];
                if (!File.Exists(strds))
                {
                    SqlCeEngine engine = new SqlCeEngine(connStr);
                    engine.CreateDatabase();
                }
                conn = new SqlCeConnection(connStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at SqlCeWrapper.SetupConn()\r\n" + ex.Message);
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        protected override bool Exists(string cname)
        {
            bool b = false;
            if (cname != string.Empty)
            {
                SqlCeCommand mycommand = ((SqlCeConnection)conn).CreateCommand();
                string sql = "SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + cname + "'";
                mycommand.CommandText = sql;
                b = (Convert.ToInt32(mycommand.ExecuteScalar()) > 0);
            }
            return b;
        }

        protected override StockOHLCV LastRow(string cname)
        {
            StockOHLCV lastC = default(StockOHLCV);
            string sql = "select * from " + cname + " where DateTime IN (select MAX(DateTime) from " + cname + ")";
            SqlCeCommand mycommand = ((SqlCeConnection)conn).CreateCommand();
            mycommand.CommandText = sql;
            try
            {
                lastC = new StockOHLCV(Convert.ToString(mycommand.ExecuteScalar()), 0, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at SqlCeWrapper.LastItem()\r\n" + ex.Message);
            }
            return lastC;
        }

        public override void InsertTable(System.Data.DataTable dt)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = (SqlCeConnection)conn;
                cmd.CommandText = dt.TableName;
                cmd.CommandType = CommandType.TableDirect;

                using (SqlCeResultSet rs = cmd.ExecuteResultSet(ResultSetOptions.Updatable | ResultSetOptions.Scrollable))
                {
                    try
                    {
                        foreach (DataRow r in dt.Rows)
                        {
                            SqlCeUpdatableRecord record = rs.CreateRecord();
                            foreach (DataColumn col in dt.Columns)
                                record.SetValue(dt.Columns.IndexOf(col), r[col]);
                            rs.Insert(record);
                        }
                    }
                    catch (SqlCeException ex)
                    {
                        Console.WriteLine("[SqlCeWrapper.InsertTable()] Exception: \r\n" + ex.Message);
                    }
                }
            }
        }

        private string _create_fields()
        {
            return " (DateTime NVARCHAR(19) PRIMARY KEY, [Open] REAL, High REAL, Low REAL, [Close] REAL, Volume REAL)";
        }

        protected override void CreateTable(string cname)
        {
            string sql = "create table " + cname + _create_fields();
            SqlCeCommand mycommand = ((SqlCeConnection)conn).CreateCommand();
            int rcnt = 0;

        ReCreateSqlCe:
            try
            {
                using (SqlCeTransaction trn = ((SqlCeConnection)conn).BeginTransaction())
                {
                    mycommand.CommandText = sql;
                    mycommand.Transaction = trn;
        RetrySqlCe:
                    try
                    {
                        mycommand.ExecuteNonQuery();
                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception at SqlCeWrapper.CreateTable(), Retry count " + rcnt + "\r\n" + ex.Message);
                        if ((trn != null)) trn.Rollback();
                        if (rcnt < 3)
                        {
                            rcnt += 1;
                            Thread.Sleep(2000);
                            goto RetrySqlCe;
                        }
                    }
                }
            }
            catch (SqlCeException ex)
            {
                if (Strings.InStr(ex.Message, "locked") > 1)
                {
                    Thread.Sleep(1000);
                    goto ReCreateSqlCe;
                }
            }
        }

        public override int FillRows(int p, string s, int n, ref DataTable dt_in)
        {
            int ccnt = 0;
            string sql = "select TOP (" + n + ") * from " + s + " order by DateTime desc";
            SqlCeCommand mycommand = ((SqlCeConnection)conn).CreateCommand();
            mycommand.CommandText = sql;

            SqlCeDataAdapter da = new SqlCeDataAdapter(mycommand);
            DataColumn col = new DataColumn("row_num", typeof(Int32));
            col.AutoIncrement = true;
            col.AutoIncrementSeed = 0;
            dt_in.Columns.Add(col);

            DataTable dt = new DataTable();
            DataTableReader dtReader = default(DataTableReader);

            try
            {
                ccnt = da.Fill(dt);
                if (ccnt > 0)
                {
                    dt = dt.Select(string.Empty, "DateTime Asc").CopyToDataTable();
                    dtReader = new DataTableReader(dt);

                    dt_in.BeginLoadData();
                    dt_in.Load(dtReader);
                    dt_in.EndLoadData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at SqlCeWrapper.FillCandles()\r\n" + ex.Message);
            }

            return ccnt;
        }

    }


    public class MySqlWrapper : WnFDbConnectionWrapper
    {
        public MySqlWrapper(WnF_DBType k, string s) : base(k, s)
        {
            dbname = string.Empty;
            SetDBName(s);
        }

        private string dbname;

        protected override void SetupConnection()
        {
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = connStr;
                conn.Open();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Exception at MySqlWrapper.SetupConn()\r\n" + ex.Message);
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        protected override bool Exists(string cname)
        {
            bool b = false;
            if (cname != string.Empty)
            {
                string sql = "select count(*) from information_schema.tables where table_schema = '" + dbname + "' and table_name = '" + cname + "'";
                MySqlCommand mycommand = new MySqlCommand(sql, (MySqlConnection)conn);
                b = (Convert.ToInt32(mycommand.ExecuteScalar()) > 0);
            }
            return b;
        }

        protected override StockOHLCV LastRow(string cname)
        {
            StockOHLCV lastC = default(StockOHLCV);
            string sql = "select * from " + cname + " where DateTime = (select MAX(DateTime) from " + cname + ")";
            MySqlCommand mycommand = new MySqlCommand(sql, (MySqlConnection)conn);
            try
            {
                lastC = new StockOHLCV(Convert.ToString(mycommand.ExecuteScalar()), 0, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at MySqlWrapper.LastItem()\r\n" + ex.Message);
            }
            return lastC;
        }

        private string _create_fields()
        {
            return " (DateTime VARCHAR(19) PRIMARY KEY, Open REAL, High REAL, Low REAL, Close REAL, Volume REAL)";
        }

        protected override void CreateTable(string cname)
        {
            string sql = "create table " + cname + _create_fields();
            MySqlCommand mycommand = new MySqlCommand(sql, (MySqlConnection)conn);
            int rcnt = 0;

        ReCreateMySQL:
            try
            {
                using (MySqlTransaction trn = ((MySqlConnection)conn).BeginTransaction())
                {
                    mycommand.Transaction = trn;
        RetryMySQL:
                    try
                    {
                        mycommand.ExecuteNonQuery();
                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception at MySqlWrapper.CreateTable(), Retry count " + rcnt + "\r\n" + ex.Message);
                        if ((trn != null)) trn.Rollback();
                        if (rcnt < 3)
                        {
                            rcnt += 1;
                            Thread.Sleep(2000);
                            goto RetryMySQL;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (Strings.InStr(ex.Message, "locked") > 1)
                {
                    Thread.Sleep(1000);
                    goto ReCreateMySQL;
                }
            }
        }

        public override int FillRows(int p, string s, int n, ref DataTable dt_in)
        {
            int ccnt = 0;
            string sql = "select * from (select * from " + s + " order by DateTime desc limit " + n + ") a order by DateTime asc";
            MySqlCommand mycommand = new MySqlCommand(sql, (MySqlConnection)conn);
            MySqlDataAdapter da = new MySqlDataAdapter(mycommand);
            DataColumn col = new DataColumn("row_num", typeof(Int32));
            col.AutoIncrement = true;
            col.AutoIncrementSeed = 0;
            dt_in.Columns.Add(col);

            try
            {
                ccnt = da.Fill(dt_in);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at MySqlWrapper.FillCandles()\r\n" + ex.Message);
            }

            return ccnt;
        }

        private void SetDBName(string s)
        {
            foreach (string st in s.Split(';'))
            {
                string[] kv = st.Trim().Split('=');
                if (kv[0] == "database")
                {
                    dbname = kv[1]; break;
                }
            }
        }
    }


    public class SQLiteWrapper : WnFDbConnectionWrapper
    {
        public SQLiteWrapper(WnF_DBType k, string s) : base(k, s)
        {
        }

        protected override void SetupConnection()
        {
            try
            {
                conn = new SQLiteConnection(connStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at SQLiteWrapper.SetupConn()\r\n" + ex.Message);
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        protected override bool Exists(string cname)
        {
            bool b = false;
            if (cname != string.Empty)
            {
                SQLiteCommand mycommand = new SQLiteCommand((SQLiteConnection)conn);
                string sql = "select count(type) from sqlite_master where type='table' and name='" + cname + "'";
                mycommand.CommandText = sql;
                b = (Convert.ToInt32(mycommand.ExecuteScalar()) > 0);
            }
            return b;
        }

        protected override StockOHLCV LastRow(string cname)
        {
            StockOHLCV lastC = default(StockOHLCV);
            string sql = "select * from " + cname + " where rowid = (select MAX(rowid) from " + cname + ")";
            SQLiteCommand mycommand = new SQLiteCommand((SQLiteConnection)conn);
            mycommand.CommandText = sql;
            try
            {
                lastC = new StockOHLCV(Convert.ToString(mycommand.ExecuteScalar()), 0, 0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at SQLiteWrapper.LastItem()\r\n" + ex.Message);
            }
            return lastC;
        }

        private string _create_fields()
        {
            return " (DateTime TEXT PRIMARY KEY, Open REAL, High REAL, Low REAL, Close REAL, Volume REAL)";
        }

        protected override void CreateTable(string cname)
        {
            string sql = "create table " + cname + _create_fields();
            SQLiteCommand mycommand = new SQLiteCommand((SQLiteConnection)conn);
            int rcnt = 0;

        ReCreate:
            try
            {
                using (SQLiteTransaction trn = (SQLiteTransaction)conn.BeginTransaction())
                {
                    mycommand.CommandText = sql;
                    mycommand.Transaction = trn;
        Retry:
                    try
                    {
                        mycommand.ExecuteNonQuery();
                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception at SQLiteWrapper.CreateTable(), Retry count " + rcnt + "\r\n" + ex.Message);
                        if ((trn != null)) trn.Rollback();
                        if (rcnt < 3)
                        {
                            rcnt += 1;
                            Thread.Sleep(2000);
                            goto Retry;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                if (Strings.InStr(ex.Message, "locked") > 1)
                {
                    Thread.Sleep(1000);
                    goto ReCreate;
                }
            }
        }

        public override int FillRows(int p, string s, int n, ref DataTable dt_in)
        {
            int ccnt = 0;
            string sql = "select * from (select * from " + s + " order by DateTime desc limit " + n + ") order by DateTime asc";
            SQLiteCommand mycommand = new SQLiteCommand((SQLiteConnection)conn);
            mycommand.CommandText = sql;

            SQLiteDataAdapter da = new SQLiteDataAdapter(mycommand);
            DataColumn col = new DataColumn("row_num", typeof(Int32));
            col.AutoIncrement = true;
            col.AutoIncrementSeed = 0;
            dt_in.Columns.Add(col);

            try
            {
                ccnt = da.Fill(dt_in);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception at SQLiteWrapper.FillCandles()\r\n" + ex.Message);
            }

            return ccnt;
        }

    }
}
