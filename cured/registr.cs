using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace cured
{
    /// <summary>
    /// ФОРМА: РЕГИСТРАЦИЯ
    /// </summary>
    public partial class registr : Form
    {
        private DataBase database = new DataBase();

        public registr()
        {
            InitializeComponent();
            button1.Enabled = false; // Кнопка заблокирована до принятия соглашения
        }

        #region 1. ОСНОВНАЯ ЛОГИКА РЕГИСТРАЦИИ

        /// <summary>
        /// ОБРАБОТЧИК: Регистрация пользователя (button1)
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            string name_user = textBox1.Text.Trim();
            string login_user = textBox2.Text.Trim();
            string password_user = textBox3.Text.Trim();
            string phone_user = maskedTextBox1.Text;

            // Валидация заполнения полей
            if (string.IsNullOrEmpty(name_user) || string.IsNullOrEmpty(login_user) || string.IsNullOrEmpty(password_user))
            {
                MessageBox.Show("Пожалуйста, заполните все поля формы!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверки безопасности и корректности данных
            if (login_user.Length < 4) { MessageBox.Show("Логин должен содержать минимум 4 символа!", "Ошибка"); return; }
            if (password_user.Length < 4) { MessageBox.Show("Пароль должен содержать минимум 4 символа!", "Ошибка"); return; }
            if (!maskedTextBox1.MaskFull) { MessageBox.Show("Введите номер телефона полностью!", "Внимание"); return; }

            // Проверка на дубликат логина
            if (checkuser()) return;

            // SQL запрос на вставку данных
            string querystring = "INSERT INTO Users(FullName, Login, Password, Phone) VALUES(@name, @login, @pass, @phone)";
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
            }
            catch (Exception ex) { MessageBox.Show("Ошибка подключения: " + ex.Message); }
            finally { database.closeConnection(); }
        }

        /// <summary>
        /// МЕТОД: Проверка существования логина в БД
        /// </summary>
        private Boolean checkuser()
        {
            string query = "SELECT UserID FROM Users WHERE Login = @login";
            SqlCommand command = new SqlCommand(query, database.getConnection());
            command.Parameters.AddWithValue("@login", textBox2.Text.Trim());

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Пользователь с таким логином уже есть!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        #endregion

        #region 2. СОГЛАСИЕ НА ОБРАБОТКУ ДАННЫХ

        /// <summary>
        /// ОБРАБОТЧИК: Чекбокс согласия
        /// </summary>
        private void checkAgreement_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = checkAgreement.Checked;
        }

        /// <summary>
        /// ОБРАБОТЧИК: Просмотр деталей политики (labelDetails)
        /// </summary>
        private void labelDetails_Click(object sender, EventArgs e)
        {
            string title = "Политика обработки персональных данных";
            string info = "Настоящим подтверждаю свое согласие на обработку моих данных...\n\n" +
                          "1. ПЕРЕЧЕНЬ: ФИО, телефон, история заказов.\n" +
                          "2. ЦЕЛИ: Управление личным кабинетом, связь по заказам.\n" +
                          "3. ЗАЩИТА: Данные не передаются третьим лицам.";
            MessageBox.Show(info, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region 3. ПАРОЛЬ (ВИДИМОСТЬ)

        /// <summary>
        /// ОБРАБОТЧИК: Показать/Скрыть пароль (btnShowClosePass)
        /// </summary>
        private void btnShowClosePass_Click(object sender, EventArgs e)
        {
            if (textBox3.PasswordChar == '*')
            {
                textBox3.PasswordChar = '\0'; // Показать
                btnShowClosePass.Text = "Скрыть пароль";
            }
            else
            {
                textBox3.PasswordChar = '*'; // Скрыть
                btnShowClosePass.Text = "Показать пароль";
            }
        }

        #endregion

        #region 4. НАВИГАЦИЯ

        /// <summary>
        /// ОБРАБОТЧИК: Ссылка "Войти" (label7)
        /// </summary>
        private void label7_Click(object sender, EventArgs e)
        {
            login f = new login();
            f.Show();
            this.Hide();
        }

        /// <summary>
        /// ОБРАБОТЧИК: Закрытие формы регистрации
        /// </summary>
        private void registr_FormClosed(object sender, FormClosedEventArgs e)
        {
            main f = new main();
            f.Show();
        }

        /// <summary>
        /// ОБРАБОТЧИК: Нажатие Enter в поле телефона
        /// </summary>
        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && button1.Enabled)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        #endregion
    }
}