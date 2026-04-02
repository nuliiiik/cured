using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace cured
{
    public partial class admin : Form
    {
        private DataGridView dgvAdmin;
        private ComboBox cbTables;
        private Button btnSave, btnDelete, btnBack, btnAddNewProduct;
        private Label lblTitle;

        DataBase db = new DataBase();
        SqlDataAdapter adapter;
        DataSet ds;

        // Русские названия категорий
        private Dictionary<string, string> tableMapping = new Dictionary<string, string>
        {
            { "Orders", "📜 Управление заказами" },
            { "Motorcycles", "🏍️ Каталог мотоциклов" },
            { "Parts", "🔧 Каталог запчастей" },
            { "Users", "👤 База пользователей" }
        };

        public admin()
        {
            InitializeComponent();
            CreateAdminInterface();
        }

        private void CreateAdminInterface()
        {
            this.Text = "Панель администратора";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1300, 750);
            this.BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "ПАНЕЛЬ УПРАВЛЕНИЯ МАГАЗИНОМ",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 18, FontStyle.Bold)
            };

            cbTables = new ComboBox
            {
                Location = new Point(20, 70),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };

            var displayList = tableMapping.ToList();
            displayList.Insert(0, new KeyValuePair<string, string>("", "--- ВЫБЕРИТЕ РАЗДЕЛ ---"));
            cbTables.DataSource = displayList;
            cbTables.DisplayMember = "Value";
            cbTables.ValueMember = "Key";
            cbTables.SelectedIndexChanged += (s, e) => {
                string table = cbTables.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(table)) LoadTable(table);
            };

            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 115),
                Size = new Size(1240, 500),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.WhiteSmoke
            };
            SetupGridStyle(dgvAdmin);

            btnSave = CreateStyledButton("💾 СОХРАНИТЬ ИЗМЕНЕНИЯ", new Point(20, 640), Color.ForestGreen);
            btnSave.Click += btnSave_Click;

            btnDelete = CreateStyledButton("🗑️ УДАЛИТЬ ВЫБРАННОЕ", new Point(230, 640), Color.DarkRed);
            btnDelete.Click += btnDelete_Click;

            btnAddNewProduct = CreateStyledButton("➕ ДОБАВИТЬ НОВЫЙ ТОВАР", new Point(440, 640), Color.DodgerBlue);
            btnAddNewProduct.Click += (s, e) => {
                using (FormAddProduct f = new FormAddProduct())
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        string table = cbTables.SelectedValue?.ToString();
                        if (table == "Motorcycles" || table == "Parts") LoadTable(table);
                    }
                }
            };

            btnBack = CreateStyledButton("↩️ ВЕРНУТЬСЯ НАЗАД", new Point(1080, 640), Color.Gray);
            btnBack.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, cbTables, dgvAdmin, btnSave, btnDelete, btnAddNewProduct, btnBack });
        }

        private void LoadTable(string tableName)
        {
            try
            {
                string query = $"SELECT * FROM [{tableName}]";

                if (tableName == "Orders")
                {
                    query = @"SELECT o.OrderID, u.FullName as [ФИО Клиента], o.Phone as [Контактный Телефон], 
                              (SELECT STUFF((SELECT ', ' + ISNULL(m.Brand + ' ' + m.Model, p.PartName) + ' (' + CAST(oi.Quantity AS VARCHAR) + ' шт.)'
                               FROM OrderItems oi 
                               LEFT JOIN Motorcycles m ON oi.MotorcycleID = m.MotorcycleID 
                               LEFT JOIN Parts p ON oi.PartID = p.PartID
                               WHERE oi.OrderID = o.OrderID FOR XML PATH('')), 1, 2, '')) as [Состав заказа],
                              CAST(o.TotalAmount AS DECIMAL(18,0)) as [Итоговая Сумма], 
                              o.OrderDate as [Дата оформления], o.Status 
                              FROM Orders o 
                              LEFT JOIN Users u ON o.UserID = u.UserID";
                }

                adapter = new SqlDataAdapter(query, db.getConnection());
                new SqlCommandBuilder(adapter);
                ds = new DataSet();
                adapter.Fill(ds, tableName);

                dgvAdmin.Columns.Clear();
                dgvAdmin.DataSource = ds.Tables[tableName];

                if (tableName == "Orders")
                {
                    foreach (DataGridViewColumn col in dgvAdmin.Columns)
                    {
                        if (col.Name == "Состав заказа")
                        {
                            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        }
                        else
                        {
                            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        }
                        if (col.Name != "Status") col.ReadOnly = true;
                    }
                    ReplaceStatusWithCombo();
                    dgvAdmin.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                }
                else
                {
                    dgvAdmin.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                TranslateColumns();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка при чтении данных: " + ex.Message); }
        }

        private void ReplaceStatusWithCombo()
        {
            DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn
            {
                HeaderText = "Статус исполнения",
                DataPropertyName = "Status",
                FlatStyle = FlatStyle.Flat
            };
            combo.Items.AddRange("Новый", "В обработке", "Завершен", "Отменен");
            int idx = dgvAdmin.Columns["Status"].Index;
            dgvAdmin.Columns.RemoveAt(idx);
            dgvAdmin.Columns.Insert(idx, combo);
        }

        private void TranslateColumns()
        {
            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "OrderID", "№ Заказа" }, { "MotorcycleID", "ID Мото" }, { "PartID", "ID Детали" },
                { "Brand", "Марка / Бренд" }, { "Model", "Модель" }, { "Price", "Цена (₽)" }, { "Quantity", "Остаток на складе" }, 
                { "FullName", "Полное имя" },{ "UserID", "ID пользователя" }, { "Login", "Логин" },{ "Role", "Роль" },
                { "Password", "Пароль" }, { "Phone", "Телефон" }, { "PartName", "Название детали" },
                { "Description", "Описание товара" }, { "ImageURL", "Путь к картинке" }, { "Year", "Год выпуска" }
            };
            foreach (DataGridViewColumn col in dgvAdmin.Columns)
                if (dict.ContainsKey(col.Name)) col.HeaderText = dict[col.Name];
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                dgvAdmin.EndEdit();
                adapter.Update(ds, cbTables.SelectedValue.ToString());
                MessageBox.Show("Все изменения успешно применены в базе данных!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения изменений: " + ex.Message); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAdmin.CurrentRow == null || dgvAdmin.CurrentRow.IsNewRow) return;
            if (MessageBox.Show("Вы уверены, что хотите безвозвратно удалить эту запись?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string table = cbTables.SelectedValue.ToString();
                    if (table == "Orders")
                    {
                        int id = Convert.ToInt32(dgvAdmin.CurrentRow.Cells["OrderID"].Value);
                        db.ExecuteNonQuery($"DELETE FROM OrderItems WHERE OrderID = {id}");
                        db.ExecuteNonQuery($"DELETE FROM Orders WHERE OrderID = {id}");
                        LoadTable("Orders");
                    }
                    else if (table == "Users")
                    {
                        int userId = Convert.ToInt32(dgvAdmin.CurrentRow.Cells["UserID"].Value);

                        // 1. Очищаем корзину пользователя
                        db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {userId}");

                        // 2. Если нужно удалять и заказы пользователя (опционально):
                        // Сначала удаляем позиции всех его заказов
                        db.ExecuteNonQuery($"DELETE FROM OrderItems WHERE OrderID IN (SELECT OrderID FROM Orders WHERE UserID = {userId})");
                        // Затем сами заказы
                        db.ExecuteNonQuery($"DELETE FROM Orders WHERE UserID = {userId}");

                        // 3. Теперь удаляем самого пользователя
                        db.ExecuteNonQuery($"DELETE FROM Users WHERE UserID = {userId}");

                        LoadTable("Users"); // Перезагружаем таблицу
                    }
                    else
                    {
                        // Для остальных таблиц оставляем как было
                        dgvAdmin.Rows.Remove(dgvAdmin.CurrentRow);
                        btnSave_Click(null, null);
                    }

                }
                catch (Exception ex) { MessageBox.Show("Удаление невозможно: запись используется в других таблицах."); }
            }
        }

        private void SetupGridStyle(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 45;
            dgv.RowTemplate.Height = 30;
        }

        private Button CreateStyledButton(string text, Point location, Color backColor) => new Button
        {
            Text = text,
            Location = location,
            Size = new Size(200, 50),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        private void admin_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();
    }
}