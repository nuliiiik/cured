using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cured
{
    public partial class profile : Form
    {
        DataBase database = new DataBase();

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

        private void profile_Load(object sender, EventArgs e)
        {
            // Отображаем информацию о пользователе
            // Предполагаю что у тебя есть label2 для имени и label3 для телефона
            // Если нет - создай их или измени под свои контролы
            label2.Text = user.full_name;
            label3.Text = user.phone;
            label4.Text = user.login_user;
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            // Кнопка "На главную"
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void panel3_Click(object sender, EventArgs e)
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