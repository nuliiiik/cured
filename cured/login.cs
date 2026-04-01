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
        #region Инициализация

        // Подключение к нашей базе данных
        DataBase database = new DataBase();

        public login()
        {
            InitializeComponent();
        }

        private void login_Load(object sender, EventArgs e)
        {
            // Скрываем символы пароля при загрузке формы
            textBox2.PasswordChar = '*';
            // Устанавливаем начальный текст кнопки
            btnShowClosePass.Text = "Показать пароль";
        }

        #endregion

        #region Логика отображения пароля

        /// <summary>
        /// Переключает видимость пароля в поле ввода.
        /// </summary>
        private void btnShowClosePass_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '*')
            {
                // Показываем пароль
                textBox2.PasswordChar = '\0'; // '\0' означает отсутствие маскировки
                btnShowClosePass.Text = "Скрыть пароль";
            }
            else
            {
                // Скрываем пароль
                textBox2.PasswordChar = '*';
                btnShowClosePass.Text = "Показать пароль";
            }
        }

        #endregion

        #region Основная логика входа

        /// <summary>
        /// Обработка нажатия кнопки "Войти".
        /// Выполняет поиск пользователя в базе по паре Логин/Пароль.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            string login_user = textBox1.Text;
            string password_user = textBox2.Text;

            // Создаем адаптер и таблицу для хранения результата запроса
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            // SQL-запрос для получения полной информации о профиле при совпадении данных
            string querystring = $"SELECT UserID, Login, FullName, Phone, Role FROM Users " +
                                 $"WHERE Login = '{login_user}' AND Password = '{password_user}'";

            SqlCommand command = new SqlCommand(querystring, database.getConnection());

            try
            {
                adapter.SelectCommand = command;
                adapter.Fill(table);

                // Если найдена ровно одна запись — данные верны
                if (table.Rows.Count == 1)
                {
                    // Сохраняем данные во временный статический класс 'user' для доступа из других форм
                    user.id_user = Convert.ToInt32(table.Rows[0]["UserID"]);
                    user.login_user = table.Rows[0]["Login"].ToString();
                    user.full_name = table.Rows[0]["FullName"].ToString();
                    user.phone = table.Rows[0]["Phone"].ToString();
                    user.role = table.Rows[0]["Role"].ToString();

                    // Флаг того, что проверка пройдена успешно
                    acc_checked.acc_check = true;

                    MessageBox.Show($"Добро пожаловать, {user.full_name}!", "Успешно!",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Переход на главную форму
                    main form_main = new main();
                    form_main.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка доступа",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке входа: " + ex.Message, "Критическая ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Удобство использования (Enter и Навигация)

        /// <summary>
        /// Позволяет пользователю входить в систему, просто нажав Enter в поле пароля.
        /// </summary>
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick(); // Имитируем клик по кнопке "Войти"
                e.SuppressKeyPress = true; // Отключаем системный звук ошибки Enter
            }
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Переход на форму регистрации при нажатии на соответствующую надпись.
        /// </summary>
        private void label4_Click(object sender, EventArgs e)
        {
            registr form_registr = new registr();
            form_registr.Show();
            this.Hide();
        }

        /// <summary>
        /// Возврат в главное меню при закрытии окна логина.
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