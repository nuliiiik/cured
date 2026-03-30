using System;
using System.Data;
using System.Data.SqlClient;

namespace cured
{
    /// <summary>
    /// Класс для управления подключением и выполнения запросов к базе данных SQL Server.
    /// Централизует логику доступа к данным для всего приложения.
    /// </summary>
    class DataBase
    {
        #region Поля и Настройка подключения

        private string connectionString;
        private SqlConnection con;

        /// <summary>
        /// Конструктор класса. Автоматически формирует строку подключения, 
        /// адаптируясь под имя текущего компьютера.
        /// </summary>
        public DataBase()
        {
            // Используем построитель, чтобы избежать ошибок в синтаксисе строки
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            // Динамически подставляем имя ПК и экземпляр сервера \CURSED
            builder.DataSource = $@"{Environment.MachineName}\Имя_вашего_сервера";

            // Название целевой базы данных
            builder.InitialCatalog = "CURSED";

            // Использование системной учетной записи Windows для входа
            builder.IntegratedSecurity = true;

            connectionString = builder.ConnectionString;
            con = new SqlConnection(connectionString);
        }

        #endregion

        #region Управление состоянием соединения

        /// <summary>
        /// Возвращает объект активного соединения. 
        /// Используется в основном в админ-панели для работы SqlDataAdapter.
        /// </summary>
        public SqlConnection getConnection() => con;

        /// <summary>
        /// Открывает соединение с базой данных, если оно закрыто.
        /// </summary>
        public void openConnection()
        {
            if (con.State == ConnectionState.Closed) con.Open();
        }

        /// <summary>
        /// Закрывает соединение с базой данных.
        /// </summary>
        public void closeConnection()
        {
            if (con.State == ConnectionState.Open) con.Close();
        }

        #endregion

        #region Методы выполнения запросов

        /// <summary>
        /// Выполняет команды, которые не возвращают данные (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">SQL запрос.</param>
        public void ExecuteNonQuery(string query)
        {
            try
            {
                // Использование 'using' гарантирует закрытие соединения даже при ошибке
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
                throw new Exception("Ошибка выполнения команды (NonQuery): " + ex.Message);
            }
        }

        /// <summary>
        /// Выполняет запрос и возвращает результат в виде таблицы (DataTable).
        /// </summary>
        /// <param name="query">SQL запрос (обычно SELECT).</param>
        public DataTable ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlDataAdapter da = new SqlDataAdapter(query, connectionString))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения данных (Query): " + ex.Message);
            }
            return dt;
        }

        /// <summary>
        /// Выполняет запрос и возвращает только одно значение (первый столбец первой строки).
        /// Используется для получения ID нового заказа или подсчета суммы.
        /// </summary>
        /// <param name="query">SQL запрос.</param>
        public object ExecuteScalar(string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка получения единичного значения (Scalar): " + ex.Message);
            }
        }

        #endregion
    }
}