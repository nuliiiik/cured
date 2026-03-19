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
    public partial class login : Form
    {
        DataBase database = new DataBase();

        public login()
        {
            InitializeComponent();
        }

        private void login_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            registr form_registr = new registr();
            form_registr.Show();
            this.Hide();
        }

        private void login_Load(object sender, EventArgs e)
        {
            textBox2.PasswordChar = '*';
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login_user = textBox1.Text;
            string password_user = textBox2.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            // Изменил запрос чтобы получать все данные пользователя
            string querystring = $"select UserID, Login, FullName, Phone, Role from Users where Login = '{login_user}' and Password = '{password_user}'";

            SqlCommand command = new SqlCommand(querystring, database.getConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                // Сохраняем все данные пользователя
                user.id_user = Convert.ToInt32(table.Rows[0]["UserID"]);
                user.login_user = table.Rows[0]["Login"].ToString();
                user.full_name = table.Rows[0]["FullName"].ToString();
                user.phone = table.Rows[0]["Phone"].ToString();
                user.role = table.Rows[0]["Role"].ToString();

                acc_checked.acc_check = true;

                MessageBox.Show($"Добро пожаловать, {user.full_name}!", "Успешно!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                main form_main = new main();
                form_main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Такого аккаунта не существует(", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Вызываем событие клика нужной кнопки
                button1.PerformClick();

                // Подавляем стандартный звук "пиканья" Windows при нажатии Enter
                e.SuppressKeyPress = true;
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Вызываем событие клика нужной кнопки
                button1.PerformClick();

                // Подавляем стандартный звук "пиканья" Windows при нажатии Enter
                e.SuppressKeyPress = true;
            }
        }
    }
}