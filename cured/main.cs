using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cured
{
    public partial class main : Form
    {

        DataBase database = new DataBase();

        public main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(acc_checked.acc_check == true)
            {
                label5.Text = user.login_user;
                label5.TextAlign = ContentAlignment.MiddleRight;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
            if (acc_checked.acc_check == false) { 
            login form_login = new login();
            form_login.Show();
            this.Hide();
            }
            else{
                if (user.login_user == "admin")
                {
                    admin form_admin = new admin();
                    form_admin.Show();
                    this.Hide();
                }
                else
                {
                    profile form_profile = new profile();
                    form_profile.Show();
                    this.Hide();
                }
            }
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
           Application.Exit();
        }

        private void label5_Enter(object sender, EventArgs e)
        {
        }

        private void main_Enter(object sender, EventArgs e)
        {
           
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}