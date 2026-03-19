using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public partial class profile : Form
    {
        DataBase db = new DataBase();

        public profile()
        {
            InitializeComponent();
        }

        private void profile_Load(object sender, EventArgs e)
        {
            if (user.id_user <= 0)
            {
                MessageBox.Show("Ошибка: Пользователь не авторизован.");
                this.Close();
                return;
            }

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            LoadUserData();
        }

        #region Навигация и Размеры
        private void panel3_Click(object sender, EventArgs e) // Профиль
        {
            tabControl1.SelectedTab = tabAcc;
        }

        private void panel4_Click(object sender, EventArgs e) // Корзина
        {
            tabControl1.SelectedTab = tabCart;
            LoadCartData();
        }

        private void panel5_Click(object sender, EventArgs e) // Заказы
        {
            tabControl1.SelectedTab = tabOrders;
            LoadOrdersData();
        }

        private void panel2_Click(object sender, EventArgs e) => this.Close();
        #endregion

        #region Аккаунт
        private void LoadUserData()
        {
            try
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
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки данных: " + ex.Message); }
        }

        private void button1_Click(object sender, EventArgs e) // Сохранить
        {
            string query = $@"UPDATE Users SET FullName = '{textBox1.Text}', Login = '{textBox2.Text}', 
                            Password = '{textBox3.Text}', Phone = '{maskedTextBox1.Text}' WHERE UserID = {user.id_user}";
            db.ExecuteNonQuery(query);
            user.full_name = textBox1.Text;
            MessageBox.Show("Данные успешно сохранены!");
        }
        #endregion

        #region Корзина (tabCart)
        private void LoadCartData()
        {
            try
            {
                string query = $@"SELECT c.CartID, ISNULL(m.Brand + ' ' + m.Model, p.PartName) AS [Товар],
                                c.Quantity AS [Кол-во], ISNULL(m.Price, p.Price) AS [Цена],
                                (c.Quantity * ISNULL(m.Price, p.Price)) AS [Сумма]
                                FROM Cart c 
                                LEFT JOIN Motorcycles m ON c.MotorcycleID = m.MotorcycleID
                                LEFT JOIN Parts p ON c.PartID = p.PartID 
                                WHERE c.UserID = {user.id_user}";

                dgvCart.DataSource = db.ExecuteQuery(query);
                if (dgvCart.Columns.Count > 0) dgvCart.Columns["CartID"].Visible = false;
                UpdateTotalSum();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка корзины: " + ex.Message); }
        }

        private void UpdateTotalSum()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
                if (row.Cells["Сумма"].Value != DBNull.Value) total += Convert.ToDecimal(row.Cells["Сумма"].Value);

            lblTotal.Text = $"Итого: {total:N0} ₽";
        }

        // Кнопки управления количеством
        private void btnAddQty_Click(object sender, EventArgs e) => ModifyQty(1);
        private void btnMinusQty_Click(object sender, EventArgs e) => ModifyQty(-1);

        private void ModifyQty(int change)
        {
            if (dgvCart.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
            {
                int id = Convert.ToInt32(row.Cells["CartID"].Value);
                int current = Convert.ToInt32(row.Cells["Кол-во"].Value);
                if (current + change <= 0) db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {id}");
                else db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity + ({change}) WHERE CartID = {id}");
            }
            LoadCartData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {row.Cells["CartID"].Value}");
            LoadCartData();
        }

        // ОФОРМЛЕНИЕ ЗАКАЗА (С ПРОВЕРКАМИ И ТЕЛЕФОНОМ)
        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            // 1. Проверка на наличие товаров (чтобы не было пустых заказов)
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show("Ваша корзина пуста!");
                return;
            }

            // 2. Проверка на заполнение полей профиля
            string phoneNumber = maskedTextBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(textBox1.Text) || phoneNumber.Length < 5)
            {
                MessageBox.Show("Пожалуйста, заполните ФИО и Телефон в профиле перед оформлением!");
                tabControl1.SelectedTab = tabAcc;
                this.Size = new Size(878, 673);
                return;
            }

            try
            {
                decimal total = 0;
                foreach (DataGridViewRow row in dgvCart.Rows)
                    total += Convert.ToDecimal(row.Cells["Сумма"].Value);

                if (total <= 0) return; // Еще одна страховка

                string totalStr = total.ToString().Replace(',', '.');

                // ВСТАВЛЯЕМ ЗАКАЗ (Включая телефон из maskedTextBox1)
                string insertOrder = $@"INSERT INTO Orders (UserID, OrderDate, TotalAmount, Status, Phone) 
                                       VALUES ({user.id_user}, GETDATE(), {totalStr}, N'Новый', '{phoneNumber}'); 
                                       SELECT SCOPE_IDENTITY();";

                int newId = Convert.ToInt32(db.ExecuteScalar(insertOrder));

                // Копируем детали заказа
                DataTable dt = db.ExecuteQuery($@"SELECT c.MotorcycleID, c.PartID, c.Quantity, ISNULL(m.Price, p.Price) as Price 
                                               FROM Cart c LEFT JOIN Motorcycles m ON c.MotorcycleID = m.MotorcycleID 
                                               LEFT JOIN Parts p ON c.PartID = p.PartID WHERE c.UserID = {user.id_user}");

                foreach (DataRow r in dt.Rows)
                {
                    string mId = r["MotorcycleID"] == DBNull.Value ? "NULL" : r["MotorcycleID"].ToString();
                    string pId = r["PartID"] == DBNull.Value ? "NULL" : r["PartID"].ToString();
                    string price = Convert.ToDecimal(r["Price"]).ToString().Replace(',', '.');

                    db.ExecuteNonQuery($@"INSERT INTO OrderItems (OrderID, MotorcycleID, PartID, Quantity, Price) 
                                         VALUES ({newId}, {mId}, {pId}, {r["Quantity"]}, {price})");
                }

                // Очистка
                db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {user.id_user}");
                MessageBox.Show($"Заказ №{newId} оформлен!");
                LoadCartData();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка оформления: " + ex.Message); }
        }
        #endregion

        #region Заказы (tabOrders)
        private void LoadOrdersData()
        {
            try
            {
                string query = $@"SELECT o.OrderID as [№ Заказа], o.OrderDate as [Дата], 
                                (SELECT STUFF((SELECT ', ' + ISNULL(m.Brand + ' ' + m.Model, p.PartName) FROM OrderItems oi 
                                LEFT JOIN Motorcycles m ON oi.MotorcycleID = m.MotorcycleID LEFT JOIN Parts p ON oi.PartID = p.PartID 
                                WHERE oi.OrderID = o.OrderID FOR XML PATH('')), 1, 2, '')) as [Товары],
                                o.TotalAmount as [Сумма], o.Status as [Статус]
                                FROM Orders o WHERE o.UserID = {user.id_user} ORDER BY o.OrderDate DESC";

                dgvOrders.DataSource = db.ExecuteQuery(query);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка истории: " + ex.Message); }
        }

        private void DeleteOrderBtn_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["№ Заказа"].Value);
            db.ExecuteNonQuery($"UPDATE Orders SET Status = N'Отменен' WHERE OrderID = {orderId}");
            LoadOrdersData();
        }
        #endregion

        private void profile_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();
    }
}