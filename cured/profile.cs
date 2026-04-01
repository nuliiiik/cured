using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cured
{
    public partial class profile : Form
    {
        private DataBase db = new DataBase();

        public profile()
        {
            InitializeComponent();
            // Подвязываем Enter к текстовым полям для сохранения профиля
            textBox1.KeyDown += CheckEnter;
            textBox2.KeyDown += CheckEnter;
            textBox3.KeyDown += CheckEnter;
        }

        private void profile_Load(object sender, EventArgs e)
        {
            if (user.id_user <= 0) { this.Close(); return; }

            // Настройка вкладок
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            // ПРИМЕНЕНИЕ ТВОИХ СТИЛЕЙ
            ApplyGridStyles(dgvCart);
            ApplyGridStyles(dgvOrders);

            // Подписка на события
            dgvCart.CellDoubleClick += dgvCart_CellDoubleClick;
            dgvCart.CellValueChanged += dgvCart_CellValueChanged;

            LoadUserData();
            LoadCartData();
            LoadOrdersData();
        }

        private void ApplyGridStyles(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;

            // Базовый режим — по содержимому, позже для главных столбцов переопределим
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        #region АККАУНТ
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length < 4 || textBox3.Text.Length < 4)
            {
                MessageBox.Show("Логин и пароль должны быть не короче 4 символов!");
                return;
            }

            db.ExecuteNonQuery($@"UPDATE Users SET FullName = N'{textBox1.Text}', Login = '{textBox2.Text}', 
                                Password = '{textBox3.Text}', Phone = '{maskedTextBox1.Text}' WHERE UserID = {user.id_user}");
            MessageBox.Show("Данные профиля обновлены!");
        }

        private void CheckEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.SuppressKeyPress = true;
            }
        }
        #endregion

        #region КОРЗИНА
        public void LoadCartData()
        {
            dgvCart.CellValueChanged -= dgvCart_CellValueChanged;

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
                dgvCart.Columns["CartID"].Visible = false;
                dgvCart.Columns["MotorcycleID"].Visible = false;
                dgvCart.Columns["PartID"].Visible = false;
                dgvCart.Columns["Stock"].Visible = false;

                foreach (DataGridViewColumn col in dgvCart.Columns) col.ReadOnly = true;
                dgvCart.Columns["Кол-во"].ReadOnly = false;

                // НАСТРОЙКА ШИРИНЫ КОРЗИНЫ
                dgvCart.Columns["Товар"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Забирает всё место
                dgvCart.Columns["Кол-во"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvCart.Columns["Цена"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvCart.Columns["Сумма"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            UpdateTotalSum();
            dgvCart.CellValueChanged += dgvCart_CellValueChanged;
        }

        private void dgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvCart.Columns[e.ColumnIndex].Name == "Кол-во")
            {
                int cartId = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["CartID"].Value);
                int stock = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["Stock"].Value);

                if (int.TryParse(dgvCart.Rows[e.RowIndex].Cells["Кол-во"].Value.ToString(), out int newQty))
                {
                    if (newQty > stock)
                    {
                        MessageBox.Show($"Извините, на складе доступно только {stock} шт.!", "Недостаточно товара");
                        newQty = stock;
                    }

                    if (newQty <= 0) db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
                    else db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE CartID = {cartId}");
                }
                LoadCartData();
            }
        }

        private void dgvCart_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCart.Columns[e.ColumnIndex].Name == "Кол-во") return;

            int mId = 0, pId = 0;
            int.TryParse(dgvCart.Rows[e.RowIndex].Cells["MotorcycleID"].Value?.ToString(), out mId);
            int.TryParse(dgvCart.Rows[e.RowIndex].Cells["PartID"].Value?.ToString(), out pId);

            int id = (mId > 0) ? mId : pId;
            string type = (mId > 0) ? "moto" : "part";

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
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\", r["ImageURL"].ToString().TrimStart('/').Replace('/', '\\'));
                    if (File.Exists(path))
                    {
                        using (var ms = new MemoryStream(File.ReadAllBytes(path))) img = Image.FromStream(ms);
                    }
                }
                catch { }

                ProductDetails pd = new ProductDetails("", name, $"{Convert.ToDecimal(r["Price"]):N0} ₽", r["Brand"].ToString(), r["Description"].ToString(), img, id, type);
                pd.ShowDialog();
                LoadCartData();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {row.Cells["CartID"].Value}");
            LoadCartData();
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0) return;
            if (MessageBox.Show("Полностью очистить корзину?", "Внимание", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user}");
                LoadCartData();
            }
        }

        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0) return;
            if (!maskedTextBox1.MaskFull) { MessageBox.Show("Укажите номер телефона!"); return; }

            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows) total += Convert.ToDecimal(row.Cells["Сумма"].Value);

            string q = $@"INSERT INTO Orders (UserID, OrderDate, TotalAmount, Status, Phone) 
                        VALUES ({user.id_user}, GETDATE(), {total.ToString().Replace(',', '.')}, N'Новый', '{maskedTextBox1.Text}'); SELECT SCOPE_IDENTITY();";
            int orderId = Convert.ToInt32(db.ExecuteScalar(q));

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                string mid = row.Cells["MotorcycleID"].Value == DBNull.Value ? "NULL" : row.Cells["MotorcycleID"].Value.ToString();
                string pid = row.Cells["PartID"].Value == DBNull.Value ? "NULL" : row.Cells["PartID"].Value.ToString();
                db.ExecuteNonQuery($"INSERT INTO OrderItems (OrderID, MotorcycleID, PartID, Quantity, Price) VALUES ({orderId}, {mid}, {pid}, {row.Cells["Кол-во"].Value}, {row.Cells["Цена"].Value.ToString().Replace(',', '.')})");
            }
            db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user}");
            MessageBox.Show("Заказ успешно оформлен!");
            LoadCartData();
            LoadOrdersData();
        }
        #endregion

        #region ИСТОРИЯ
        private void LoadOrdersData()
        {
            string query = $@"SELECT o.OrderID as [№], FORMAT(o.OrderDate, 'dd.MM.yyyy HH:mm') as [Дата], 
                (SELECT STUFF((SELECT ', ' + ISNULL(m.Brand + ' ' + m.Model, p.PartName) + ' (' + CAST(oi.Quantity AS VARCHAR) + ' шт.)'
                 FROM OrderItems oi LEFT JOIN Motorcycles m ON oi.MotorcycleID = m.MotorcycleID LEFT JOIN Parts p ON oi.PartID = p.PartID
                 WHERE oi.OrderID = o.OrderID FOR XML PATH('')), 1, 2, '')) as [Состав заказа],
                CAST(o.TotalAmount AS DECIMAL(18,0)) as [Сумма], o.Status as [Статус]
                FROM Orders o WHERE o.UserID = {user.id_user} ORDER BY o.OrderDate DESC";

            dgvOrders.DataSource = db.ExecuteQuery(query);

            if (dgvOrders.Columns.Count > 0)
            {
                // НАСТРОЙКА ШИРИНЫ ИСТОРИИ ЗАКАЗОВ
                dgvOrders.Columns["№"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvOrders.Columns["Дата"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvOrders.Columns["Сумма"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvOrders.Columns["Статус"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                // Главный столбец расширяется на всё свободное пространство
                dgvOrders.Columns["Состав заказа"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                // Разрешаем перенос текста в составе заказа для наглядности
                dgvOrders.Columns["Состав заказа"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            }
        }

        private void DeleteOrderBtn_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["№"].Value);
            db.ExecuteNonQuery($"UPDATE Orders SET Status = N'Отменен' WHERE OrderID = {id}");
            LoadOrdersData();
        }
        #endregion

        #region НАВИГАЦИЯ И ИТОГИ
        private void panel3_Click(object sender, EventArgs e) => tabControl1.SelectedTab = tabAcc;
        private void panel4_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabCart; LoadCartData(); }
        private void panel5_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabOrders; LoadOrdersData(); }
        private void panel2_Click(object sender, EventArgs e) => this.Close();
        private void profile_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();

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