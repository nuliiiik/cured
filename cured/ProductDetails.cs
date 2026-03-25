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
        private Label lblStock;
        private int currentStock = 0;
        private DataBase db = new DataBase();

        public ProductDetails(string art, string name, string price, string info, string desc, Image img, int id, string type)
        {
            this.productId = id;
            this.productType = type;
            this.Text = name;
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeComponent();
            GetStockFromDB();
            InitializeCustomUI(name, price, desc, img);
        }

        private void GetStockFromDB()
        {
            string table = (productType == "moto") ? "Motorcycles" : "Parts";
            string idCol = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM {table} WHERE {idCol} = {productId}");
            if (dt.Rows.Count > 0) currentStock = Convert.ToInt32(dt.Rows[0]["Quantity"]);
        }

        private void InitializeCustomUI(string name, string price, string desc, Image img)
        {
            int marginLeft = 25;
            int contentWidth = this.ClientSize.Width - (marginLeft * 2);

            PictureBox pb = new PictureBox { Location = new Point(marginLeft, 15), Size = new Size(contentWidth, 250), SizeMode = PictureBoxSizeMode.Zoom, Image = img };
            Label lblName = new Label { Location = new Point(marginLeft, 280), Size = new Size(contentWidth, 40), Text = name, Font = new Font("Segoe UI", 18, FontStyle.Bold) };

            Label lblPrice = new Label
            {
                Location = new Point(marginLeft, 325),
                Size = new Size(contentWidth, 45),
                Text = price,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = (currentStock > 0) ? Color.DarkRed : SystemColors.ButtonShadow
            };

            Label lblDesc = new Label
            {
                Location = new Point(marginLeft, 380),
                Size = new Size(contentWidth, 100),
                Text = string.IsNullOrEmpty(desc) ? "Описание скоро будет добавлено" : desc,
                Font = new Font("Segoe UI", 11),
                ForeColor = SystemColors.ButtonShadow
            };

            lblStock = new Label
            {
                Location = new Point(marginLeft, 500),
                Text = $"В наличии: {currentStock} шт.",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = (currentStock > 0) ? SystemColors.ButtonShadow : Color.DarkRed
            };

            numQty = new NumericUpDown
            {
                Location = new Point(marginLeft + 120, 538),
                Size = new Size(70, 29),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.DarkRed,
                Minimum = 1,
                Maximum = (currentStock > 0) ? currentStock : 1,
                Enabled = (currentStock > 0)
            };

            Button btnAdd = new Button
            {
                Location = new Point(marginLeft, 600),
                Size = new Size(contentWidth, 55),
                Text = (currentStock > 0) ? "ДОБАВИТЬ В КОРЗИНУ" : "ОТСУТСТВУЕТ",
                BackColor = (currentStock > 0) ? Color.DarkRed : SystemColors.ButtonShadow,
                Enabled = (currentStock > 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { pb, lblName, lblPrice, lblDesc, lblStock, numQty, btnAdd });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) { MessageBox.Show("Пожалуйста, войдите в аккаунт"); return; }

            int want = (int)numQty.Value;
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";

            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            int inCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;

            if (inCart + want > currentStock)
            {
                MessageBox.Show("Недостаточно товара на складе для такого количества");
            }
            else
            {
                if (inCart > 0)
                    db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity + {want} WHERE UserID = {user.id_user} AND {col} = {productId}");
                else
                    db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {col}, Quantity) VALUES ({user.id_user}, {productId}, {want})");

                MessageBox.Show("Товар добавлен в корзину!");
                this.Close();
            }
        }
    }
}