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
    public partial class admin : Form
    {
        DataBase database = new DataBase();

        public admin()
        {
            InitializeComponent();
        }

        private void admin_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void admin_Load(object sender, EventArgs e)
        {
            label1.Text = "Панель администратора";
            label2.Text = $"Администратор: {user.full_name}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Кнопка "На главную"
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Кнопка "Выйти"
            acc_checked.acc_check = false;
            user.login_user = "";
            user.full_name = "";
            user.phone = "";
            user.role = "";

            main form_main = new main();
            form_main.Show();
            this.Hide();
        }
    }
}