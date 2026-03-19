using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public partial class ProductDetails : Form
    {
        private int productId;
        private string productType;
        private NumericUpDown numQty;
        private DataBase db = new DataBase();

        public ProductDetails(string art, string name, string price, string info, string desc, Image img, int id, string type)
        {
            this.Text = "Детали товара";
            Size targetSize = new Size(480, 780);
            this.Size = targetSize;
            this.MinimumSize = targetSize;
            this.MaximumSize = targetSize;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            this.productId = id;
            this.productType = type;

            InitializeComponent();
            InitializeCustomUI(art, name, price, info, desc, img);
        }

        private void InitializeCustomUI(string art, string name, string price, string info, string desc, Image img)
        {
            int marginLeft = 25;
            int contentWidth = this.ClientSize.Width - (marginLeft * 2);

            PictureBox pb = new PictureBox
            {
                Location = new Point(marginLeft, 20),
                Size = new Size(contentWidth, 260),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = img,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblName = new Label
            {
                Location = new Point(marginLeft, 300),
                Size = new Size(contentWidth, 35),
                Text = name,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            Label lblPrice = new Label
            {
                Location = new Point(marginLeft, 345),
                Size = new Size(contentWidth, 40),
                Text = price,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.Green
            };

            // 1 и 2: ОПИСАНИЕ В КОРОТКОМ LABEL
            Label lblDesc = new Label
            {
                Location = new Point(marginLeft, 400),
                Size = new Size(contentWidth, 60), // Небольшая длина (высота)
                Text = desc,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                AutoEllipsis = true // Добавит "..." если описание слишком длинное
            };

            Label lblQtyTitle = new Label
            {
                Location = new Point(marginLeft, 480),
                Text = "Количество:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            numQty = new NumericUpDown
            {
                Location = new Point(marginLeft + 120, 478),
                Size = new Size(80, 26),
                Minimum = 1,
                Value = 1,
                Font = new Font("Segoe UI", 11)
            };

            Button btnAdd = new Button
            {
                Location = new Point((this.ClientSize.Width - 280) / 2, 530),
                Size = new Size(280, 50),
                Text = "ДОБАВИТЬ В КОРЗИНУ",
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { pb, lblName, lblPrice, lblDesc, lblQtyTitle, numQty, btnAdd });
        }

        // ВАЖНО: Удалите или закомментируйте метод FormClosed, если он у вас был такой:
        // private void ProductDetails_FormClosed(...) { ... main form_main = new main(); ... }
        // Окно должно просто закрываться.

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) { MessageBox.Show("Войдите в аккаунт!"); return; }

            string column = (productType == "moto") ? "MotorcycleID" : "PartID";
            string query = $"INSERT INTO Cart (UserID, {column}, Quantity) VALUES ({user.id_user}, {productId}, {(int)numQty.Value})";

            try
            {
                db.ExecuteNonQuery(query);
                MessageBox.Show("Добавлено!");
                this.Close(); // Просто закрываем текущее окно
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}