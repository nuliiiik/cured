using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public class ProductCard : UserControl
    {
        public PictureBox pictureBox;
        public Label labelName, labelPrice, labelDetails, labelArticle;

        private Panel cartPanel;
        private Button btnQuickAdd;
        private Label lblQty;
        private Button btnPlus, btnMinus;

        private int productId;
        private string productType;
        private int currentStock;
        private DataBase db = new DataBase();

        public ProductCard(int id, string type, int stock)
        {
            this.productId = id;
            this.productType = type;
            this.currentStock = stock;

            this.Size = new Size(220, 320);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Margin = new Padding(10);

            // Инициализация элементов
            pictureBox = new PictureBox { Size = new Size(200, 150), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke, Cursor = Cursors.Hand };
            labelName = new Label { Location = new Point(10, 165), Size = new Size(200, 35), Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            string prefix = (productType == "moto") ? "М" : "Д";
            labelArticle = new Label { Location = new Point(10, 200), Size = new Size(200, 15), Text = $"Артикул: {prefix}{productId}", Font = new Font("Segoe UI", 7), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            labelPrice = new Label
            {
                Location = new Point(10, 215),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            labelDetails = new Label { Location = new Point(10, 240), Size = new Size(200, 20), Font = new Font("Segoe UI", 7), ForeColor = SystemColors.ButtonShadow, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            // ПОДПИСЫВАЕМ ВСЕ ЭЛЕМЕНТЫ НА ОТКРЫТИЕ (БЕЗ RECURSION)
            pictureBox.Click += (s, e) => OpenDetailsWindow();
            labelName.Click += (s, e) => OpenDetailsWindow();
            labelPrice.Click += (s, e) => OpenDetailsWindow();
            labelArticle.Click += (s, e) => OpenDetailsWindow();
            labelDetails.Click += (s, e) => OpenDetailsWindow();

            InitializeCartUI();

            this.Controls.AddRange(new Control[] { pictureBox, labelName, labelArticle, labelPrice, labelDetails });

            CheckInitialCartStatus();
        }

        private void OpenDetailsWindow()
        {
            // Вместо OnClick вызываем базовый метод события, чтобы main.cs его поймал
            // Это НЕ вызовет зацикливания
            base.OnClick(EventArgs.Empty);
        }

        private void InitializeCartUI()
        {
            cartPanel = new Panel { Location = new Point(10, 265), Size = new Size(200, 45), BackColor = Color.Transparent };

            btnQuickAdd = new Button
            {
                Size = new Size(180, 35),
                Location = new Point(10, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
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
            if (!acc_checked.acc_check) { MessageBox.Show("Войдите в аккаунт!"); return; }
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

        private void UpdateCartQty(int change)
        {
            string col = (productType == "moto") ? "MotorcycleID" : "PartID";
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            int currentInCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;

            int newQty = currentInCart + change;
            if (newQty > currentStock) { MessageBox.Show($"Доступно только {currentStock} шт."); return; }

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

        // Этот метод больше не вызывает проблем, так как он не связан напрямую с событиями кнопок
        public void PerformCardClick() { base.OnClick(EventArgs.Empty); }
    }
}