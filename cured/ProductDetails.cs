using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public partial class ProductDetails : Form
    {
        private int productId;
        private string productType; // "moto" или "part"
        private int currentStock = 0;
        private DataBase db = new DataBase();

        private NumericUpDown numQty;
        private Label lblStock;

        // КОНСТРУКТОР 1: Вызывается из корзины или по ID (всего 2 аргумента)
        public ProductDetails(int id, string type)
        {
            this.productId = id;
            this.productType = type;

            InitializeComponent();
            LoadProductAndUI();
        }

        // КОНСТРУКТОР 2: Вызывается из каталога (8 аргументов)
        // Если типы данных в вызове не совпадают (например, цена — это decimal), 
        // убедитесь, что при вызове стоит .ToString()
        public ProductDetails(string art, string name, string price, string info, string desc, Image img, int id, string type)
        {
            this.productId = id;
            this.productType = type;

            InitializeComponent();

            // Получаем актуальный остаток из БД
            GetStockFromDB();

            // Формируем красивое описание из того, что пришло из каталога
            string fullInfo = $"{info}\n\nОписание:\n{desc}";

            SetupWindowProps("Детали товара");
            InitializeCustomUI(name, price, fullInfo, img);
        }

        private void LoadProductAndUI()
        {
            GetStockFromDB();

            string table = (productType == "moto") ? "Motorcycles" : "Parts";
            string idCol = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT * FROM {table} WHERE {idCol} = {productId}");

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string name, fullInfo;

                if (productType == "moto")
                {
                    name = $"{row["Brand"]} {row["Model"]}"; // Марка + Модель
                    fullInfo = $"🏍️ Категория: Мотоцикл\n" +
                               $"Производитель (Бренд): {row["Brand"]}\n" + // <-- ВОТ ЗДЕСЬ
                               $"Модель: {row["Model"]}\n" +
                               $"Год выпуска: {row["Year"]} г.\n\n" +
                               $"Описание: {row["Description"]}";
                }
                else
                {
                    name = row["PartName"].ToString();
                    fullInfo = $"🔧 Категория: Запчасти\n" +
                               $"Наименование: {row["PartName"]}\n" +
                               $"Бренд: {row["Brand"]}\n" + // <-- И ВОТ ЗДЕСЬ
                               $"Для моделей: {row["ForModels"]}\n\n" +
                               $"Описание: {row["Description"]}";
                }

                string price = $"{Convert.ToDecimal(row["Price"]):N0} ₽";

                Image img = null;
                try
                {
                    string path = Application.StartupPath + row["ImageURL"].ToString();
                    img = Image.FromFile(path);
                }
                catch { /* Если фото нет, pb будет пустым */ }

                SetupWindowProps("Детали товара");
                InitializeCustomUI(name, price, fullInfo, img);
            }
        }

        private void SetupWindowProps(string title)
        {
            this.Text = title;
            this.BackColor = Color.White;
            this.Size = new Size(500, 750);
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
            catch { currentStock = 0; }
        }

        private void InitializeCustomUI(string name, string price, string fullInfo, Image img)
        {
            this.Controls.Clear(); // Очистка на случай повторного вызова
            int marginLeft = 25;
            int contentWidth = this.ClientSize.Width - (marginLeft * 2);

            PictureBox pb = new PictureBox
            {
                Location = new Point(marginLeft, 15),
                Size = new Size(contentWidth, 250),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = img,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblName = new Label
            {
                Location = new Point(marginLeft, 280),
                Size = new Size(contentWidth, 60),
                Text = name,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            Label lblPriceText = new Label
            {
                Location = new Point(marginLeft, 345),
                Size = new Size(contentWidth, 40),
                Text = price,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = (currentStock > 0) ? Color.DarkRed : Color.Gray
            };

            Label lblInfo = new Label
            {
                Location = new Point(marginLeft, 400),
                Size = new Size(contentWidth, 150),
                Text = fullInfo,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Black
            };

            lblStock = new Label
            {
                Location = new Point(marginLeft, 560),
                Text = (currentStock > 0) ? $"✅ Доступно: {currentStock} шт." : "❌ Нет в наличии",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = (currentStock > 0) ? Color.ForestGreen : Color.DarkRed
            };

            Label lblQtyHint = new Label { Location = new Point(marginLeft, 592), Text = "Количество:", AutoSize = true };

            numQty = new NumericUpDown
            {
                Location = new Point(marginLeft + 100, 590),
                Size = new Size(70, 25),
                Minimum = 1,
                Maximum = (currentStock > 0) ? currentStock : 1,
                Enabled = (currentStock > 0)
            };

            Button btnAdd = new Button
            {
                Location = new Point(marginLeft, 640),
                Size = new Size(contentWidth, 55),
                Text = (currentStock > 0) ? "ДОБАВИТЬ В КОРЗИНУ" : "НЕТ В НАЛИЧИИ",
                BackColor = (currentStock > 0) ? Color.DarkRed : Color.Silver,
                Enabled = (currentStock > 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { pb, lblName, lblPriceText, lblInfo, lblStock, lblQtyHint, numQty, btnAdd });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) { MessageBox.Show("Войдите в систему!"); return; }

            int want = (int)numQty.Value;
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";

            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            int inCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;

            if (inCart + want > currentStock)
            {
                MessageBox.Show($"Недостаточно на складе!");
            }
            else
            {
                if (inCart > 0)
                    db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity + {want} WHERE UserID = {user.id_user} AND {col} = {productId}");
                else
                    db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {col}, Quantity) VALUES ({user.id_user}, {productId}, {want})");

                MessageBox.Show("Товар в корзине!");
                this.Close();
            }
        }
    }
}