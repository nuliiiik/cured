using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace cured
{
    /// <summary>
    /// Класс формы администратора: управление заказами, товарами и пользователями.
    /// </summary>
    public partial class admin : Form
    {
        #region ПОЛЯ И ПЕРЕМЕННЫЕ

        private DataGridView dgvAdmin;              // Таблица для отображения данных
        private ComboBox cbTables;                 // Выбор раздела (таблицы)
        private Button btnSave, btnDelete, btnBack, btnAddNewProduct; // Кнопки управления
        private Label lblTitle;                    // Заголовок панели

        DataBase db = new DataBase();              // Подключение к базе данных
        SqlDataAdapter adapter;                    // Адаптер для работы с SQL
        DataSet ds;                                // Хранилище данных в памяти

        // Словарь для связи системных имен таблиц с их красивыми названиями в интерфейсе
        private Dictionary<string, string> tableMapping = new Dictionary<string, string>
        {
            { "Orders", "📜 Управление заказами" },
            { "Motorcycles", "🏍️ Каталог мотоциклов" },
            { "Parts", "🔧 Каталог запчастей" },
            { "Users", "👤 База пользователей" }
        };

        #endregion

        #region КОНСТРУКТОР

        public admin()
        {
            InitializeComponent();
            CreateAdminInterface(); // Инициализация кастомного дизайна
        }

        #endregion

        #region ИНИЦИАЛИЗАЦИЯ ИНТЕРФЕЙСА

        /// <summary>
        /// Динамическое создание элементов управления и настройка внешнего вида формы.
        /// </summary>
        private void CreateAdminInterface()
        {
            // Настройка самой формы
            this.Text = "Панель администратора";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1300, 750);
            this.BackColor = Color.White;

            // Заголовок
            lblTitle = new Label
            {
                Text = "ПАНЕЛЬ УПРАВЛЕНИЯ МАГАЗИНОМ",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 18, FontStyle.Bold)
            };

            // Выпадающий список выбора таблиц
            cbTables = new ComboBox
            {
                Location = new Point(20, 70),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };

            // Подготовка списка разделов для ComboBox
            var displayList = tableMapping.ToList();
            displayList.Insert(0, new KeyValuePair<string, string>("", "--- ВЫБЕРИТЕ РАЗДЕЛ ---"));
            cbTables.DataSource = displayList;
            cbTables.DisplayMember = "Value";
            cbTables.ValueMember = "Key";

            // Событие при выборе раздела
            cbTables.SelectedIndexChanged += (s, e) => {
                string table = cbTables.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(table)) LoadTable(table);
            };

            // Настройка таблицы данных (DataGridView)
            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 115),
                Size = new Size(1240, 500),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.WhiteSmoke
            };
            SetupGridStyle(dgvAdmin);

            // Кнопки действий
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

            // Добавление всех элементов на форму
            this.Controls.AddRange(new Control[] { lblTitle, cbTables, dgvAdmin, btnSave, btnDelete, btnAddNewProduct, btnBack });
        }

        #endregion

        #region ЛОГИКА РАБОТЫ С ДАННЫМИ (ЗАГРУЗКА И ОТОБРАЖЕНИЕ)

        /// <summary>
        /// Загрузка данных из БД в DataGridView в зависимости от выбранной таблицы.
        /// </summary>
        private void LoadTable(string tableName)
        {
            try
            {
                string query = $"SELECT * FROM [{tableName}]";

                // Сложный запрос для заказов (собираем состав заказа из разных таблиц в одну строку)
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
                new SqlCommandBuilder(adapter); // Генерация команд Update/Delete автоматически
                ds = new DataSet();
                adapter.Fill(ds, tableName);

                dgvAdmin.Columns.Clear();
                dgvAdmin.DataSource = ds.Tables[tableName];

                // Специфические настройки для таблицы Заказов
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
                        // Разрешаем редактировать только статус
                        if (col.Name != "Status") col.ReadOnly = true;
                    }
                    ReplaceStatusWithCombo(); // Замена текстового поля статуса на выпадающий список
                    dgvAdmin.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                }
                else
                {
                    dgvAdmin.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                TranslateColumns(); // Перевод заголовков колонок на русский язык
            }
            catch (Exception ex) { MessageBox.Show("Ошибка при чтении данных: " + ex.Message); }
        }

        /// <summary>
        /// Создание выпадающего списка для управления статусом заказа прямо в таблице.
        /// </summary>
        private void ReplaceStatusWithCombo()
        {
            DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn
            {
                Name = "Status",
                HeaderText = "Статус исполнения",
                DataPropertyName = "Status",
                FlatStyle = FlatStyle.Flat
            };
            combo.Items.AddRange("Новый", "В обработке", "Завершен", "Отменен");

            int idx = dgvAdmin.Columns["Status"].Index;
            dgvAdmin.Columns.RemoveAt(idx);
            dgvAdmin.Columns.Insert(idx, combo);
        }

        /// <summary>
        /// Словарь-переводчик для системных заголовков столбцов.
        /// </summary>
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

        #endregion

        #region ОБРАБОТКА СОБЫТИЙ (КЛИКИ)

        /// <summary>
        /// Сохранение измененных в таблице данных обратно в базу.
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            string currentTable = cbTables.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(currentTable)) return;

            try
            {
                dgvAdmin.EndEdit(); // Завершаем текущее редактирование ячейки

                if (currentTable == "Orders")
                {
                    // Для заказов обновляем статус вручную циклом
                    foreach (DataGridViewRow row in dgvAdmin.Rows)
                    {
                        if (row.Cells["OrderID"].Value != null && row.Cells["Status"].Value != null)
                        {
                            int orderId = Convert.ToInt32(row.Cells["OrderID"].Value);
                            string status = row.Cells["Status"].Value.ToString();

                            // N'строка' используется для корректной записи кириллицы в SQL
                            db.ExecuteNonQuery($"UPDATE Orders SET Status = N'{status}' WHERE OrderID = {orderId}");
                        }
                    }
                    MessageBox.Show("Статусы заказов успешно обновлены!", "Успех");
                }
                else
                {
                    // Для простых таблиц используем стандартный метод Update адаптера
                    adapter.Update(ds, currentTable);
                    MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        /// <summary>
        /// Удаление выбранной записи с учетом зависимостей (связанных таблиц).
        /// </summary>
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
                        // Сначала удаляем состав заказа, затем сам заказ
                        db.ExecuteNonQuery($"DELETE FROM OrderItems WHERE OrderID = {id}");
                        db.ExecuteNonQuery($"DELETE FROM Orders WHERE OrderID = {id}");
                        LoadTable("Orders");
                    }
                    else if (table == "Users")
                    {
                        int userId = Convert.ToInt32(dgvAdmin.CurrentRow.Cells["UserID"].Value);

                        // 1. Очищаем корзину пользователя
                        db.ExecuteNonQuery($"DELETE FROM Cart WHERE UserID = {userId}");

                        // 2. Очищаем историю заказов (связанные OrderItems и Orders)
                        db.ExecuteNonQuery($"DELETE FROM OrderItems WHERE OrderID IN (SELECT OrderID FROM Orders WHERE UserID = {userId})");
                        db.ExecuteNonQuery($"DELETE FROM Orders WHERE UserID = {userId}");

                        // 3. Удаляем профиль
                        db.ExecuteNonQuery($"DELETE FROM Users WHERE UserID = {userId}");

                        LoadTable("Users");
                    }
                    else
                    {
                        // Для мотоциклов и запчастей — удаляем из UI и сохраняем изменения
                        dgvAdmin.Rows.Remove(dgvAdmin.CurrentRow);
                        btnSave_Click(null, null);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Удаление невозможно: запись используется в других таблицах."); }
            }
        }

        #endregion

        #region ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ СТИЛИЗАЦИИ

        /// <summary>
        /// Применение визуальных стилей к DataGridView.
        /// </summary>
        private void SetupGridStyle(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 45;
            dgv.RowTemplate.Height = 30;
        }

        /// <summary>
        /// Универсальный метод для создания кнопок в едином стиле.
        /// </summary>
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

        // Событие при закрытии формы: возврат в главное меню
        private void admin_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();

        #endregion
    }
}