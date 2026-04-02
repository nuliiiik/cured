using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    /// <summary>
    /// ПОЛЬЗОВАТЕЛЬСКИЙ КОНТРОЛ: КАРТОЧКА ТОВАРА
    /// Отвечает за отображение краткой информации о товаре (фото, цена, название)
    /// и управление быстрыми действиями с корзиной.
    /// </summary>
    public class ProductCard : UserControl
    {
        #region 1. ЭЛЕМЕНТЫ ИНТЕРФЕЙСА (UI)

        public PictureBox pictureBox;
        public Label labelName, labelPrice, labelDetails, labelArticle;

        private Panel cartPanel;
        private Button btnQuickAdd;
        private Label lblQty;
        private Button btnPlus, btnMinus;

        #endregion

        #region 2. ПОЛЯ И ДАННЫЕ ОБЪЕКТА

        private int productId;       // ID товара из базы
        private string productType;  // Тип: "moto" (мотоцикл) или "part" (запчасть)
        private int currentStock;    // Текущий остаток на складе
        private DataBase db = new DataBase();

        #endregion

        #region 3. КОНСТРУКТОР

        /// <summary>
        /// Инициализация карточки с привязкой к конкретному товару.
        /// </summary>
        public ProductCard(int id, string type, int stock)
        {
            this.productId = id;
            this.productType = type;
            this.currentStock = stock;

            InitializeComponentStyle(); // Настройка внешнего вида контейнера
            InitializeProductData();    // Создание текстовых полей и фото
            LoadInfoFromDB();           // Загрузка текстовой информации из SQL
            InitializeCartUI();         // Настройка кнопок покупки
            CheckInitialCartStatus();   // Проверка, есть ли этот товар уже в корзине
        }

        #endregion

        #region 4. РАБОТА С БАЗОЙ ДАННЫХ (ЗАГРУЗКА)

        /// <summary>
        /// Загружает подробные данные о товаре из БД для заполнения текстовых меток.
        /// </summary>
        private void LoadInfoFromDB()
        {
            // Выбираем таблицу и колонку ID в зависимости от типа товара
            string table = (productType == "moto") ? "Motorcycles" : "Parts";
            string idCol = (productType == "moto") ? "MotorcycleID" : "PartID";

            DataTable dt = db.ExecuteQuery($"SELECT * FROM {table} WHERE {idCol} = {productId}");

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                // Извлекаем чистые данные из колонок
                string brandFromDB = row["Brand"]?.ToString().Trim() ?? "";
                string partNameFromDB = (productType == "moto")
                    ? row["Model"]?.ToString().Trim()
                    : row["PartName"]?.ToString().Trim();
                string forModels = (productType == "moto")
                    ? row["Year"]?.ToString()
                    : row["ForModels"]?.ToString().Trim();

                // ФОРМИРУЕМ НАЗВАНИЕ: "Бренд Модель" или просто "Название"
                if (!string.IsNullOrEmpty(brandFromDB))
                    labelName.Text = $"{brandFromDB} {partNameFromDB}";
                else
                    labelName.Text = partNameFromDB;

                // ФОРМИРУЕМ ДЕТАЛИ (строка под ценой)
                if (productType == "moto")
                    labelDetails.Text = $"Производитель: {brandFromDB} | {forModels} г.";
                else
                    labelDetails.Text = $"Бренд: {brandFromDB} ({forModels})";

                // Цена с разделением тысяч
                labelPrice.Text = $"{Convert.ToDecimal(row["Price"]):N0} ₽";

                // Попытка загрузки изображения
                try
                {
                    string path = row["ImageURL"]?.ToString();
                    if (!string.IsNullOrEmpty(path))
                        pictureBox.Image = Image.FromFile(Application.StartupPath + path);
                }
                catch { /* Игнорируем отсутствие фото, останется стандартная заглушка */ }
            }
        }

        #endregion

        #region 5. ИНИЦИАЛИЗАЦИЯ И ДИЗАЙН

        private void InitializeComponentStyle()
        {
            this.Size = new Size(220, 320);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Margin = new Padding(10);
        }

        private void InitializeProductData()
        {
            // Фото товара
            pictureBox = new PictureBox { Size = new Size(200, 140), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke, Cursor = Cursors.Hand };

            // Название (увеличенная высота для длинных имен)
            labelName = new Label { Location = new Point(10, 155), Size = new Size(200, 40), Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            // Артикул
            string prefix = (productType == "moto") ? "М" : "Д";
            labelArticle = new Label { Location = new Point(10, 195), Size = new Size(200, 15), Text = $"Артикул: {prefix}{productId}", Font = new Font("Segoe UI", 7), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };

            // Цена
            labelPrice = new Label { Location = new Point(10, 210), Size = new Size(200, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkRed, TextAlign = ContentAlignment.MiddleCenter };

            // Доп. информация (бренд/год)
            labelDetails = new Label { Location = new Point(10, 235), Size = new Size(200, 30), Font = new Font("Segoe UI", 8), ForeColor = Color.DimGray, TextAlign = ContentAlignment.TopCenter };

            // Привязка кликов для открытия окна деталей
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
                btnQuickAdd.Click += (s, e) =>
                {
                    if (acc_checked.acc_check) { UpdateCartQty(1); ToggleCartButtons(true); }
                    else MessageBox.Show("Войдите в аккаунт для покупок!", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };
            }
            else
            {
                btnQuickAdd.Text = "НЕТ В НАЛИЧИИ";
                btnQuickAdd.BackColor = Color.Silver;
                btnQuickAdd.Enabled = false;
            }

            // Элементы управления количеством (скрыты по умолчанию)
            btnMinus = new Button { Size = new Size(35, 35), Location = new Point(10, 5), Text = "−", Visible = false, FlatStyle = FlatStyle.Flat };
            lblQty = new Label { Size = new Size(110, 35), Location = new Point(45, 5), TextAlign = ContentAlignment.MiddleCenter, Visible = false, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnPlus = new Button { Size = new Size(35, 35), Location = new Point(155, 5), Text = "+", Visible = false, FlatStyle = FlatStyle.Flat };

            btnPlus.Click += (s, e) => UpdateCartQty(1);
            btnMinus.Click += (s, e) => UpdateCartQty(-1);

            cartPanel.Controls.AddRange(new Control[] { btnQuickAdd, btnMinus, lblQty, btnPlus });
            this.Controls.Add(cartPanel);
        }

        #endregion

        #region 6. ЛОГИКА КОРЗИНЫ (ИНТЕГРАЦИЯ С БД)

        /// <summary>
        /// Изменяет количество товара в корзине пользователя.
        /// </summary>
        private void UpdateCartQty(int change)
        {
            if (!acc_checked.acc_check) return;

            string col = (productType == "moto") ? "MotorcycleID" : "PartID";

            // Проверяем текущее наличие в корзине
            DataTable dt = db.ExecuteQuery($"SELECT Quantity FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
            int inCart = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["Quantity"]) : 0;
            int newQty = inCart + change;

            // Валидация: нельзя купить больше, чем есть на складе
            if (newQty > currentStock)
            {
                MessageBox.Show($"Извините, на складе всего {currentStock} шт.", "Ограничение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (newQty <= 0)
            {
                // Если количество 0 — удаляем из корзины
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user} AND {col} = {productId}");
                ToggleCartButtons(false);
            }
            else
            {
                // Добавляем или обновляем запись
                if (inCart == 0)
                    db.ExecuteNonQuery($"INSERT INTO Cart (UserID, {col}, Quantity) VALUES ({user.id_user}, {productId}, 1)");
                else
                    db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE UserID = {user.id_user} AND {col} = {productId}");

                lblQty.Text = newQty.ToString();
            }
        }

        /// <summary>
        /// Переключает видимость кнопок: "Купить" или "+ / - количество".
        /// </summary>
        private void ToggleCartButtons(bool inCart)
        {
            btnQuickAdd.Visible = !inCart;
            btnMinus.Visible = inCart;
            lblQty.Visible = inCart;
            btnPlus.Visible = inCart;
        }

        /// <summary>
        /// Проверяет при создании карточки, добавлен ли этот товар уже в корзину пользователя.
        /// </summary>
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

        #region 7. НАВИГАЦИЯ

        /// <summary>
        /// Вызывает стандартное событие клика родителя для открытия формы деталей.
        /// </summary>
        private void OpenDetailsWindow() => base.OnClick(EventArgs.Empty);

        #endregion
    }
}