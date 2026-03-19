using System;
using System.Data;
using System.Data.SqlClient;

namespace cured
{
    class DataBase
    {
        // ВНИМАНИЕ: Проверьте строку подключения! Она должна быть такой же, как в ваших рабочих формах.
        private string connectionString = @"Data Source=DESKTOP-O67QLR8\CURSED;Initial Catalog=CURSED;Integrated Security=True";

        SqlConnection con;

        public DataBase()
        {
            con = new SqlConnection(connectionString);
        }

        // Метод для ваших старых форм
        public SqlConnection getConnection()
        {
            return con;
        }

        public void openConnection()
        {
            if (con.State == ConnectionState.Closed) con.Open();
        }

        public void closeConnection()
        {
            if (con.State == ConnectionState.Open) con.Close();
        }

        // Новый метод для добавления в корзину
        public void ExecuteNonQuery(string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Выводим ошибку, чтобы понять, что не так с SQL или строкой подключения
                throw new Exception("Ошибка БД: " + ex.Message);
            }
        }

        public DataTable ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(query, connectionString))
            {
                da.Fill(dt);
            }
            return dt;
        }
    }
}