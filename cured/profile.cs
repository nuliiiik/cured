using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cured
{
    public partial class profile : Form
    {
        // Объект для работы с базой данных
        private DataBase db = new DataBase();

        public profile()
        {
            InitializeComponent();

            // Назначаем обработчик клавиши Enter для текстовых полей профиля,
            // чтобы данные сохранялись при нажатии Enter, а не только по кнопке.
            textBox1.KeyDown += CheckEnter;
            textBox2.KeyDown += CheckEnter;
            textBox3.KeyDown += CheckEnter;
        }

        private void profile_Load(object sender, EventArgs e)
        {
            // Если ID пользователя не валиден (не залогинен), закрываем форму
            if (user.id_user <= 0) { this.Close(); return; }

            // Визуальная настройка вкладок (TabControl): делаем их плоскими и скрываем заголовки
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            // Применяем кастомные стили оформления к таблицам корзины и заказов
            ApplyGridStyles(dgvCart);
            ApplyGridStyles(dgvOrders);

            // Подписываемся на события таблиц для интерактивности
            dgvCart.CellDoubleClick += dgvCart_CellDoubleClick; // Просмотр товара при двойном клике
            dgvCart.CellValueChanged += dgvCart_CellValueChanged; // Изменение кол-ва товара

            // Первичная загрузка всех данных из БД
            LoadUserData();    // Данные профиля
            LoadCartData();    // Содержимое корзины
            LoadOrdersData();  // История заказов
        }

        /// <summary>
        /// Метод для настройки внешнего вида таблиц (цвета, шрифты, выделение)
        /// </summary>
        private void ApplyGridStyles(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed; // Темно-красная шапка
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false; // Скрываем пустой столбец слева
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dgv.AllowUserToAddRows = false; // Запрет на добавление пустых строк пользователем
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; // Автоподбор ширины
        }

        #region БЛОК: УПРАВЛЕНИЕ АККАУНТОМ

        // Загрузка данных текущего пользователя в текстовые поля
        public void LoadUserData()
        {
            DataTable dt = db.ExecuteQuery($"SELECT FullName, Login, Password, Phone FROM Users WHERE UserID = {user.id_user}");
            if (dt.Rows.Count > 0)
            {
                textBox1.Text = dt.Rows[0]["FullName"].ToString();
                textBox2.Text = dt.Rows[0]["Login"].ToString();
                textBox3.Text = dt.Rows[0]["Password"].ToString();
                maskedTextBox1.Text = dt.Rows[0]["Phone"].ToString();
            }
        }

        // КНОПКА: Сохранение изменений профиля
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length < 4 || textBox3.Text.Length < 4)
            {
                MessageBox.Show("Логин и пароль должны быть не короче 4 символов!");
                return;
            }

            // Обновляем данные в таблице Users
            db.ExecuteNonQuery($@"UPDATE Users SET FullName = N'{textBox1.Text}', Login = '{textBox2.Text}', 
                                Password = '{textBox3.Text}', Phone = '{maskedTextBox1.Text}' WHERE UserID = {user.id_user}");
            MessageBox.Show("Данные профиля обновлены!");
        }

        // Вспомогательный метод для обработки нажатия Enter в полях ввода
        private void CheckEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick(); // Имитируем нажатие кнопки сохранения
                e.SuppressKeyPress = true; // Убираем звук системного сигнала
            }
        }
        #endregion

        #region БЛОК: КОРЗИНА

        // Метод загрузки товаров, добавленных в корзину
        public void LoadCartData()
        {
            // Отключаем событие изменения, чтобы не вызвать бесконечный цикл при обновлении данных
            dgvCart.CellValueChanged -= dgvCart_CellValueChanged;

            // Сложный запрос с JOIN для получения названий мотоциклов ИЛИ запчастей
            string query = $@"SELECT c.CartID, c.MotorcycleID, c.PartID,
                            ISNULL(m.Brand + ' ' + m.Model, p.PartName) AS [Товар],
                            c.Quantity AS [Кол-во], 
                            ISNULL(m.Quantity, p.Quantity) AS [Stock],
                            ISNULL(m.Price, p.Price) AS [Цена],
                            (c.Quantity * ISNULL(m.Price, p.Price)) AS [Сумма]
                            FROM Cart c 
                            LEFT JOIN Motorcycles m ON c.MotorcycleID = m.MotorcycleID
                            LEFT JOIN Parts p ON c.PartID = p.PartID 
                            WHERE c.UserID = {user.id_user}";

            dgvCart.DataSource = db.ExecuteQuery(query);

            if (dgvCart.Columns.Count > 0)
            {
                // Скрываем служебные колонки с ID и остатком на складе
                dgvCart.Columns["CartID"].Visible = false;
                dgvCart.Columns["MotorcycleID"].Visible = false;
                dgvCart.Columns["PartID"].Visible = false;
                dgvCart.Columns["Stock"].Visible = false;

                // Делаем все поля только для чтения, кроме колонки "Кол-во"
                foreach (DataGridViewColumn col in dgvCart.Columns) col.ReadOnly = true;
                dgvCart.Columns["Кол-во"].ReadOnly = false;

                // Растягиваем название товара на всё свободное место
                dgvCart.Columns["Товар"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            UpdateTotalSum(); // Пересчитываем общую сумму корзины
            dgvCart.CellValueChanged += dgvCart_CellValueChanged; // Возвращаем событие на место
        }

        // Обработка ручного изменения количества товара в сетке
        private void dgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCart.Columns[e.ColumnIndex].Name == "Кол-во")
            {
                int cartId = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["CartID"].Value);
                int stock = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["Stock"].Value);

                if (int.TryParse(dgvCart.Rows[e.RowIndex].Cells["Кол-во"].Value.ToString(), out int newQty))
                {
                    // Проверка на наличие товара на складе
                    if (newQty > stock) { MessageBox.Show($"Доступно только {stock} шт."); newQty = stock; }

                    // Если кол-во 0 или меньше — удаляем из корзины, иначе обновляем
                    if (newQty <= 0) db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
                    else db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE CartID = {cartId}");
                }
                LoadCartData(); // Перегружаем таблицу для актуализации сумм
            }
        }

        // Двойной клик по строке открывает карточку товара
        private void dgvCart_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCart.Columns[e.ColumnIndex].Name == "Кол-во") return;

            int mId = 0, pId = 0;
            int.TryParse(dgvCart.Rows[e.RowIndex].Cells["MotorcycleID"].Value?.ToString(), out mId);
            int.TryParse(dgvCart.Rows[e.RowIndex].Cells["PartID"].Value?.ToString(), out pId);

            int id = (mId > 0) ? mId : pId;
            string type = (mId > 0) ? "moto" : "part";

            // Выбираем данные о товаре для показа в форме деталей
            string sql = (type == "moto")
                ? $"SELECT Brand, Model, Price, Description, ImageURL FROM Motorcycles WHERE MotorcycleID = {id}"
                : $"SELECT Brand, PartName as Model, Price, Description, ImageURL FROM Parts WHERE PartID = {id}";

            DataTable dt = db.ExecuteQuery(sql);
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                string name = (type == "moto") ? $"{r["Brand"]} {r["Model"]}" : r["Model"].ToString();
                Image img = null;
                try
                {
                    // Собираем путь к картинке
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\", r["ImageURL"].ToString().TrimStart('/').Replace('/', '\\'));
                    if (File.Exists(path)) { using (var ms = new MemoryStream(File.ReadAllBytes(path))) img = Image.FromStream(ms); }
                }
                catch { }

                // Открываем форму ProductDetails
                ProductDetails pd = new ProductDetails("", name, $"{Convert.ToDecimal(r["Price"]):N0} ₽", r["Brand"].ToString(), r["Description"].ToString(), img, id, type);
                pd.ShowDialog();
                LoadCartData();
            }
        }

        // Удаление выделенных строк из корзины
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {row.Cells["CartID"].Value}");
            LoadCartData();
        }

        // Полная очистка корзины текущего пользователя
        private void btnClearCart_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0) return;
            if (MessageBox.Show("Очистить корзину?", "Вопрос", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user}");
                LoadCartData();
            }
        }

        // ФИНАЛЬНЫЙ ШАГ: Оформление заказа
        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0) return;
            string userPhone = maskedTextBox1.Text;

            // Проверка наличия телефона для связи
            if (!maskedTextBox1.MaskFull) { MessageBox.Show("Укажите телефон в профиле!"); return; }

            // Считаем общую сумму заказа
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows) total += Convert.ToDecimal(row.Cells["Сумма"].Value);

            // 1. Создаем запись в таблице Orders и получаем ID нового заказа
            string q = $@"INSERT INTO Orders (UserID, OrderDate, TotalAmount, Status, Phone) 
                        VALUES ({user.id_user}, GETDATE(), {total.ToString().Replace(',', '.')}, N'Новый', '{userPhone}'); SELECT SCOPE_IDENTITY();";
            int orderId = Convert.ToInt32(db.ExecuteScalar(q));

            // 2. Переносим товары из корзины в таблицу состава заказа (OrderItems) и списываем со склада
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                string mid = row.Cells["MotorcycleID"].Value == DBNull.Value ? "NULL" : row.Cells["MotorcycleID"].Value.ToString();
                string pid = row.Cells["PartID"].Value == DBNull.Value ? "NULL" : row.Cells["PartID"].Value.ToString();
                int qtyOrdered = Convert.ToInt32(row.Cells["Кол-во"].Value);
                decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);

                // Добавляем запись о позиции заказа
                db.ExecuteNonQuery($"INSERT INTO OrderItems (OrderID, MotorcycleID, PartID, Quantity, Price) VALUES ({orderId}, {mid}, {pid}, {qtyOrdered}, {price.ToString().Replace(',', '.')})");

                // Списание остатков в таблице Motorcycles или Parts
                if (mid != "NULL") db.ExecuteNonQuery($"UPDATE Motorcycles SET Quantity = Quantity - {qtyOrdered} WHERE MotorcycleID = {mid}");
                else if (pid != "NULL") db.ExecuteNonQuery($"UPDATE Parts SET Quantity = Quantity - {qtyOrdered} WHERE PartID = {pid}");
            }

            // 3. Очищаем корзину, так как заказ уже оформлен
            db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user}");
            MessageBox.Show("Заказ оформлен! Мы скоро свяжемся с вами.");
            LoadCartData();
            LoadOrdersData();
        }
        #endregion

        #region БЛОК: ИСТОРИЯ ЗАКАЗОВ

        // Загрузка списка всех заказов пользователя
        private void LoadOrdersData()
        {
            // Используем подзапрос FOR XML PATH для склейки всех товаров заказа в одну строку
            string query = $@"SELECT o.OrderID as [№], FORMAT(o.OrderDate, 'dd.MM.yyyy HH:mm') as [Дата], 
                (SELECT STUFF((SELECT ', ' + ISNULL(m.Brand + ' ' + m.Model, p.PartName) + ' (' + CAST(oi.Quantity AS VARCHAR) + ' шт.)'
                 FROM OrderItems oi LEFT JOIN Motorcycles m ON oi.MotorcycleID = m.MotorcycleID LEFT JOIN Parts p ON oi.PartID = p.PartID
                 WHERE oi.OrderID = o.OrderID FOR XML PATH('')), 1, 2, '')) as [Состав заказа],
                CAST(o.TotalAmount AS DECIMAL(18,0)) as [Сумма], o.Status as [Статус]
                FROM Orders o WHERE o.UserID = {user.id_user} ORDER BY o.OrderDate DESC";

            dgvOrders.DataSource = db.ExecuteQuery(query);

            if (dgvOrders.Columns.Count > 0)
            {
                dgvOrders.Columns["Состав заказа"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvOrders.Columns["Состав заказа"].DefaultCellStyle.WrapMode = DataGridViewTriState.True; // Разрешаем перенос текста
                dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; // Автовысота строк
            }
        }

        // Отмена заказа (смена статуса)
        private void DeleteOrderBtn_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["№"].Value);
            db.ExecuteNonQuery($"UPDATE Orders SET Status = N'Отменен' WHERE OrderID = {id}");
            LoadOrdersData();
        }
        #endregion

        #region БЛОК: НАВИГАЦИЯ (Переключение вкладок)

        // Клик по пункту "Аккаунт"
        private void panel3_Click(object sender, EventArgs e) => tabControl1.SelectedTab = tabAcc;

        // Клик по пункту "Корзина"
        private void panel4_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabCart; LoadCartData(); }

        // Клик по пункту "Заказы"
        private void panel5_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabOrders; LoadOrdersData(); }

        // Клик по кнопке "Выход" (закрытие формы)
        private void panel2_Click(object sender, EventArgs e) => this.Close();

        // При закрытии формы личного кабинета возвращаемся на главную
        private void profile_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();

        // Метод обновления текста "Итого" под корзиной
        private void UpdateTotalSum()
        {
            decimal t = 0;
            foreach (DataGridViewRow r in dgvCart.Rows)
                if (r.Cells["Сумма"].Value != DBNull.Value) t += Convert.ToDecimal(r.Cells["Сумма"].Value);
            lblTotal.Text = $"Итого: {t:N0} ₽";
        }
        #endregion
    }
}