using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateFolders
{
    internal class DBMySQLUtils
    {
        public static MySqlConnection GetDBConnection(string host, int port, string username, string password)
        {
            // Connection String.
            string connString = "Server=" + host + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }
    }
}
