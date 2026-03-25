using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    /// <summary>
    /// Окно детальной информации о товаре.
    /// Позволяет выбрать количество и добавить товар в корзину с проверкой остатков.
    /// </summary>
    public partial class ProductDetails : Form
    {
        #region Поля данных
        private int productId;
        private string productType;
        private int currentStock = 0;
        private DataBase db = new DataBase();

        // Элементы управления, к которым нужен доступ из методов
        private NumericUpDown numQty;
        private Label lblStock;
        #endregion

        public ProductDetails(string art, string name, string price, string info, string desc, Image img, int id, string type)
        {
            this.productId = id;
            this.productType = type;

            // Базовая настройка окна
            this.Text = name;
            this.BackColor = Color.White;
            this.Size = new Size(500, 720); // Фиксированный размер для предсказуемого UI
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeComponent();

            // 1. Сначала узнаем реальный остаток из БД
            GetStockFromDB();

            // 2. Затем строим интерфейс на основе этих данных
            InitializeCustomUI(name, price, desc, img);
        }

        /// <summary>
        /// Запрашивает актуальное количество товара на складе.
        /// </summary>
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
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка синхронизации остатков: " + ex.Message);
            }
        }

        #region Инициализация Интерфейса

        private void InitializeCustomUI(string name, string price, string desc, Image img)
        {
            int marginLeft = 25;
            int contentWidth = this.ClientSize.Width - (marginLeft * 2);

            // Изображение
            PictureBox pb = new PictureBox { Location = new Point(marginLeft, 15), Size = new Size(contentWidth, 250), SizeMode = PictureBoxSizeMode.Zoom, Image = img };

            // Название
            Label lblName = new Label { Location = new Point(marginLeft, 280), Size = new Size(contentWidth, 50), Text = name, Font = new Font("Segoe UI", 16, FontStyle.Bold) };

            // Цена
            Label lblPrice = new Label
            {
                Location = new Point(marginLeft, 335),
                Size = new Size(contentWidth, 40),
                Text = price,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = (currentStock > 0) ? Color.DarkRed : Color.Gray
            };

            // Описание
            Label lblDesc = new Label
            {
                Location = new Point(marginLeft, 385),
                Size = new Size(contentWidth, 110),
                Text = string.IsNullOrEmpty(desc) ? "Описание товара временно отсутствует." : desc,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            };

            // Информация о наличии
            lblStock = new Label
            {
                Location = new Point(marginLeft, 510),
                Text = (currentStock > 0) ? $"На складе: {currentStock} шт." : "Товара нет в наличии",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = (currentStock > 0) ? Color.ForestGreen : Color.DarkRed
            };

            // Выбор количества
            Label lblQtyHint = new Label { Location = new Point(marginLeft, 542), Text = "Количество:", AutoSize = true, Font = new Font("Segoe UI", 10) };

            numQty = new NumericUpDown
            {
                Location = new Point(marginLeft + 100, 540),
                Size = new Size(80, 29),
                Font = new Font("Segoe UI", 11),
                Minimum = 1,
                Maximum = (currentStock > 0) ? currentStock : 1,
                Enabled = (currentStock > 0)
            };

            // Кнопка действия
            Button btnAdd = new Button
            {
                Location = new Point(marginLeft, 600),
                Size = new Size(contentWidth, 55),
                Text = (currentStock > 0) ? "ДОБАВИТЬ В КОРЗИНУ" : "НЕТ В НАЛИЧИИ",
                BackColor = (currentStock > 0) ? Color.DarkRed : Color.Silver,
                Enabled = (currentStock > 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = (currentStock > 0) ? Cursors.Hand : Cursors.Default
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { pb, lblName, lblPrice, lblDesc, lblStock, lblQtyHint, numQty, btnAdd });
        }

        #endregion

        #region Логика добавления

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // 1. Проверка авторизации
            if (!acc_checked.acc_check)
            {
                MessageBox.Show("Для добавления товаров в корзину необходимо авторизоваться.", "Вход не выполнен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int wantToAdd = (int)numQty.Value;
            string colName = (productType == "moto") ? "MotorcycleID" : "PartID";

            try
            {
                // 2. Проверяем, сколько уже лежит в корзине
                DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {colName} = {productId}");
                int alreadyInCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;

                // 3. Финальная проверка на превышение лимита склада
                if (alreadyInCart + wantToAdd > currentStock)
                {
                    MessageBox.Show($"Вы не можете добавить {wantToAdd} шт., так как в корзине уже {alreadyInCart} шт., а на складе всего {currentStock} шт.",
                        "Превышение лимита", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (alreadyInCart > 0)
                        db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity + {wantToAdd} WHERE UserID = {user.id_user} AND {colName} = {productId}");
                    else
                        db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {colName}, Quantity) VALUES ({user.id_user}, {productId}, {wantToAdd})");

                    MessageBox.Show("Товар успешно добавлен в корзину!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Закрываем окно после успешного действия
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении корзины: " + ex.Message);
            }
        }

        #endregion
    }
}