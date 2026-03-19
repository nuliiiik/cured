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
            // Настройка вкладок
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            dgvCart.CellValueChanged += dgvCart_CellValueChanged;
            LoadCartData();
        }

        private void LoadCartData()
        {
            try
            {
                string query = $@"
            SELECT 
                c.CartID, 
                ISNULL(m.Brand + ' ' + m.Model, p.PartName) AS [Товар],
                c.Quantity AS [Кол-во],
                ISNULL(m.Price, p.Price) AS [Цена],
                (c.Quantity * ISNULL(m.Price, p.Price)) AS [Сумма]
            FROM Cart c
            LEFT JOIN Motorcycles m ON c.MotorcycleID = m.MotorcycleID
            LEFT JOIN Parts p ON c.PartID = p.PartID
            WHERE c.UserID = {user.id_user}";

                // Отключаем события на время заливки данных, чтобы не сработал CellValueChanged
                dgvCart.CellValueChanged -= dgvCart_CellValueChanged;
                dgvCart.DataSource = db.ExecuteQuery(query);
                dgvCart.CellValueChanged += dgvCart_CellValueChanged;

                // Настройки колонок
                if (dgvCart.Columns.Count > 0)
                {
                    dgvCart.Columns["CartID"].Visible = false;
                    dgvCart.Columns["Товар"].ReadOnly = true;
                    dgvCart.Columns["Цена"].ReadOnly = true;
                    dgvCart.Columns["Сумма"].ReadOnly = true;
                    dgvCart.Columns["Кол-во"].ReadOnly = false; // Разрешаем ввод
                }
                UpdateTotalSum();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // СОБЫТИЕ: Срабатывает после того, как вы вписали число и нажали Enter
        private void dgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что изменили именно колонку "Кол-во"
            if (dgvCart.Columns[e.ColumnIndex].Name == "Кол-во")
            {
                try
                {
                    int cartId = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["CartID"].Value);
                    string val = dgvCart.Rows[e.RowIndex].Cells["Кол-во"].Value.ToString();

                    if (int.TryParse(val, out int newQty) && newQty >= 0)
                    {
                        if (newQty == 0)
                        {
                            db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
                        }
                        else
                        {
                            db.ExecuteNonQuery($"UPDATE Cart SET Quantity = {newQty} WHERE CartID = {cartId}");
                        }
                        LoadCartData(); // Перегружаем, чтобы пересчитать колонку "Сумма" и Итого
                    }
                    else
                    {
                        MessageBox.Show("Введите корректное целое число больше 0");
                        LoadCartData(); // Откатываем назад
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        // Кнопка "Изменить данные" (button1)
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка на пустые поля
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Поля не могут быть пустыми!");
                return;
            }

            // Формируем запрос на обновление только для текущего UserID
            string query = $@"UPDATE Users SET 
                            FullName = '{textBox1.Text}', 
                            Login = '{textBox2.Text}', 
                            Password = '{textBox3.Text}', 
                            Phone = '{maskedTextBox1.Text}' 
                            WHERE UserID = {user.id_user}";

            try
            {
                db.ExecuteNonQuery(query);

                // Обновляем данные в глобальном классе, чтобы на главной имя тоже сменилось
                user.full_name = textBox1.Text;
                user.login_user = textBox2.Text;

                MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void UpdateTotalSum()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (row.Cells["Сумма"].Value != DBNull.Value)
                    total += Convert.ToDecimal(row.Cells["Сумма"].Value);
            }
            lblTotal.Text = $"Итого: {total:N0} ₽";
        }

        private void profile_FormClosed(object sender, FormClosedEventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        // Навигация по панелям (как у вас в коде)
        private void panel2_Click(object sender, EventArgs e)
        {
            main form_main = new main();
            form_main.Show();
            this.Hide();
        }

        private void panel3_Click(object sender, EventArgs e) => tabControl1.SelectedTab = tabAcc;
        private void panel4_Click(object sender, EventArgs e) { tabControl1.SelectedTab = tabCart; LoadCartData(); }
        private void panel5_Click(object sender, EventArgs e) => tabControl1.SelectedTab = tabOrders;

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) return;

            var result = MessageBox.Show("Удалить выбранные товары из корзины?", "Подтверждение", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dgvCart.SelectedRows)
                {
                    int cartId = Convert.ToInt32(row.Cells["CartID"].Value);
                    db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
                }
                LoadCartData();
            }
        }

        private void btnMinusQty_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
            {
                int cartId = Convert.ToInt32(row.Cells["CartID"].Value);
                int currentQty = Convert.ToInt32(row.Cells["Кол-во"].Value);

                if (currentQty > 1)
                    db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity - 1 WHERE CartID = {cartId}");
                else
                    db.ExecuteNonQuery($"DELETE FROM Cart WHERE CartID = {cartId}");
            }
            LoadCartData();
        }

        private void btnAddQty_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvCart.SelectedRows)
            {
                int cartId = Convert.ToInt32(row.Cells["CartID"].Value);
                db.ExecuteNonQuery($"UPDATE Cart SET Quantity = Quantity + 1 WHERE CartID = {cartId}");
            }
            LoadCartData();
        }
    }
}