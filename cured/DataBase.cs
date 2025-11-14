using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cured
{
    public class DataBase
    {
        SqlConnection con = new SqlConnection(@"Server=DESKTOP-O67QLR8\CURSED; Initial Catalog=cursed; Integrated Security=True;");

        public void openConnection()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
        }

        public void closeConnection()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }

        public SqlConnection getConnection()
        {
            return con;
        }

    }
}
