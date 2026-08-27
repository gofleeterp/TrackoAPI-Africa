using System.Data;
using System.Data.SqlClient;

namespace HangfireService.Tools
{
    public class DBHelper
    {
        private string connectionString { get; set; }

        public DBHelper(string connectionStringKey = "MainDBConnection")
        {
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
        }

        /// <summary>
        ///  执行SQL语句，返回受影响的行数(用于insert,delete,update等)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteSql(string strSQL)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(e.StackTrace);
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// 执行带参数的非查询SQL
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="values">参数</param>
        /// <returns>受影响行数</returns>
        public int ExecuteCommand(string strSQL, params SqlParameter[] values)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddRange(values);
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(string.Format("执行{0}失败:{1}", strSQL, ex.Message));
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        ///  执行查询SQL语句，返回SqlDataReader(只进记录集) ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string strSQL)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        //CommandBehavior.CloseConnection 能够保证当SqlDataReader对象被关闭时，其依赖的连接也会被自动关闭。
                        SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        return myReader;
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(string.Format("执行{0}失败:{1}", strSQL, ex.Message));
                        return null;
                    }
                }
            }
        }

        /// <summary>
        ///  执行带参数的查询SQL语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <param name="values">参数</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string strSQL, params SqlParameter[] values)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        //CommandBehavior.CloseConnection 能够保证当SqlDataReader对象被关闭时，其依赖的连接也会被自动关闭。
                        cmd.Parameters.AddRange(values);
                        SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        return myReader;
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(string.Format("执行{0}失败:{1}", strSQL, ex.Message));
                        return null;
                    }
                }
            }
        }


        /// <summary>
        ///  执行带参数的查询SQL，返回离线记录集
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public DataSet getDataSetbySQL(string strSQL, params SqlParameter[] values)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        cmd.Parameters.AddRange(values);
                        SqlDataAdapter myAdapter = new SqlDataAdapter(cmd);
                        myAdapter.Fill(ds);
                        return ds;
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(string.Format("执行{0}失败:{1}", strSQL, ex.Message));
                        return new DataSet();
                    }
                }
            }
        }

        /// <summary>
        ///  执行查询SQL语句，返回离线记录集
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <returns>离线记录DataSet</returns>
        public DataTable getDataTablebySQL(string strSQL)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        SqlDataAdapter myAdapter = new SqlDataAdapter(cmd);
                        myAdapter.Fill(ds);
                        return ds.Tables[0];
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(string.Format("执行{0}失败:{1}", strSQL, ex.Message));
                        return new DataTable();
                    }
                }
            }
        }

        /// <summary>
        ///  执行带参数的查询SQL，返回离线记录集
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public DataTable getDataTablebySQL(string strSQL, params SqlParameter[] values)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, conn))
                {
                    try
                    {
                        conn.Open();
                        DataSet ds = new DataSet();
                        cmd.Parameters.AddRange(values);
                        SqlDataAdapter myAdapter = new SqlDataAdapter(cmd);
                        myAdapter.Fill(ds);
                        return ds.Tables[0];
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        System.Diagnostics.Trace.WriteLine(string.Format("执行{0}失败:{1}", strSQL, ex.Message));
                        return new DataTable();
                    }
                }
            }
        }


    }
}
