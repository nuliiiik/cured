using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace cured
{
    /// <summary>
    /// Класс управления административной панелью.
    /// Позволяет просматривать, редактировать и удалять данные напрямую из таблиц БД.
    /// </summary>
    public partial class admin : Form
    {
        #region Переменные и инициализация

        private DataGridView dgvAdmin;
        private ComboBox cbTables;
        private Button btnSave, btnDelete, btnBack;
        private Label lblTitle;

        // Объекты для работы с данными
        DataBase db = new DataBase();
        SqlDataAdapter adapter;
        DataSet ds;

        public admin()
        {
            InitializeComponent();
            CreateAdminInterface();
        }

        #endregion

        #region Интерфейс и Стилизация

        /// <summary>
        /// Динамическое создание элементов управления и настройка внешнего вида формы.
        /// </summary>
        private void CreateAdminInterface()
        {
            // Параметры окна
            this.Text = "Управление базой данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 700);
            this.BackColor = Color.White;

            // Заголовок в фирменном стиле DarkRed
            lblTitle = new Label
            {
                Text = "ПАНЕЛЬ АДМИНИСТРАТОРА",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            // Выпадающий список для выбора таблиц
            cbTables = new ComboBox
            {
                Location = new Point(20, 65),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cbTables.Items.AddRange(new string[] { "Users", "Motorcycles", "Parts", "Orders", "OrderItems", "Cart" });

            // Событие смены таблицы
            cbTables.SelectedIndexChanged += (s, e) => {
                if (cbTables.SelectedItem != null)
                    LoadTable(cbTables.SelectedItem.ToString());
            };

            // Настройка основной таблицы данных
            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(1040, 460),
                BorderStyle = BorderStyle.None
            };
            SetupGridStyle(dgvAdmin);

            // Инициализация кнопок управления
            btnSave = CreateStyledButton("СОХРАНИТЬ ИЗМЕНЕНИЯ", new Point(20, 590), Color.ForestGreen);
            btnSave.Click += btnSave_Click;

            btnDelete = CreateStyledButton("УДАЛИТЬ СТРОКУ", new Point(220, 590), Color.DarkRed);
            btnDelete.Click += btnDelete_Click;

            btnBack = CreateStyledButton("ВЕРНУТЬСЯ", new Point(880, 590), Color.Gray);
            btnBack.Click += (s, e) => this.Close();

            // Добавление элементов на форму
            this.Controls.Add(lblTitle);
            this.Controls.Add(cbTables);
            this.Controls.Add(dgvAdmin);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnBack);
        }

        /// <summary>
        /// Универсальный метод для создания кнопок в едином визуальном стиле.
        /// </summary>
        private Button CreateStyledButton(string text, Point location, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(190, 45),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        /// <summary>
        /// Настройка визуального оформления DataGridView (цвета, шрифты, заголовки).
        /// </summary>
        private void SetupGridStyle(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToResizeRows = false;

            // Стилизация шапки
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            // Стилизация контента
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        #endregion

        #region Работа с данными

        /// <summary>
        /// Загрузка данных из выбранной таблицы БД в DataGridView.
        /// Использует SqlDataAdapter для автоматической генерации команд вставки/правки.
        /// </summary>
        private void LoadTable(string tableName)
        {
            try
            {
                string query = $"SELECT * FROM {tableName}";
                SqlConnection con = db.getConnection();

                adapter = new SqlDataAdapter(query, con);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter); // Генерирует SQL команды Update/Delete

                ds = new DataSet();
                adapter.Fill(ds, tableName);
                dgvAdmin.DataSource = ds.Tables[tableName];

                TranslateColumns(); // Локализация заголовков
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Локализация заголовков столбцов из системных имен БД в читаемый русский вид.
        /// </summary>
        private void TranslateColumns()
        {
            Dictionary<string, string> translations = new Dictionary<string, string>
            {
                // Пользователи
                { "UserID", "ID" },
                { "Login", "Логин" },
                { "Password", "Пароль" },
                { "FullName", "ФИО" },
                { "Phone", "Телефон" },
                { "Role", "Роль" },

                // Мотоциклы и Запчасти
                { "MotorcycleID", "ID Мото" },
                { "PartID", "ID Детали" },
                { "Brand", "Марка" },
                { "Model", "Модель" },
                { "Year", "Год" },
                { "Price", "Цена (₽)" },
                { "Description", "Описание" },
                { "Quantity", "Остаток/Кол-во" },
                { "PartName", "Название детали" },

                // Заказы и логистика
                { "OrderID", "№ Заказа" },
                { "OrderDate", "Дата и время заказа" },
                { "AddedDate", "Дата и время добавления" },
                { "TotalAmount", "Сумма (₽)" },
                { "Status", "Статус" },
                { "CartID", "ID Корзины" },
                { "OrderItemID", "ID Позиции" }
            };

            foreach (DataGridViewColumn col in dgvAdmin.Columns)
            {
                if (translations.ContainsKey(col.Name))
                {
                    col.HeaderText = translations[col.Name];
                }
            }
        }

        #endregion

        #region Обработка событий

        /// <summary>
        /// Сохранение всех изменений, внесенных в таблицу, обратно в базу данных.
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (adapter != null && ds != null && cbTables.SelectedItem != null)
                {
                    dgvAdmin.EndEdit(); // Завершаем редактирование текущей ячейки
                    adapter.Update(ds, cbTables.SelectedItem.ToString());
                    MessageBox.Show("База данных успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изменений: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Удаление выделенной строки из таблицы с последующим обновлением БД.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAdmin.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?", "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dgvAdmin.SelectedRows)
                    {
                        if (!row.IsNewRow) dgvAdmin.Rows.Remove(row);
                    }
                    // После удаления в интерфейсе автоматически вызываем сохранение в базу
                    btnSave_Click(null, null);
                }
            }
        }

        private void admin_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();

        #endregion
    }
}