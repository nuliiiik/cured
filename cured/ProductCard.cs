using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    /// <summary>
    /// Интерактивная карточка товара. 
    /// Умеет самостоятельно работать с корзиной и проверять остатки.
    /// </summary>
    public class ProductCard : UserControl
    {
        #region Элементы управления (UI)
        public PictureBox pictureBox;
        public Label labelName, labelPrice, labelDetails, labelArticle;

        private Panel cartPanel;
        private Button btnQuickAdd;
        private Label lblQty;
        private Button btnPlus, btnMinus;
        #endregion

        #region Данные товара
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
            InitializeCartUI();

            // Начальная проверка: есть ли этот товар уже в корзине у пользователя?
            CheckInitialCartStatus();
        }

        #region Настройка внешнего вида

        private void InitializeComponentStyle()
        {
            this.Size = new Size(220, 320);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Margin = new Padding(10);
            this.Cursor = Cursors.Default;
        }

        private void InitializeProductData()
        {
            // Картинка товара
            pictureBox = new PictureBox { Size = new Size(200, 150), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke, Cursor = Cursors.Hand };

            // Название
            labelName = new Label { Location = new Point(10, 165), Size = new Size(200, 35), Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            // Артикул (М - мотоцикл, Д - деталь/запчасть)
            string prefix = (productType == "moto") ? "М" : "Д";
            labelArticle = new Label { Location = new Point(10, 200), Size = new Size(200, 15), Text = $"Артикул: {prefix}{productId}", Font = new Font("Segoe UI", 7), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            // Цена
            labelPrice = new Label { Location = new Point(10, 215), Size = new Size(200, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkRed, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            // Доп. инфо (Бренд, год)
            labelDetails = new Label { Location = new Point(10, 240), Size = new Size(200, 20), Font = new Font("Segoe UI", 7), ForeColor = SystemColors.ButtonShadow, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            // Подписываем элементы на клик для открытия окна деталей в main.cs
            pictureBox.Click += (s, e) => OpenDetailsWindow();
            labelName.Click += (s, e) => OpenDetailsWindow();
            labelPrice.Click += (s, e) => OpenDetailsWindow();
            labelArticle.Click += (s, e) => OpenDetailsWindow();
            labelDetails.Click += (s, e) => OpenDetailsWindow();

            this.Controls.AddRange(new Control[] { pictureBox, labelName, labelArticle, labelPrice, labelDetails });
        }

        #endregion

        #region Логика Корзины

        private void InitializeCartUI()
        {
            cartPanel = new Panel { Location = new Point(10, 265), Size = new Size(200, 45), BackColor = Color.Transparent };

            // Кнопка "Купить"
            btnQuickAdd = new Button { Size = new Size(180, 35), Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnQuickAdd.FlatAppearance.BorderSize = 0;

            if (currentStock > 0)
            {
                btnQuickAdd.Text = "🛒 КУПИТЬ";
                btnQuickAdd.BackColor = Color.DarkRed;
                btnQuickAdd.ForeColor = Color.White;
                btnQuickAdd.Click += BtnQuickAdd_Click;
            }
            else
            {
                btnQuickAdd.Text = "НЕТ В НАЛИЧИИ";
                btnQuickAdd.BackColor = SystemColors.ButtonShadow;
                btnQuickAdd.ForeColor = Color.White;
                btnQuickAdd.Enabled = false;
            }

            // Элементы управления количеством (скрыты по умолчанию)
            btnMinus = new Button { Size = new Size(35, 35), Location = new Point(10, 5), Text = "−", FlatStyle = FlatStyle.Flat, BackColor = SystemColors.ButtonShadow, ForeColor = Color.White, Visible = false, Cursor = Cursors.Hand };
            btnMinus.FlatAppearance.BorderSize = 0;

            lblQty = new Label { Size = new Size(110, 35), Location = new Point(45, 5), Text = "1", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DarkRed, Visible = false };

            btnPlus = new Button { Size = new Size(35, 35), Location = new Point(155, 5), Text = "+", FlatStyle = FlatStyle.Flat, BackColor = SystemColors.ButtonShadow, ForeColor = Color.White, Visible = false, Cursor = Cursors.Hand };
            btnPlus.FlatAppearance.BorderSize = 0;

            btnPlus.Click += (s, e) => UpdateCartQty(1);
            btnMinus.Click += (s, e) => UpdateCartQty(-1);

            cartPanel.Controls.AddRange(new Control[] { btnQuickAdd, btnMinus, lblQty, btnPlus });
            this.Controls.Add(cartPanel);
        }

        private void BtnQuickAdd_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) { MessageBox.Show("Для покупки необходимо войти в аккаунт."); return; }
            UpdateCartQty(1);
            ToggleCartButtons(true);
        }

        private void ToggleCartButtons(bool inCart)
        {
            btnQuickAdd.Visible = !inCart;
            btnMinus.Visible = inCart;
            lblQty.Visible = inCart;
            btnPlus.Visible = inCart;
        }

        /// <summary>
        /// Обновляет количество товара в БД (таблица Cart) и в интерфейсе карточки.
        /// </summary>
        private void UpdateCartQty(int change)
        {
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            int currentInCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;

            int newQty = currentInCart + change;

            // Проверка на наличие на складе
            if (newQty > currentStock)
            {
                MessageBox.Show($"Извините, на складе осталось только {currentStock} шт.", "Ограничение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newQty <= 0)
            {
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
                ToggleCartButtons(false);
            }
            else
            {
                if (currentInCart == 0)
                    db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {col}, Quantity) VALUES ({user.id_user}, {productId}, 1)");
                else
                    db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE UserID = {user.id_user} AND {col} = {productId}");

                lblQty.Text = newQty.ToString();
            }
        }

        private void CheckInitialCartStatus()
        {
            if (!acc_checked.acc_check || currentStock <= 0) return;
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            if (dt.Rows.Count > 0)
            {
                lblQty.Text = dt.Rows[0]["Quantity"].ToString();
                ToggleCartButtons(true);
            }
        }

        #endregion

        private void OpenDetailsWindow() => base.OnClick(EventArgs.Empty);

        public void PerformCardClick() => base.OnClick(EventArgs.Empty);
    }
}