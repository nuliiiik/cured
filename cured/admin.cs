using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq; // Обязательно для .ToList()

namespace cured
{
    public partial class admin : Form
    {
        #region Переменные и инициализация

        private DataGridView dgvAdmin;
        private ComboBox cbTables;
        private Button btnSave, btnDelete, btnBack;
        private Label lblTitle;

        DataBase db = new DataBase();
        SqlDataAdapter adapter;
        DataSet ds;

        // Словарь сопоставления (Английское имя в БД -> Русское имя для юзера)
        private Dictionary<string, string> tableMapping = new Dictionary<string, string>
        {
            { "Users", "Пользователи" },
            { "Motorcycles", "Мотоциклы" },
            { "Parts", "Запчасти" },
            { "Orders", "Заказы" },
            { "OrderItems", "Состав заказов" },
            { "Cart", "Корзина" }
        };

        public admin()
        {
            InitializeComponent();
            CreateAdminInterface();
        }

        #endregion

        #region Интерфейс

        private void CreateAdminInterface()
        {
            this.Text = "Управление базой данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 700);
            this.BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "ПАНЕЛЬ АДМИНИСТРАТОРА",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            cbTables = new ComboBox
            {
                Location = new Point(20, 65),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };

            // 1. ПОДГОТОВКА ДАННЫХ
            var displayList = tableMapping.ToList();
            displayList.Insert(0, new KeyValuePair<string, string>("", "Выберите таблицу..."));

            // 2. ПРИВЯЗКА ДАННЫХ
            cbTables.DataSource = displayList;
            cbTables.DisplayMember = "Value";
            cbTables.ValueMember = "Key";

            // 3. БЕЗОПАСНАЯ УСТАНОВКА ИНДЕКСА (Исправляет ArgumentOutOfRangeException)
            if (cbTables.Items.Count > 0)
            {
                cbTables.SelectedIndex = 0;
            }

            // 4. ПОДПИСКА НА СОБЫТИЕ (Только после инициализации данных)
            cbTables.SelectedIndexChanged += cbTables_SelectedIndexChanged;

            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(1040, 460),
                BorderStyle = BorderStyle.None
            };
            SetupGridStyle(dgvAdmin);

            btnSave = CreateStyledButton("СОХРАНИТЬ ИЗМЕНЕНИЯ", new Point(20, 590), Color.ForestGreen);
            btnSave.Click += btnSave_Click;

            btnDelete = CreateStyledButton("УДАЛИТЬ СТРОКУ", new Point(220, 590), Color.DarkRed);
            btnDelete.Click += btnDelete_Click;

            btnBack = CreateStyledButton("ВЕРНУТЬСЯ", new Point(880, 590), Color.Gray);
            btnBack.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(cbTables);
            this.Controls.Add(dgvAdmin);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnBack);
        }

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

        private void SetupGridStyle(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersHeight = 40;
            dgv.DataError += (s, e) => { e.ThrowException = false; };
        }

        #endregion

        #region Работа с данными

        private void cbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = cbTables.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(selectedTable))
            {
                LoadTable(selectedTable);
            }
            else
            {
                dgvAdmin.DataSource = null;
            }
        }

        private void LoadTable(string tableName)
        {
            try
            {
                // Используем SELECT *, чтобы избежать ошибок с именами столбцов
                string query = $"SELECT * FROM [{tableName}]";
                SqlConnection con = db.getConnection();

                adapter = new SqlDataAdapter(query, con);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                ds = new DataSet();
                adapter.Fill(ds, tableName);
                dgvAdmin.DataSource = ds.Tables[tableName];

                TranslateColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка SQL: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TranslateColumns()
        {
            // Словарь переводов для ВСЕХ возможных колонок из ваших таблиц
            Dictionary<string, string> translations = new Dictionary<string, string>
            {
                { "UserID", "ID" },
                { "Login", "Логин" },
                { "Password", "Пароль" },
                { "FullName", "ФИО" },
                { "Phone", "Телефон" },
                { "Role", "Роль" },
                { "MotorcycleID", "ID Мото" },
                { "PartID", "ID Детали" },
                { "Brand", "Марка" },
                { "Model", "Модель" },
                { "ForModels", "Для моделей" }, // Исправлено для таблицы Parts
                { "Year", "Год" },
                { "Price", "Цена (₽)" },
                { "Description", "Описание" },
                { "Quantity", "Кол-во" },
                { "PartName", "Название детали" },
                { "OrderID", "№ Заказа" },
                { "OrderDate", "Дата заказа" },
                { "TotalAmount", "Сумма (₽)" },
                { "Status", "Статус" },
                { "ImageURL", "Путь к фото" }
            };

            foreach (DataGridViewColumn col in dgvAdmin.Columns)
            {
                if (translations.ContainsKey(col.Name))
                    col.HeaderText = translations[col.Name];
            }
        }

        #endregion

        #region Обработка событий

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedTable = cbTables.SelectedValue?.ToString();
                if (adapter != null && ds != null && !string.IsNullOrEmpty(selectedTable))
                {
                    dgvAdmin.EndEdit();
                    adapter.Update(ds, selectedTable);
                    MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAdmin.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dgvAdmin.SelectedRows)
                    {
                        if (!row.IsNewRow) dgvAdmin.Rows.Remove(row);
                    }
                    btnSave_Click(null, null); // Сразу сохраняем в БД
                }
            }
        }

        private void dgvAdmin_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dgvAdmin.Columns[e.ColumnIndex].ValueType == typeof(int) || dgvAdmin.Columns[e.ColumnIndex].ValueType == typeof(decimal))
            {
                if (!string.IsNullOrEmpty(e.FormattedValue.ToString()) && !decimal.TryParse(e.FormattedValue.ToString(), out _))
                {
                    MessageBox.Show("Здесь должно быть число!");
                    e.Cancel = true;
                }
            }
        }

        private void admin_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();

        #endregion
    }
}