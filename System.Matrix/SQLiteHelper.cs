using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Windows.Forms;

namespace System.Matrix
{
    /// <summary>
    /// Copyright (C) 2011 Maticsoft 
    /// ���ݷ��ʻ�����(����SQLite)
    /// �����û������޸������Լ���Ŀ����Ҫ��
    /// </summary>
    public abstract class SQLiteHelper
    {
        //���ݿ������ַ���(web.config������)�����Զ�̬����connectionString֧�ֶ����ݿ�.		
        //public static string connectionString = "Data Source= " + Application.StartupPath + "\\" + ClsAppCommon.AppCommon.dbSourcePath + ";Pooling=true;Min Pool Size=0;Max Pool Size=100;PublicKeyToken=db937bc2d44ff139;";
        public static string connectionString = $"Data Source={Application.StartupPath}\\ASP64B16.db";//"Data Source= " + ClsAppCommon.AppCommon.dbSourcePath + "";
        //public static readonly ILog loggerDebug = log4net.LogManager.GetLogger("debug");//ѡ��<logger name="debug">������ 

        public SQLiteHelper()
        {
        }

        #region ���÷���

        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }
        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static bool Exists(string strSql, params SQLiteParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region  ִ�м�SQL���

        /// <summary>
        /// ִ��SQL��䣬����Ӱ��ļ�¼��
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SQLiteException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }
        /// <summary>
        /// ִ��SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">SQL���</param>		
        public static void ExecuteSqlTran(string sqlString)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    using (SQLiteTransaction tx = conn.BeginTransaction())
                    {
                        cmd.Transaction = tx;
                        try
                        {
                            cmd.CommandText = sqlString;
                            cmd.ExecuteNonQuery();
                            tx.Commit();
                        }
                        catch (SQLiteException ex)
                        {
                            tx.Rollback();
                            throw ex;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">����SQL���</param>		
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = conn;
                SQLiteTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (SQLiteException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
            }
        }
        /// <summary>
        /// ִ�ж�������SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">ִ�е�sql���</param>
        /// <param name="tableName">����</param>
        public static void ExecuteInsertSqlTran(ArrayList SQLStringList, string tableName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = conn;
                SQLiteTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = "INSERT INTO [" + tableName + "]" + strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (SQLiteException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
            }
        }
        /// <summary>
        /// ִ�ж�������SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="sqlStringList">ִ�е�sql���</param>
        public static void ExecuteInsertSqlTran(List<string> sqlStringList)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    using (SQLiteTransaction tx = conn.BeginTransaction())
                    {
                        cmd.Transaction = tx;
                        try
                        {
                            for (int n = 0; n < sqlStringList.Count; n++)
                            {
                                string sql = sqlStringList[n].ToString();
                                if (sql.Trim().Length > 1)
                                {
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            tx.Commit();
                        }
                        catch (SQLiteException ex)
                        {
                            tx.Rollback();
                            throw ex;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// ִ�д�һ���洢���̲����ĵ�SQL��䡣
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <param name="content">��������,����һ���ֶ��Ǹ�ʽ���ӵ����£���������ţ�����ͨ�������ʽ����</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(SQLString, connection);
                SQLiteParameter myParameter = new SQLiteParameter("@content", DbType.String);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// �����ݿ������ͼ���ʽ���ֶ�(������������Ƶ���һ��ʵ��)
        /// </summary>
        /// <param name="strSQL">SQL���</param>
        /// <param name="fs">ͼ���ֽ�,���ݿ���ֶ�����Ϊimage�����</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(strSQL, connection);
                SQLiteParameter myParameter = new SQLiteParameter("@fs", DbType.Binary);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SQLiteException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// ִ��һ�������ѯ�����䣬���ز�ѯ�����object����
        /// </summary>
        /// <param name="SQLString">�����ѯ������</param>
        /// <returns>��ѯ�����object��</returns>
        public static object GetSingle(string SQLString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SQLiteException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// ִ�в�ѯ��䣬����SQLiteDataReader
        /// </summary>
        /// <param name="strSQL">��ѯ���</param>
        /// <returns>SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string strSQL)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand(strSQL, connection);
            try
            {
                connection.Open();
                SQLiteDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (SQLiteException e)
            {
                throw new Exception(e.Message);
            }

        }
        /// <summary>
        /// ִ�в�ѯ��䣬����DataSet
        /// </summary>
        /// <param name="SQLString">��ѯ���</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                    //loggerDebug.Debug(SQLString);
                }
                catch (SQLiteException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }


        #endregion

        #region ִ�д�������SQL���

        /// <summary>
        /// ִ��SQL��䣬����Ӱ��ļ�¼��
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public static int ExecuteSql(string SQLString, params SQLiteParameter[] cmdParms)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SQLiteException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
        }


        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">SQL���Ĺ�ϣ����keyΪsql��䣬value�Ǹ�����SQLiteParameter[]��</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    SQLiteCommand cmd = new SQLiteCommand();
                    try
                    {
                        //ѭ��
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SQLiteParameter[] cmdParms = (SQLiteParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// ִ��һ�������ѯ�����䣬���ز�ѯ�����object����
        /// </summary>
        /// <param name="SQLString">�����ѯ������</param>
        /// <returns>��ѯ�����object��</returns>
        public static object GetSingle(string SQLString, params SQLiteParameter[] cmdParms)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SQLiteException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// ִ�в�ѯ��䣬����SQLiteDataReader
        /// </summary>
        /// <param name="strSQL">��ѯ���</param>
        /// <returns>SQLiteDataReader</returns>
        public static SQLiteDataReader ExecuteReader(string SQLString, params SQLiteParameter[] cmdParms)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SQLiteDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (SQLiteException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// ִ�в�ѯ��䣬����DataSet
        /// </summary>
        /// <param name="SQLString">��ѯ���</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params SQLiteParameter[] cmdParms)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (SQLiteException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }


        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, string cmdText, SQLiteParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SQLiteParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        #endregion

    }
}