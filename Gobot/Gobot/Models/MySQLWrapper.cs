using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Odbc;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace Gobot.Models
{
    public class MySQLWrapper
    {
        private OdbcConnection connection;

        public MySQLWrapper()
        {
            connection = new OdbcConnection("DRIVER={MySQL ODBC 5.3 Unicode Driver};SERVER=67.68.203.251;PORT=3306;DATABASE=gobot;USER=Max;PASSWORD=yolo;OPTION=3;");
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

                OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                foreach (OdbcParameter param in values)
                {
                    command.Parameters.Add(param);
                }

                return command.ExecuteNonQuery();
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
                
                OdbcCommand command = new OdbcCommand(sql.ToString(), connection);

                foreach(OdbcParameter val in values)
                {
                    command.Parameters.Add(val);
                }

                if(conditions.Count > 0 && where != "")
                {
                    foreach (OdbcParameter param in conditions)
                    {
                        command.Parameters.Add(param);
                    }
                }                

                return command.ExecuteNonQuery();
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
            if(connection != null && tablename != "")
            {
                StringBuilder sql = new StringBuilder("delete " + tablename);

                if(where != "")
                {
                    sql.Append(" where " + where);
                }

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
                StringBuilder sql = new StringBuilder("call " + procedurename + "(");

                if (args.Length > 0)
                {
                    foreach (OdbcParameter arg in args)
                    {
                        sql.Append("?,");
                    }
                    sql.Remove(sql.Length - 1, 1);
                }
                sql.Append(")");

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

                adapt.Fill(result);
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
                sessionuser.ProfilPic = UserResult.Rows[0]["Image"].ToString().Replace("=\"\" ", "/");
                if (UserResult.Rows[0]["Credit"].GetType() != typeof(System.DBNull))
                {
                    sessionuser.Credits = (int)UserResult.Rows[0]["Credit"];
                }
                else
                {
                    sessionuser.Credits = 0;
                }
                sessionuser.SteamID = UserResult.Rows[0]["SteamProfile"].ToString();
                if (UserResult.Rows[0]["Win"].GetType() != typeof(System.DBNull))
                {
                    sessionuser.Wins = (int)UserResult.Rows[0]["Win"];
                }
                else
                {
                    sessionuser.Wins = 0;
                }
                if (UserResult.Rows[0]["Game"].GetType() != typeof(System.DBNull))
                {
                    sessionuser.Games = (int)UserResult.Rows[0]["Game"];
                }
                else
                {
                    sessionuser.Games = 0;
                }
                if (UserResult.Rows[0]["TotalCredit"].GetType() != typeof(System.DBNull))
                {
                    sessionuser.TotalCredits = (int)UserResult.Rows[0]["TotalCredit"];
                }
                else
                {
                    sessionuser.TotalCredits = 0;
                }
                if (UserResult.Rows[0]["EXP"].GetType() != typeof(System.DBNull))
                {
                    sessionuser.EXP = (int)UserResult.Rows[0]["EXP"];
                }
                else
                {
                    sessionuser.EXP = 0;
                }
                if (UserResult.Rows[0]["LVL"].GetType() != typeof(System.DBNull))
                {
                    sessionuser.Level = (int)UserResult.Rows[0]["LVL"];
                }
                else
                {
                    sessionuser.Level = 1;
                }

                return sessionuser;
            }
            else
            {
                return null;
            }
        }

        public List<Match> GetFutureMatches(bool future)
        {
            MySQLWrapper Bd = new MySQLWrapper();
            List<Match> matches = new List<Match>();
            List<Team> teams = new List<Team>();

            if (future)
            {
                DataTable FutureMatches = Bd.Procedure("GetMatchAfter", new OdbcParameter(":date", DateTime.Now));
                DataTable AllTeams = GetAllTeam();
                foreach (DataRow row in FutureMatches.Rows)
                {
                    Match m = new Match();
                    m.Id = (int)row["IdMatch"];
                    m.Date = (DateTime)row["Date"];
                    m.Teams[0] = null;
                    m.Teams[1] = null;

                    foreach (Team t in teams)
                    {
                        if ((int)row["Team_IdTeam1"] == t.Id)
                        {
                            m.Teams[0] = t;
                        }
                        if ((int)row["Team_IdTeam2"] == t.Id)
                        {
                            m.Teams[1] = t;
                        }
                        if (m.Teams[0] != null && m.Teams[1] != null)
                        {
                            break;
                        }
                    }

                    matches.Add(m);
                }

                return matches;
            }
            else
            {
                DataTable PassMatches = Bd.Procedure("GetMatchBefore", new OdbcParameter(":date", DateTime.Now));
                DataTable AllTeams = GetAllTeam();
                foreach (DataRow row in PassMatches.Rows)
                {
                    Match m = new Match();
                    m.Id = (int)row["IdMatch"];
                    m.Date = (DateTime)row["Date"];
                    m.Teams[0] = null;
                    m.Teams[1] = null;

                    foreach (Team t in teams)
                    {
                        if ((int)row["Team_IdTeam1"] == t.Id)
                        {
                            m.Teams[0] = t;
                        }
                        if ((int)row["Team_IdTeam2"] == t.Id)
                        {
                            m.Teams[1] = t;
                        }
                        if (m.Teams[0] != null && m.Teams[1] != null)
                        {
                            break;
                        }
                    }

                    matches.Add(m);
                }

                return matches;
            }
        }
    }
        
    public List<Team> GetAllTeam()
    {
        MySQLWrapper Bd = new MySQLWrapper();
        List<Team> teams = new List<Team>();

        DataTable AllTeams = Bd.Procedure("GetAllTeam");
        foreach (DataRow row in AllTeams.Rows)
        {
            Team t = new Team();
            t.Id = (int)row["IdTeam"];
            t.Name = row["Name"].ToString();
            t.Wins = (int)row["Win"];
            t.Games = (int)row["Game"];
            t.ImagePath = row["ImageTeam"].ToString();

            DataTable BotsFromTeam = Bd.Procedure("BotFromTeam", new OdbcParameter(":IdTeam", row["IdTeam"]));
            for (int i = 0; i < 5; i++)
            {
                t.TeamComp[i] = new Bot(
                    (int)BotsFromTeam.Rows[i]["IdBot"], BotsFromTeam.Rows[i]["NomBot"].ToString(),
                    Convert.ToInt32(BotsFromTeam.Rows[i]["KDA"].ToString().Split('/')[0]),
                    Convert.ToInt32(BotsFromTeam.Rows[i]["KDA"].ToString().Split('/')[1]),
                    Convert.ToInt32(BotsFromTeam.Rows[i]["KDA"].ToString().Split('/')[2]));
            }

            teams.Add(t);
        }
        return teams;
    }
}
