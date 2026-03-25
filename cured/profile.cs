using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public partial class profile : Form
    {
        DataBase db = new DataBase();

        public profile() { InitializeComponent(); }

        private void profile_Load(object sender, EventArgs e)
        {
            if (user.id_user <= 0) { this.Close(); return; }

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            // Настройка дизайна таблиц
            SetupTableStyle(dgvCart);
            SetupTableStyle(dgvOrders);

            // Подписка на изменение количества
            dgvCart.CellValueChanged += dgvCart_CellValueChanged;

            LoadUserData();
        }

        private void SetupTableStyle(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false; // Убираем пустую строку внизу для защиты от Null
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Стиль заголовков (DarkRed)
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;

            // Стиль строк
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        #region Аккаунт
        private void LoadUserData()
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

        private void button1_Click(object sender, EventArgs e)
        {
            string query = $@"UPDATE Users SET FullName = '{textBox1.Text}', Login = '{textBox2.Text}', 
                            Password = '{textBox3.Text}', Phone = '{maskedTextBox1.Text}' WHERE UserID = {user.id_user}";
            db.ExecuteNonQuery(query);
            user.full_name = textBox1.Text;
            MessageBox.Show("Данные сохранены!");
        }
        #endregion

        #region Корзина
        private void LoadCartData()
        {
            dgvCart.CellValueChanged -= dgvCart_CellValueChanged;

            // Запрос с учетом остатков на складе
            string query = $@"SELECT c.CartID, c.MotorcycleID, c.PartID,
                            ISNULL(m.Brand + ' ' + m.Model, p.PartName) AS [Товар],
                            c.Quantity AS [Кол-во], 
                            ISNULL(m.Quantity, p.Quantity) AS [НаСкладе],
                            ISNULL(m.Price, p.Price) AS [Цена],
                            (c.Quantity * ISNULL(m.Price, p.Price)) AS [Сумма]
                            FROM Cart c 
                            LEFT JOIN Motorcycles m ON c.MotorcycleID = m.MotorcycleID
                            LEFT JOIN Parts p ON c.PartID = p.PartID 
                            WHERE c.UserID = {user.id_user}";

            DataTable dt = db.ExecuteQuery(query);

            // Проверка: удаляем если 0 на складе, уменьшаем если не хватает
            foreach (DataRow row in dt.Rows)
            {
                int inCart = Convert.ToInt32(row["Кол-во"]);
                int inStock = Convert.ToInt32(row["НаСкладе"]);
                int cartId = Convert.ToInt32(row["CartID"]);

                if (inStock <= 0) db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
                else if (inCart > inStock) db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {inStock} WHERE CartID = {cartId}");
            }

            dgvCart.DataSource = db.ExecuteQuery(query);

            if (dgvCart.Columns.Count > 0)
            {
                dgvCart.Columns["CartID"].Visible = false;
                dgvCart.Columns["MotorcycleID"].Visible = false;
                dgvCart.Columns["PartID"].Visible = false;
                dgvCart.Columns["НаСкладе"].Visible = false;

                // Разрешаем редактировать только "Кол-во"
                dgvCart.ReadOnly = false;
                foreach (DataGridViewColumn col in dgvCart.Columns)
                {
                    if (col.Name == "Кол-во")
                    {
                        col.ReadOnly = false;
                        col.DefaultCellStyle.ForeColor = Color.DarkRed;
                        col.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else col.ReadOnly = true;
                }
            }

            UpdateTotalSum();
            dgvCart.CellValueChanged += dgvCart_CellValueChanged;
        }

        private void dgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Защита от Null и некорректных индексов
            if (e.RowIndex < 0 || dgvCart.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null) return;

            if (dgvCart.Columns[e.ColumnIndex].Name == "Кол-во")
            {
                try
                {
                    int cartId = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["CartID"].Value);
                    int inStock = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["НаСкладе"].Value);
                    int newQty = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                    if (newQty > inStock)
                    {
                        MessageBox.Show($"Превышен лимит! На складе всего: {inStock}");
                        newQty = inStock;
                    }

                    if (newQty <= 0) db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
                    else db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE CartID = {cartId}");

                    LoadCartData();
                }
                catch { LoadCartData(); }
            }
        }

        private void UpdateTotalSum()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                // Проверка на Null при подсчете суммы
                if (!row.IsNewRow && row.Cells["Сумма"].Value != null && row.Cells["Сумма"].Value != DBNull.Value)
                    total += Convert.ToDecimal(row.Cells["Сумма"].Value);
            }
            lblTotal.Text = $"Итого: {total:N0} ₽"; // Используем lblTotal
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {row.Cells["CartID"].Value}");
            LoadCartData();
        }

        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0) { MessageBox.Show("Корзина пуста!"); return; }
            if (string.IsNullOrWhiteSpace(textBox1.Text) || maskedTextBox1.Text.Length < 5) { MessageBox.Show("Заполните профиль!"); return; }

            LoadCartData(); // Свежая проверка перед заказом
            if (dgvCart.Rows.Count == 0) return;

            try
            {
                decimal total = 0;
                foreach (DataGridViewRow row in dgvCart.Rows) total += Convert.ToDecimal(row.Cells["Сумма"].Value);

                string insertOrder = $@"INSERT INTO Orders (UserID, OrderDate, TotalAmount, Status, Phone) 
                                       VALUES ({user.id_user}, GETDATE(), {total.ToString().Replace(',', '.')}, N'Новый', '{maskedTextBox1.Text}');
                                       SELECT SCOPE_IDENTITY();";
                int newId = Convert.ToInt32(db.ExecuteScalar(insertOrder));

                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    string mId = row.Cells["MotorcycleID"].Value == DBNull.Value ? "NULL" : row.Cells["MotorcycleID"].Value.ToString();
                    string pId = row.Cells["PartID"].Value == DBNull.Value ? "NULL" : row.Cells["PartID"].Value.ToString();
                    int qty = Convert.ToInt32(row.Cells["Кол-во"].Value);

                    db.ExecuteNonQuery($@"INSERT INTO OrderItems (OrderID, MotorcycleID, PartID, Quantity, Price)
                                         VALUES ({newId}, {mId}, {pId}, {qty}, {row.Cells["Цена"].Value.ToString().Replace(',', '.')})");

                    if (mId != "NULL") db.ExecuteNonQuery($"UPDATE Motorcycles SET Quantity = Quantity - {qty} WHERE MotorcycleID = {mId}");
                    else db.ExecuteNonQuery($"UPDATE Parts SET Quantity = Quantity - {qty} WHERE PartID = {pId}");
                }

                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user}");
                MessageBox.Show("Заказ оформлен!");
                LoadCartData();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        #endregion

        #region Заказы
        private void LoadOrdersData()
        {
            try
            {
                string query = $@"
        SELECT 
            o.OrderID as [№], 
            FORMAT(o.OrderDate, 'dd.MM.yyyy') as [Дата], 
            (SELECT STUFF((
                SELECT ', ' + ISNULL(m.Brand + ' ' + m.Model, p.PartName) + ' (' + CAST(oi.Quantity AS VARCHAR) + ' шт.)'
                FROM OrderItems oi 
                LEFT JOIN Motorcycles m ON oi.MotorcycleID = m.MotorcycleID
                LEFT JOIN Parts p ON oi.PartID = p.PartID
                WHERE oi.OrderID = o.OrderID 
                FOR XML PATH('')), 1, 2, '')) as [Товары],
            CAST(o.TotalAmount AS DECIMAL(18,0)) as [Сумма], 
            o.Status as [Статус]
        FROM Orders o 
        WHERE o.UserID = {user.id_user} 
        ORDER BY o.OrderDate DESC";

                dgvOrders.DataSource = db.ExecuteQuery(query);

                if (dgvOrders.Columns.Count > 0)
                {
                    dgvOrders.Columns["№"].FillWeight = 30;
                    dgvOrders.Columns["Дата"].FillWeight = 60;
                    dgvOrders.Columns["Товары"].FillWeight = 220; // Немного увеличил ширину для текста с количеством
                    dgvOrders.Columns["Сумма"].FillWeight = 60;
                    dgvOrders.Columns["Статус"].FillWeight = 70;
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void DeleteOrderBtn_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["№"].Value);
            db.ExecuteNonQuery($"UPDATE Orders SET Status = N'Отменен' WHERE OrderID = {orderId}");
            LoadOrdersData();
        }
        #endregion

        #region Навигация
        private void panel3_Click(object sender, EventArgs e) => tabControl1.SelectedTab = tabAcc;
        private void panel4_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabCart; LoadCartData(); }
        private void panel5_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabOrders; LoadOrdersData(); }
        private void panel2_Click(object sender, EventArgs e) => this.Close();
        #endregion

        private void profile_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();
    }
}