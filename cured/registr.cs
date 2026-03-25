using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace cured
{
    public partial class registr : Form
    {
        DataBase database = new DataBase();

        public registr()
        {
            InitializeComponent();
        }

        // Возврат на главную при закрытии
        private void registr_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        // Переход на окно входа
        private void label7_Click(object sender, EventArgs e)
        {
            login form_login = new login();
            form_login.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. Собираем данные из полей
            string name_user = textBox1.Text.Trim();
            string login_user = textBox2.Text.Trim();
            string password_user = textBox3.Text.Trim();
            string phone_user = maskedTextBox1.Text;

            // 2. ПРОВЕРКА: Все ли поля заполнены?
            // MaskedTextBox.MaskFull возвращает true, если маска заполнена до конца
            if (string.IsNullOrEmpty(name_user) ||
                string.IsNullOrEmpty(login_user) ||
                string.IsNullOrEmpty(password_user) ||
                !maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Пожалуйста, заполните ВСЕ поля формы!\nУкажите ФИО, Логин, Пароль и полный номер телефона.",
                                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Прерываем выполнение метода, запрос в БД не уйдет
            }

            // 3. Проверяем, не занят ли логин (вызываем твой метод)
            if (checkuser())
            {
                return;
            }

            // 4. Если всё ок — регистрируем
            string querystring = "insert into Users(FullName, Login, Password, Phone) values(@name, @login, @pass, @phone)";

            SqlCommand command = new SqlCommand(querystring, database.getConnection());
            command.Parameters.AddWithValue("@name", name_user);
            command.Parameters.AddWithValue("@login", login_user);
            command.Parameters.AddWithValue("@pass", password_user);
            command.Parameters.AddWithValue("@phone", phone_user);

            try
            {
                database.openConnection();
                if (command.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("Аккаунт успешно создан!", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    login form_login = new login();
                    form_login.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании аккаунта.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }
        }

        /// <summary>
        /// Проверяет, существует ли уже пользователь с таким ЛОГИНОМ.
        /// </summary>
        private Boolean checkuser()
        {
            string login_user = textBox2.Text;

            // Проверяем ТОЛЬКО логин (пароль при проверке уникальности не важен)
            string query = "SELECT UserID FROM Users WHERE Login = @login";
            SqlCommand command = new SqlCommand(query, database.getConnection());
            command.Parameters.AddWithValue("@login", login_user);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        // Обработка нажатия Enter на поле ввода телефона
        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true; // Отключаем звук "бип"
            }
        }
    }
}