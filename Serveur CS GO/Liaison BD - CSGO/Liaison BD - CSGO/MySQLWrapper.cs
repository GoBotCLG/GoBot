using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Odbc;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Liaison_BD___CSGO
{
    public class MySQLWrapper
    {
        private MySqlConnection connection;

        public MySQLWrapper()
        {
            Connect();
        }

        ~MySQLWrapper() { try { connection.Close(); } catch (Exception) { } }

        public void Connect()
        {
            connection = new MySqlConnection("Server=MYSQL5014.SmarterASP.NET;Database=db_a13e4f_gobotdb;Uid=a13e4f_gobotdb;Pwd=Yolo1234Sw4g1234");
            //connection = new MySqlConnection("Server=MYSQL5014.SmarterASP.NET;Database=db_a13e4f_gobotdb;Uid=a13e4f_gobotdb;Pwd=Yolo1234Sw4g1234");
            //connection = new MySqlConnection("DRIVER={MySQL ODBC 5.3 Unicode Driver};SERVER=70.54.173.42;PORT=3306;DATABASE=gobot;USER=User;PASSWORD=yolo;OPTION=3;");
            try { connection.Open(); }
            catch (Exception e)
            {
                string ex = e.Message;
            }
        }

        /// <summary>
        /// Select columns from a table with where condition
        /// </summary>
        /// <param name="tablename">Name of the table</param>
        /// <param name="where">Condition (ex. Alias = ? AND Name = ? OR Surname = ?</param>
        /// <param name="condition">Collection of OdbcParameter (In the same order as in "where" condition</param>
        /// <param name="columnnames">Names of the needed columns</param>
        /// <returns>2D list of data returned by the query</returns>
        public DataTable Select(string tablename, string where, List<MySqlParameter> conditions, params string[] columnnames)
        {
            if (connection.State == ConnectionState.Open && connection != null && columnnames.Length > 0 && tablename != "")
            {
                StringBuilder sql = new StringBuilder("select ");

                foreach (string col in columnnames)
                {
                    sql.Append(col + ',');
                }
                sql.Remove(sql.Length - 1, 1);

                sql.Append(" from " + tablename);

                if (where != "")
                {
                    sql.Append(" where " + where);

                }

                MySqlCommand command = new MySqlCommand(sql.ToString(), connection);

                if (conditions.Count > 0)
                {
                    foreach (MySqlParameter param in conditions)
                    {
                        command.Parameters.Add(param);
                    }
                }

                MySqlDataAdapter adapt = new MySqlDataAdapter(command);

                DataTable result = new DataTable();

                adapt.Fill(result);
                result.TableName = tablename;

                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes a stored procedure
        /// </summary>
        /// <param name="procedurename">Name of the procedure</param>
        /// <param name="args">All the parameters for the procedure</param>
        public DataTable Procedure(string procedurename, params MySqlParameter[] args)
        {
            if (connection.State == ConnectionState.Open && connection != null && procedurename != "")
            {
                StringBuilder sql = new StringBuilder("call " + procedurename + "(");

                if (args.Length > 0)
                {
                    foreach (MySqlParameter arg in args)
                    {
                        sql.Append("?,");
                    }
                    sql.Remove(sql.Length - 1, 1);
                }
                sql.Append(");");

                MySqlCommand command = new MySqlCommand(sql.ToString(), connection);

                foreach (MySqlParameter arg in args)
                {
                    command.Parameters.Add(arg);
                }
                DataTable result = new DataTable();
                MySqlDataAdapter adapt = new MySqlDataAdapter(command);
                adapt.Fill(result);
                StringBuilder sb = new StringBuilder();
                sb.Append(procedurename + "(");
                foreach (MySqlParameter param in args)
                {
                    sb.Append(param.Value + ",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
                result.TableName = sb.ToString();

                return result;
            }
            else
            {
                return null;
            }
        }
    }
        public class MySQLWrapperODBC
    {
        private OdbcConnection connection;

        public MySQLWrapperODBC()
        {
            connection = new OdbcConnection("DRIVER={MySQL ODBC 5.3 Unicode Driver};Server=MYSQL5014.SmarterASP.NET;Database=db_a13e4f_gobotdb;Uid=a13e4f_gobotdb;Pwd=Yolo1234Sw4g1234");
            //connection = new OdbcConnection("DRIVER={MySQL ODBC 5.3 Unicode Driver};SERVER=70.54.173.42;PORT=3306;DATABASE=gobot;USER=User;PASSWORD=yolo;OPTION=3;");
            connection.Open();
        }

        ~MySQLWrapperODBC()
        {
            connection.Close();
            connection.Dispose();
        }

        /// <summary>
        /// Select columns from a table with where condition
        /// </summary>
        /// <param name="tablename">Name of the table</param>
        /// <param name="where">Condition (ex. Alias = ? AND Name = ? OR Surname = ?</param>
        /// <param name="condition">Collection of OdbcParameter (In the same order as in "where" condition</param>
        /// <param name="columnnames">Names of the needed columns</param>
        /// <returns>2D list of data returned by the query</returns>
        public DataTable Select(string tablename, string where, List<OdbcParameter> conditions, params string[] columnnames)
        {
            if (connection != null && columnnames.Length > 0 && tablename != "")
            {
                StringBuilder sql = new StringBuilder("select ");

                foreach (string col in columnnames)
                {
                    sql.Append(col + ',');
                }
                sql.Remove(sql.Length - 1, 1);

                sql.Append(" from " + tablename);
                
                if(where != "")
                {
                    sql.Append(" where " + where);

                }
                OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                if(conditions.Count > 0)
                {
                    foreach (OdbcParameter param in conditions)
                    {
                        command.Parameters.Add(param);
                    }
                }

                OdbcDataAdapter adapt = new OdbcDataAdapter(command);

                DataTable result = new DataTable();
                Monitor.Enter(connection);
                try
                {
                    adapt.Fill(result);
                }
                finally
                {
                    Monitor.Exit(connection);
                }
                result.TableName = tablename;
                adapt.Dispose();
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Insert one row in a table
        /// </summary>
        /// <param name="tablename">Name of the table</param>
        /// <param name="columnNames">List of column names to insert</param>
        /// <param name="values">List of parameters for values to insert</param>
        /// <returns>Number of rows inserted (1 if inserted or 0 if error)</returns>
        public int Insert(string tablename, List<string> columnNames, List<OdbcParameter> values)
        {
            if (connection != null && columnNames.Count > 0 && values.Count > 0 && tablename != "")
            {
                StringBuilder sql = new StringBuilder("insert into " + tablename + "(");

                foreach (string val in columnNames)
                {
                    sql.Append(val + ",");
                }
                sql.Remove(sql.Length - 1, 1);
                sql.Append(") values(");

                foreach (OdbcParameter val in values)
                {
                    sql.Append("?,");
                }
                sql.Remove(sql.Length - 1, 1);
                sql.Append(")");

                Monitor.Enter(connection);
                try
                {
                    OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                    foreach (OdbcParameter param in values)
                    {
                        command.Parameters.Add(param);
                    }
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    Monitor.Exit(connection);
                }
                
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Update one or multiple columns on rows matching with where statement
        /// </summary>
        /// <param name="tablename">Name of the table</param>
        /// <param name="columnNames">List of column names to update</param>
        /// <param name="values">List of values to put in the table</param>
        /// <param name="where">Condition (ex. Alias = ? AND Name = ? OR Firstname = ?</param>
        /// <param name="conditions">Collection of OdbcParameter (In the same order as in "where" condition</param>
        /// <returns>Number of rows updated</returns>
        public int Update(string tablename, List<string> columnNames, List<OdbcParameter> values, string where, List<OdbcParameter> conditions)
        {
            if (connection != null && columnNames.Count > 0 && values.Count > 0 && tablename != "")
            {
                StringBuilder sql = new StringBuilder("update " + tablename + " set ");

                foreach(string col in columnNames)
                {
                    sql.Append(col + " = ?,");
                }
                sql.Remove(sql.Length - 1, 1);

                if(where != "" && conditions.Count > 0)
                {
                    sql.Append(" where " + where);
                }

                Monitor.Enter(connection);
                try
                {
                    OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                    foreach (OdbcParameter val in values)
                    {
                        command.Parameters.Add(val);
                    }

                    if (conditions.Count > 0 && where != "")
                    {
                        foreach (OdbcParameter param in conditions)
                        {
                            command.Parameters.Add(param);
                        }
                    }
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    Monitor.Exit(connection);
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Delete rows matching with the where statement
        /// </summary>
        /// <param name="tablename">Name of the table</param>
        /// <param name="where">Condition (ex. Alias = ? AND Name = ? OR FirstName = ?</param>
        /// <param name="conditions">Collection of OdbcParameter (In the same order as in "where" condition</param>
        /// <returns></returns>
        public int Delete(string tablename, string where, List<OdbcParameter> conditions)
        {
            if (connection != null && tablename != "")
            {
                StringBuilder sql = new StringBuilder("delete " + tablename);

                if(where != "")
                {
                    sql.Append(" where " + where);
                }

                Monitor.Enter(connection);
                try
                {
                    OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                    if(conditions.Count > 0)
                    {
                        foreach(OdbcParameter param in conditions)
                        {
                            command.Parameters.Add(param);
                        }
                    }
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    Monitor.Exit(connection);
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Executes a stored procedure
        /// </summary>
        /// <param name="procedurename">Name of the procedure</param>
        /// <param name="args">All the parameters for the procedure</param>
        public DataTable Procedure(string procedurename, params OdbcParameter[] args)
        {
            if (connection != null && procedurename != "")
            {
                StringBuilder sql = new StringBuilder("{call " + procedurename + "(");

                if (args.Length > 0)
                {
                    foreach (OdbcParameter arg in args)
                    {
                        sql.Append("?,");
                    }
                    sql.Remove(sql.Length - 1, 1);
                }
                sql.Append(")}");
                OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                foreach (OdbcParameter arg in args)
                {
                    command.Parameters.Add(arg);
                }
                DataTable result = new DataTable();
                OdbcDataAdapter adapt = new OdbcDataAdapter(command);
                adapt.Fill(result);
                StringBuilder sb = new StringBuilder();
                sb.Append(procedurename + "(");
                foreach (OdbcParameter param in args)
                {
                    sb.Append(param.Value + ",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
                result.TableName = sb.ToString();
                adapt.Dispose();
                return result;
            }
            else
            {
                return null;
            }
        }

        public DataTable Function(string functionname, params OdbcParameter[] args)
        {
            if (connection != null && functionname != "")
            {
                StringBuilder sql = new StringBuilder("select " + functionname + "(");

                if (args.Length > 0)
                {
                    foreach (OdbcParameter arg in args)
                        sql.Append("?,");

                    sql.Remove(sql.Length - 1, 1);
                }

                sql.Append(")");
                
                OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                foreach (OdbcParameter arg in args)
                {
                    command.Parameters.Add(arg);
                }

                OdbcDataAdapter adapt = new OdbcDataAdapter(command);
                DataTable result = new DataTable();

                Monitor.Enter(connection);
                try
                {
                    adapt.Fill(result);
                }
                finally
                {
                    Monitor.Exit(connection);
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(functionname + "(");
                foreach(OdbcParameter param in args)
                {
                    sb.Append(param.Value + ",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
                result.TableName = sb.ToString();
                return result;
            }
            else
            {
                return null;
            }
        }

        public DateTime GetBDTime()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                try
                {
                    OdbcDataAdapter adapt = new OdbcDataAdapter(new OdbcCommand("select now();", connection));

                    DataTable result = new DataTable();
                    adapt.Fill(result);

                    string dbDate = result.Rows[0].ItemArray[0].ToString();
                    DateTime dt = DateTime.ParseExact(dbDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    return dt;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
            else
            {
                return DateTime.Now;
            }
        }

        /*
        public User GetUserFromDB(string username)
        {
            if (connection == null || username == "")
            {
                return null;
            }

            DataTable UserResult = Procedure("GetUser", new OdbcParameter(":Username", username));

            if (UserResult.Rows.Count > 0)
            {
                User sessionuser = new User();
                sessionuser.Username = UserResult.Rows[0]["Username"].ToString();
                sessionuser.Email = UserResult.Rows[0]["Email"].ToString();
                //if (UserResult.Rows[0]["Image"].GetType() != typeof(System.DBNull))
                //{
                //    byte[] imagebytes = (byte[])UserResult.Rows[0]["Image"];
                //    TypeConverter tc = TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
                //    sessionuser.ProfilPic = (System.Drawing.Bitmap)tc.ConvertFrom(imagebytes);
                //}
                sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                sessionuser.Level = (int)UserResult.Rows[0]["LVL"];

                return sessionuser;
            }
            else
            {
                return null;
            }
        }*/
    }
}