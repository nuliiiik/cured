using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace cured
{
    /// <summary>
    /// Форма авторизации пользователей.
    /// Обеспечивает проверку учетных данных и инициализацию глобальной сессии пользователя.
    /// </summary>
    public partial class login : Form
    {
        #region ИНИЦИАЛИЗАЦИЯ И ЗАГРУЗКА

        // Подключение к нашей базе данных через вспомогательный класс
        DataBase database = new DataBase();

        public login()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Событие при загрузке формы: настройка полей ввода по умолчанию.
        /// </summary>
        private void login_Load(object sender, EventArgs e)
        {
            // Скрываем символы пароля звездочками для безопасности
            textBox2.PasswordChar = '*';
            // Устанавливаем начальный текст кнопки переключения видимости
            btnShowClosePass.Text = "Показать пароль";
        }

        #endregion

        #region ЛОГИКА ОТОБРАЖЕНИЯ ПАРОЛЯ

        /// <summary>
        /// Переключает режим видимости пароля (маскировка символов).
        /// </summary>
        private void btnShowClosePass_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '*')
            {
                // Убираем маскировку (показываем текст)
                textBox2.PasswordChar = '\0';
                btnShowClosePass.Text = "Скрыть пароль";
            }
            else
            {
                // Возвращаем маскировку звездочками
                textBox2.PasswordChar = '*';
                btnShowClosePass.Text = "Показать пароль";
            }
        }

        #endregion

        #region ОСНОВНАЯ ЛОГИКА ВХОДА (АВТОРИЗАЦИЯ)

        /// <summary>
        /// Обработка нажатия кнопки "Войти".
        /// Выполняет поиск пользователя в базе по паре Логин/Пароль.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // Считываем данные из текстовых полей
            string login_user = textBox1.Text;
            string password_user = textBox2.Text;

            // Подготавливаем инструменты для работы с SQL
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            // Формируем SQL-запрос для получения данных профиля
            // Внимание: Данный метод подвержен SQL-инъекциям, в будущем лучше использовать параметры (Parameters.Add)
            string querystring = $"SELECT UserID, Login, FullName, Phone, Role FROM Users " +
                                 $"WHERE Login = '{login_user}' AND Password = '{password_user}'";

            SqlCommand command = new SqlCommand(querystring, database.getConnection());

            try
            {
                // Выполняем запрос и заполняем таблицу результатами
                adapter.SelectCommand = command;
                adapter.Fill(table);

                // Проверяем: если в таблице ровно одна строка — пользователь найден
                if (table.Rows.Count == 1)
                {
                    // Сохраняем данные во временный статический класс 'user'. 
                    // Это позволит нам знать, кто залогинен, на любой другой форме (например, в корзине).
                    user.id_user = Convert.ToInt32(table.Rows[0]["UserID"]);
                    user.login_user = table.Rows[0]["Login"].ToString();
                    user.full_name = table.Rows[0]["FullName"].ToString();
                    user.phone = table.Rows[0]["Phone"].ToString();
                    user.role = table.Rows[0]["Role"].ToString();

                    // Ставим глобальную отметку успешного входа
                    acc_checked.acc_check = true;

                    MessageBox.Show($"Добро пожаловать, {user.full_name}!", "Успешно!",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Перенаправление в зависимости от роли пользователя (Админ или Обычный)
                    if (user.role == "admin")
                    {
                        new admin().Show();
                        this.Hide();
                    }
                    else
                    {
                        new main().Show();
                        this.Hide();
                    }
                }
                else
                {
                    // Если совпадений нет, выводим предупреждение
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка доступа",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // Обработка непредвиденных ошибок (например, отсутствие связи с сервером БД)
                MessageBox.Show("Ошибка при попытке входа: " + ex.Message, "Критическая ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region УДОБСТВО И НАВИГАЦИЯ

        /// <summary>
        /// Позволяет совершить вход нажатием клавиши Enter прямо в поле пароля.
        /// </summary>
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick(); // Программно нажимаем кнопку "Войти"
                e.SuppressKeyPress = true; // Отключаем стандартный "писк" Windows при нажатии Enter
            }
        }

        /// <summary>
        /// Дублирование логики Enter для кнопки входа (для фокуса на кнопке).
        /// </summary>
        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Переход на форму регистрации новых пользователей.
        /// </summary>
        private void label4_Click(object sender, EventArgs e)
        {
            registr form_registr = new registr();
            form_registr.Show();
            this.Hide();
        }

        /// <summary>
        /// Возврат в главное меню при закрытии окна авторизации.
        /// </summary>
        private void login_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        #endregion
    }
}