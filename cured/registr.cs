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
            button1.Enabled = false;
        }

        #region ОСНОВНАЯ ЛОГИКА РЕГИСТРАЦИИ

        private void button1_Click(object sender, EventArgs e)
        {
            string name_user = textBox1.Text.Trim();
            string login_user = textBox2.Text.Trim();
            string password_user = textBox3.Text.Trim();
            string phone_user = maskedTextBox1.Text;

            if (string.IsNullOrEmpty(name_user) ||
                string.IsNullOrEmpty(login_user) ||
                string.IsNullOrEmpty(password_user))
            {
                MessageBox.Show("Пожалуйста, заполните все поля формы!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ПРОВЕРКИ ДЛИНЫ И ТЕЛЕФОНА
            if (login_user.Length < 4) { MessageBox.Show("Логин должен содержать минимум 4 символа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (password_user.Length < 4) { MessageBox.Show("Пароль должен содержать минимум 4 символа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!maskedTextBox1.MaskFull) { MessageBox.Show("Введите номер телефона полностью!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // ПРОВЕРКА НА СУЩЕСТВУЮЩИЙ ЛОГИН (ВАЖНО)
            if (checkuser())
            {
                // Если checkuser вернул true, значит логин занят, выходим из метода
                return;
            }

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
            }
            catch (Exception ex) { MessageBox.Show("Ошибка подключения: " + ex.Message); }
            finally { database.closeConnection(); }
        }

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

        #region СОГЛАСИЕ НА ОБРАБОТКУ ДАННЫХ
        private void checkAgreement_CheckedChanged(object sender, EventArgs e) { button1.Enabled = checkAgreement.Checked; }

        private void labelDetails_Click(object sender, EventArgs e)
        {
            string title = "Политика обработки персональных данных";
            string info = "Настоящим подтверждаю свое согласие на обработку моих персональных данных 'Магазин мототехники' на следующих условиях:\n\n" +
                          "1. ПЕРЕЧЕНЬ СОБИРАЕМЫХ ДАННЫХ:\n" +
                          "• Фамилия, Имя, Отчество (ФИО);\n" +
                          "• Контактный номер телефона;\n" +
                          "• Данные об истории заказов (состав корзины, дата покупки).\n\n" +
                          "2. ЦЕЛИ ОБРАБОТКИ:\n" +
                          "• Создание и управление личным кабинетом пользователя;\n" +
                          "• Идентификация стороны в рамках заказов и договоров;\n" +
                          "• Связь с пользователем для подтверждения наличия товара и уточнения деталей доставки;\n" +
                          "• Предоставление технической поддержки.\n\n" +
                          "3. ЗАЩИТА И ХРАНЕНИЕ:\n" +
                          "• Мы обязуемся не передавать ваши данные третьим лицам (кроме случаев, предусмотренных законом);\n" +
                          "• Пользователь имеет право запросить удаление аккаунта и всех связанных данных.\n\n" +
                          "Нажимая галочку и продолжая регистрацию, вы принимаете данные условия в полном объеме.";
            MessageBox.Show(info, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Логика отображения пароля

        /// <summary>
        /// Переключает видимость пароля в поле ввода.
        /// </summary>
        private void btnShowClosePass_Click(object sender, EventArgs e)
        {
            if (textBox3.PasswordChar == '*')
            {
                // Показываем пароль
                textBox3.PasswordChar = '\0'; // '\0' означает отсутствие маскировки
                btnShowClosePass.Text = "Скрыть пароль";
            }
            else
            {
                // Скрываем пароль
                textBox3.PasswordChar = '*';
                btnShowClosePass.Text = "Показать пароль";
            }
        }

        #endregion

        #region НАВИГАЦИЯ
        private void label7_Click(object sender, EventArgs e) { login f = new login(); f.Show(); this.Hide(); }
        private void registr_FormClosed(object sender, FormClosedEventArgs e) { main f = new main(); f.Show(); this.Hide(); }
        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter && button1.Enabled) { button1.PerformClick(); e.SuppressKeyPress = true; } }
        #endregion
    }
}