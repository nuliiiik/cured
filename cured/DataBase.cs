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
        #region ПОЛЯ И НАСТРОЙКА ПОДКЛЮЧЕНИЯ

        private string connectionString; // Строка подключения со всеми параметрами
        private SqlConnection con;       // Объект соединения с SQL Server

        /// <summary>
        /// Конструктор класса. Автоматически формирует строку подключения, 
        /// адаптируясь под имя текущего компьютера.
        /// </summary>
        public DataBase()
        {
            // Используем построитель, чтобы избежать ошибок в синтаксисе строки
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            // Указываем адрес сервера (DataSource). В данном случае — локальный ПК пользователя.
            builder.DataSource = $@"nuliikk";

            // Название целевой базы данных, с которой будет работать программа
            builder.InitialCatalog = "CURSED";

            // Включаем Windows-аутентификацию (вход через текущую учетную запись ОС)
            builder.IntegratedSecurity = true;

            // Собираем итоговую строку и инициализируем объект подключения
            connectionString = builder.ConnectionString;
            con = new SqlConnection(connectionString);
        }

        #endregion

        #region УПРАВЛЕНИЕ СОСТОЯНИЕМ СОЕДИНЕНИЯ

        /// <summary>
        /// Возвращает объект активного соединения. 
        /// Используется в основном в админ-панели для работы SqlDataAdapter.
        /// </summary>
        public SqlConnection getConnection() => con;

        /// <summary>
        /// Открывает соединение с базой данных, если оно в данный момент закрыто.
        /// </summary>
        public void openConnection()
        {
            if (con.State == ConnectionState.Closed) con.Open();
        }

        /// <summary>
        /// Безопасно закрывает соединение с базой данных.
        /// </summary>
        public void closeConnection()
        {
            if (con.State == ConnectionState.Open) con.Close();
        }

        #endregion

        #region МЕТОДЫ ВЫПОЛНЕНИЯ ЗАПРОСОВ

        /// <summary>
        /// Выполняет команды "действия", которые не возвращают таблицу с данными.
        /// Применяется для добавления (INSERT), изменения (UPDATE) или удаления (DELETE) записей.
        /// </summary>
        /// <param name="query">SQL запрос на изменение данных.</param>
        public void ExecuteNonQuery(string query)
        {
            try
            {
                // Блок 'using' автоматически закроет соединение и освободит память даже при сбое
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery(); // Выполнение команды без возврата результата
                    }
                }
            }
            catch (Exception ex)
            {
                // Перебрасываем ошибку выше с понятным описанием
                throw new Exception("Ошибка выполнения команды (NonQuery): " + ex.Message);
            }
        }

        /// <summary>
        /// Выполняет запрос на выборку данных и возвращает результат в виде объекта DataTable.
        /// Идеально подходит для заполнения списков товаров или таблиц.
        /// </summary>
        /// <param name="query">SQL запрос на чтение (SELECT).</param>
        /// <returns>Таблица с результатами запроса.</returns>
        public DataTable ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();
            try
            {
                // Используем адаптер для автоматического заполнения таблицы результатами запроса
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
        /// Выполняет запрос и возвращает только одно единственное значение.
        /// Полезно для получения ID новой записи, суммы заказа (SUM) или количества (COUNT).
        /// </summary>
        /// <param name="query">SQL запрос.</param>
        /// <returns>Объект с результатом (первая колонка первой строки).</returns>
        public object ExecuteScalar(string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    // Метод ExecuteScalar намного быстрее, чем получение целой таблицы ради одного значения
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