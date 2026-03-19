using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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

        private void registr_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            login form_login = new login();
            form_login.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name_user = textBox1.Text;
            string login_user = textBox2.Text;
            string password_user = textBox3.Text;
            string phone_user = maskedTextBox1.Text;

            string querystring = $"insert into Users(FullName, Login, Password, Phone) values('{name_user}','{login_user}','{password_user}','{phone_user}')";

            SqlCommand command = new SqlCommand(querystring, database.getConnection());

            if (checkuser())
            {
                return;
            }

            database.openConnection();

            if(command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Аккаунт успешно был создан", "Успешно!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                login form_login = new login();
                form_login.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Произошла ошибка, попробуйте снова!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            database.closeConnection();
        }

        private Boolean checkuser()
        {
            string login_user = textBox2.Text;
            string password_user = textBox3.Text;

            SqlDataAdapter adapter = new SqlDataAdapter(); 
            DataTable table = new DataTable();

            string querystring = $"select UserID, Login, Password from Users where Login = '{login_user}' and Password = '{password_user}'";

            SqlCommand command = new SqlCommand(querystring, database.getConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0) 
            {
                MessageBox.Show("Произошла ошибка, такой аккаунт уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void maskedTextBox1_KeyDown(object sender, KeyEventArgs e)
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
