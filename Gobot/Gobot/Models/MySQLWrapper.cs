using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Odbc;
using System.Text;

namespace Gobot.Models
{
    public class MySQLWrapper
    {
        private OdbcConnection connection;

        public MySQLWrapper(string user, string password)
        {
            try
            {
                connection = new OdbcConnection("DRIVER={MySQL ODBC 5.2 UNICODE Driver};Database=gobot;server=67.68.203.251;Port=3306;UID=" + user + ";PWD=" + password + ";");
            }
            catch(OdbcException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Select columns from a table with where condition
        /// </summary>
        /// <param name="tablename">Name of the table</param>
        /// <param name="where">Condition (ex. Alias = ? AND Name = ? OR Surname = ?</param>
        /// <param name="condition">Collection of OdbcParameter (In the same order as in "where" condition</param>
        /// <param name="columnnames">Names of the needed columns</param>
        /// <returns></returns>
        public string[] Select(string tablename, string where, OdbcParameterCollection conditions, params string[] columnnames)
        {
            if(columnnames.Length > 0)
            {
                StringBuilder sql = new StringBuilder("select ");

                foreach(string col in columnnames)
                {
                    sql.Append(col + ',');
                }
                sql.Remove(sql.Length - 1, 1);

                sql.Append(" from " + tablename + "where " + where);

                OdbcCommand command = new OdbcCommand();
            }
            
        }
    }
}