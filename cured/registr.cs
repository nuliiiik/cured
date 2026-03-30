using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace cured
{
    public partial class registr : Form
    {
        // Подключаем класс для работы с базой данных
        DataBase database = new DataBase();

        public registr()
        {
            InitializeComponent();

            // Устанавливаем кнопку регистрации выключенной при старте
            button1.Enabled = false;
        }

        #region ОСНОВНАЯ ЛОГИКА РЕГИСТРАЦИИ

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. Получаем данные из текстовых полей
            string name_user = textBox1.Text.Trim();     // ФИО
            string login_user = textBox2.Text.Trim();    // Логин
            string password_user = textBox3.Text.Trim(); // Пароль
            string phone_user = maskedTextBox1.Text;     // Номер телефона

            // 2. Проверка на заполнение всех обязательных полей
            if (string.IsNullOrEmpty(name_user) ||
                string.IsNullOrEmpty(login_user) ||
                string.IsNullOrEmpty(password_user) ||
                !maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Пожалуйста, заполните все поля формы!", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Проверка: не занят ли уже такой логин в базе
            if (checkuser()) return;

            // 4. SQL-запрос для добавления нового пользователя
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
                    MessageBox.Show("Аккаунт успешно создан!", "Успешно",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Переходим на форму входа после успеха
                    login form_login = new login();
                    form_login.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }
            finally
            {
                database.closeConnection();
            }
        }

        // Вспомогательный метод для проверки уникальности логина
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
                MessageBox.Show("Пользователь с таким логином уже есть!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        #endregion

        #region СОГЛАСИЕ НА ОБРАБОТКУ ДАННЫХ

        // Метод, который активирует кнопку button1 только при нажатом чекбоксе
        private void checkAgreement_CheckedChanged(object sender, EventArgs e)
        {
            // Свойство Enabled кнопки принимает значение свойства Checked чекбокса
            button1.Enabled = checkAgreement.Checked;
        }

        // Клик по надписи "Подробнее"
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

        #region НАВИГАЦИЯ И ГОРЯЧИЕ КЛАВИШИ

        // Переход на форму входа (кнопка/ссылка "Назад")
        private void label7_Click(object sender, EventArgs e)
        {
            login form_login = new login();
            form_login.Show();
            this.Hide();
        }

        // Возврат на главную при закрытии формы регистрации
        private void registr_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        // Позволяет нажать Enter для быстрой регистрации
        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && button1.Enabled)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true; // Убирает системный звук "пик"
            }
        }

        #endregion
    }
}