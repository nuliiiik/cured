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
        private int currentStock = 0;
        private DataBase db = new DataBase();

        private NumericUpDown numQty;
        private Label lblStock;

        // Конструктор для открытия из корзины (по ID)
        public ProductDetails(int id, string type)
        {
            this.productId = id;
            this.productType = type;

            InitializeComponent();
            GetStockFromDB();

            string table = (productType == "moto") ? "Motorcycles" : "Parts";
            string idCol = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT * FROM {table} WHERE {idCol} = {productId}");

            if (dt.Rows.Count > 0)
            {
                string name = (productType == "moto") ? dt.Rows[0]["Brand"].ToString() + " " + dt.Rows[0]["Model"].ToString() : dt.Rows[0]["PartName"].ToString();
                string price = $"{Convert.ToDecimal(dt.Rows[0]["Price"]):N0} ₽";
                string desc = dt.Rows[0]["Description"].ToString();

                Image img = null;
                try { img = Image.FromFile(Application.StartupPath + dt.Rows[0]["ImageURL"].ToString()); } catch { }

                SetupWindowProps(name);
                InitializeCustomUI(name, price, desc, img);
            }
        }

        // Оригинальный конструктор для каталога
        public ProductDetails(string art, string name, string price, string info, string desc, Image img, int id, string type)
        {
            this.productId = id;
            this.productType = type;

            SetupWindowProps(name);
            InitializeComponent();
            GetStockFromDB();
            InitializeCustomUI(name, price, desc, img);
        }

        private void SetupWindowProps(string title)
        {
            this.Text = title;
            this.BackColor = Color.White;
            this.Size = new Size(500, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void GetStockFromDB()
        {
            string table = (productType == "moto") ? "Motorcycles" : "Parts";
            string idCol = (productType == "moto") ? "MotorcycleID" : "PartID";
            try
            {
                DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM {table} WHERE {idCol} = {productId}");
                if (dt.Rows.Count > 0)
                    currentStock = Convert.ToInt32(dt.Rows[0]["Quantity"]);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка синхронизации: " + ex.Message); }
        }

        private void InitializeCustomUI(string name, string price, string desc, Image img)
        {
            int marginLeft = 25;
            int contentWidth = this.ClientSize.Width - (marginLeft * 2);

            PictureBox pb = new PictureBox { Location = new Point(marginLeft, 15), Size = new Size(contentWidth, 250), SizeMode = PictureBoxSizeMode.Zoom, Image = img };
            Label lblName = new Label { Location = new Point(marginLeft, 280), Size = new Size(contentWidth, 50), Text = name, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
            Label lblPriceText = new Label { Location = new Point(marginLeft, 335), Size = new Size(contentWidth, 40), Text = price, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = (currentStock > 0) ? Color.DarkRed : Color.Gray };
            Label lblDesc = new Label { Location = new Point(marginLeft, 385), Size = new Size(contentWidth, 110), Text = string.IsNullOrEmpty(desc) ? "Описание отсутствует." : desc, Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray };

            lblStock = new Label { Location = new Point(marginLeft, 510), Text = (currentStock > 0) ? $"На складе: {currentStock} шт." : "Нет в наличии", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = (currentStock > 0) ? Color.ForestGreen : Color.DarkRed };
            Label lblQtyHint = new Label { Location = new Point(marginLeft, 542), Text = "Количество:", AutoSize = true, Font = new Font("Segoe UI", 10) };

            numQty = new NumericUpDown { Location = new Point(marginLeft + 100, 540), Size = new Size(80, 29), Font = new Font("Segoe UI", 11), Minimum = 1, Maximum = (currentStock > 0) ? currentStock : 1, Enabled = (currentStock > 0) };

            Button btnAdd = new Button { Location = new Point(marginLeft, 600), Size = new Size(contentWidth, 55), Text = (currentStock > 0) ? "ДОБАВИТЬ В КОРЗИНУ" : "НЕТ В НАЛИЧИИ", BackColor = (currentStock > 0) ? Color.DarkRed : Color.Silver, Enabled = (currentStock > 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { pb, lblName, lblPriceText, lblDesc, lblStock, lblQtyHint, numQty, btnAdd });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) { MessageBox.Show("Нужна авторизация!"); return; }
            int wantToAdd = (int)numQty.Value;
            string colName = (productType == "moto") ? "MotorcycleID" : "PartID";

            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {colName} = {productId}");
            int alreadyInCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;

            if (alreadyInCart + wantToAdd > currentStock) { MessageBox.Show("Превышен лимит склада!"); }
            else
            {
                if (alreadyInCart > 0) db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity + {wantToAdd} WHERE UserID = {user.id_user} AND {colName} = {productId}");
                else db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {colName}, Quantity) VALUES ({user.id_user}, {productId}, {wantToAdd})");
                MessageBox.Show("Добавлено!");
                this.Close();
            }
        }
    }
}