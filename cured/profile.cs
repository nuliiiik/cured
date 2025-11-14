using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cured
{
    public partial class profile : Form
    {
        public profile()
        {
            InitializeComponent();
        }

        private void profile_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Click(object sender, EventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void panel4_Click(object sender, EventArgs e)
        {

        }

        private void panel3_Click(object sender, EventArgs e)
        {

        }

        private void profile_Load(object sender, EventArgs e)
        {

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

            if (command.ExecuteNonQuery() == 1)
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
    }
}
