using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public class ProductCard : UserControl
    {
        #region Элементы UI
        public PictureBox pictureBox;
        public Label labelName, labelPrice, labelDetails, labelArticle;
        private Panel cartPanel;
        private Button btnQuickAdd;
        private Label lblQty;
        private Button btnPlus, btnMinus;
        #endregion

        #region Данные
        private int productId;
        private string productType;
        private int currentStock;
        private DataBase db = new DataBase();
        #endregion

        public ProductCard(int id, string type, int stock)
        {
            this.productId = id;
            this.productType = type;
            this.currentStock = stock;

            InitializeComponentStyle();
            InitializeProductData();
            LoadInfoFromDB(); // ЗАГРУЗКА ДАННЫХ
            InitializeCartUI();
            CheckInitialCartStatus();
        }
        private void LoadInfoFromDB()
        {
            string table = (productType == "moto") ? "Motorcycles" : "Parts";
            string idCol = (productType == "moto") ? "MotorcycleID" : "PartID";

            DataTable dt = db.ExecuteQuery($"SELECT * FROM {table} WHERE {idCol} = {productId}");

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                // 1. Достаем чистые данные из колонок (как в вашей БД на скрине)
                string brandFromDB = row["Brand"]?.ToString().Trim() ?? "";
                string partNameFromDB = (productType == "moto") ? row["Model"]?.ToString().Trim() : row["PartName"]?.ToString().Trim();
                string forModels = (productType == "moto") ? row["Year"]?.ToString() : row["ForModels"]?.ToString().Trim();

                // 2. ФОРМИРУЕМ НАЗВАНИЕ (Здесь исправлена ошибка "Запчасть")
                // Если бренд есть, пишем "Бренд Название", если нет - только "Название"
                if (!string.IsNullOrEmpty(brandFromDB))
                {
                    labelName.Text = $"{brandFromDB} {partNameFromDB}";
                }
                else
                {
                    labelName.Text = partNameFromDB;
                }

                // 3. ФОРМИРУЕМ ДЕТАЛИ (Нижняя строчка)
                if (productType == "moto")
                {
                    labelDetails.Text = $"Производитель: {brandFromDB} | {forModels} г.";
                }
                else
                {
                    // Для запчастей пишем Бренд и для каких моделей
                    labelDetails.Text = $"Бренд: {brandFromDB} ({forModels})";
                }

                // 4. Цена и картинка
                labelPrice.Text = $"{Convert.ToDecimal(row["Price"]):N0} ₽";

                try
                {
                    string path = row["ImageURL"]?.ToString();
                    if (!string.IsNullOrEmpty(path))
                        pictureBox.Image = Image.FromFile(Application.StartupPath + path);
                }
                catch { /* игнорируем ошибку фото */ }
            }
        }

        private void InitializeComponentStyle()
        {
            this.Size = new Size(220, 320);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Margin = new Padding(10);
        }

        private void InitializeProductData()
        {
            pictureBox = new PictureBox { Size = new Size(200, 140), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke, Cursor = Cursors.Hand };

            // Увеличили высоту labelName, чтобы влезло и название, и бренд
            labelName = new Label { Location = new Point(10, 155), Size = new Size(200, 40), Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            string prefix = (productType == "moto") ? "М" : "Д";
            labelArticle = new Label { Location = new Point(10, 195), Size = new Size(200, 15), Text = $"Артикул: {prefix}{productId}", Font = new Font("Segoe UI", 7), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };

            labelPrice = new Label { Location = new Point(10, 210), Size = new Size(200, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkRed, TextAlign = ContentAlignment.MiddleCenter };

            labelDetails = new Label
            {
                Location = new Point(10, 235),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.TopCenter
            };

            pictureBox.Click += (s, e) => OpenDetailsWindow();
            labelName.Click += (s, e) => OpenDetailsWindow();
            labelDetails.Click += (s, e) => OpenDetailsWindow();

            this.Controls.AddRange(new Control[] { pictureBox, labelName, labelArticle, labelPrice, labelDetails });
        }

        private void InitializeCartUI()
        {
            cartPanel = new Panel { Location = new Point(10, 265), Size = new Size(200, 45) };
            btnQuickAdd = new Button { Size = new Size(180, 35), Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };

            if (currentStock > 0)
            {
                btnQuickAdd.Text = "🛒 КУПИТЬ";
                btnQuickAdd.BackColor = Color.DarkRed;
                btnQuickAdd.ForeColor = Color.White;
                btnQuickAdd.Click += (s, e) => { if (acc_checked.acc_check) { UpdateCartQty(1); ToggleCartButtons(true); } else MessageBox.Show("Войдите!"); };
            }
            else
            {
                btnQuickAdd.Text = "НЕТ В НАЛИЧИИ";
                btnQuickAdd.BackColor = Color.Silver;
                btnQuickAdd.Enabled = false;
            }

            btnMinus = new Button { Size = new Size(35, 35), Location = new Point(10, 5), Text = "−", Visible = false, FlatStyle = FlatStyle.Flat };
            lblQty = new Label { Size = new Size(110, 35), Location = new Point(45, 5), TextAlign = ContentAlignment.MiddleCenter, Visible = false, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnPlus = new Button { Size = new Size(35, 35), Location = new Point(155, 5), Text = "+", Visible = false, FlatStyle = FlatStyle.Flat };

            btnPlus.Click += (s, e) => UpdateCartQty(1);
            btnMinus.Click += (s, e) => UpdateCartQty(-1);

            cartPanel.Controls.AddRange(new Control[] { btnQuickAdd, btnMinus, lblQty, btnPlus });
            this.Controls.Add(cartPanel);
        }

        private void UpdateCartQty(int change)
        {
            if (!acc_checked.acc_check) return;
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            int inCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;
            int newQty = inCart + change;

            if (newQty > currentStock) { MessageBox.Show("Превышен остаток!"); return; }

            if (newQty <= 0)
            {
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
                ToggleCartButtons(false);
            }
            else
            {
                if (inCart == 0) db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {col}, Quantity) VALUES ({user.id_user}, {productId}, 1)");
                else db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE UserID = {user.id_user} AND {col} = {productId}");
                lblQty.Text = newQty.ToString();
            }
        }

        private void ToggleCartButtons(bool inCart)
        {
            btnQuickAdd.Visible = !inCart;
            btnMinus.Visible = inCart;
            lblQty.Visible = inCart;
            btnPlus.Visible = inCart;
        }

        private void CheckInitialCartStatus()
        {
            if (!acc_checked.acc_check || currentStock <= 0) return;
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            if (dt.Rows.Count > 0) { lblQty.Text = dt.Rows[0]["Quantity"].ToString(); ToggleCartButtons(true); }
        }

        private void OpenDetailsWindow() => base.OnClick(EventArgs.Empty);
    }
}