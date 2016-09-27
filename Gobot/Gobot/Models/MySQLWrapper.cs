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

        public string[] Select(string tablename, params string[] columnnames)
        {
            if(columnnames.Length > 0)
            {
                StringBuilder sql = new StringBuilder("select ");

                foreach(string col in columnnames)
                {
                    sql.Append(col + ',');
                }


                OdbcCommand command = new OdbcCommand() 
            }
            
        }
    }
}