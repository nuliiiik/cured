using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace cured
{
    public partial class admin : Form
    {
        private DataGridView dgvAdmin;
        private ComboBox cbTables;
        private Button btnSave, btnDelete, btnBack;
        private Label lblTitle;

        DataBase db = new DataBase();
        SqlDataAdapter adapter;
        DataSet ds;

        public admin()
        {
            InitializeComponent();
            CreateAdminInterface();
        }

        private void CreateAdminInterface()
        {
            this.Text = "Управление базой данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 700);
            this.BackColor = Color.White;

            // Заголовок в стиле DarkRed
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
            cbTables.Items.AddRange(new string[] { "Users", "Motorcycles", "Parts", "Orders", "OrderItems", "Cart" });

            cbTables.SelectedIndexChanged += (s, e) => {
                if (cbTables.SelectedItem != null)
                    LoadTable(cbTables.SelectedItem.ToString());
            };

            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(1040, 460),
                BorderStyle = BorderStyle.None
            };
            SetupGridStyle(dgvAdmin);

            // Кнопки в стиле приложения
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
            dgv.AllowUserToResizeRows = false;

            // Шапка таблицы
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            // Строки
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        private void LoadTable(string tableName)
        {
            try
            {
                string query = $"SELECT * FROM {tableName}";
                SqlConnection con = db.getConnection();
                adapter = new SqlDataAdapter(query, con);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                ds = new DataSet();
                adapter.Fill(ds, tableName);
                dgvAdmin.DataSource = ds.Tables[tableName];

                TranslateColumns();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
        }

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

                // Мотоциклы / Запчасти (унифицировано)
                { "MotorcycleID", "ID Мото" },
                { "PartID", "ID Детали" },
                { "Brand", "Марка" },
                { "Model", "Модель" },
                { "Year", "Год" },
                { "Price", "Цена (₽)" },
                { "Description", "Описание" },
                { "Quantity", "Остаток/Кол-во" }, // Stock заменен на Quantity для соответствия твоей базе
                { "PartName", "Название детали" },

                // Заказы
                { "OrderID", "№ Заказа" },
                { "OrderDate", "Дата и время заказа" },
                { "AddedDate", "Дата и время добавления" },// Твой запрос на перевод времени
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (adapter != null && ds != null && cbTables.SelectedItem != null)
                {
                    dgvAdmin.EndEdit();
                    adapter.Update(ds, cbTables.SelectedItem.ToString());
                    MessageBox.Show("База данных успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения: " + ex.Message); }
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
                }
            }
        }

        private void admin_FormClosed(object sender, FormClosedEventArgs e) => new main().Show();
    }
}