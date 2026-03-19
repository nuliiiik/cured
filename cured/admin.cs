using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace cured
{
    public partial class admin : Form
    {
        // Элементы управления
        private DataGridView dgvAdmin;
        private ComboBox cbTables;
        private Button btnSave, btnDelete, btnBack;
        private Label lblTitle;

        // Объекты для работы с БД
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
            this.Text = "Панель администратора";
            this.StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "Панель администратора",
                Location = new Point(20, 20),
                AutoSize = true
            };

            cbTables = new ComboBox
            {
                Location = new Point(20, 60),
                Width = 200
            };
            cbTables.Items.AddRange(new string[] { "Users", "Motorcycles", "Parts", "Orders", "OrderItems", "Cart" });
            // Подписываемся на событие смены таблицы
            cbTables.SelectedIndexChanged += (s, e) => {
                if (cbTables.SelectedItem != null)
                    LoadTable(cbTables.SelectedItem.ToString());
            };

            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(1040, 480),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnSave = new Button
            {
                Text = "Сохранить изменения",
                Location = new Point(20, 600),
                Size = new Size(180, 40)
            };
            btnSave.Click += btnSave_Click; // Теперь метод существует

            btnDelete = new Button
            {
                Text = "Удалить выбранное",
                Location = new Point(210, 600),
                Size = new Size(180, 40)
            };
            btnDelete.Click += btnDelete_Click; // Теперь метод существует

            btnBack = new Button
            {
                Text = "Вернуться на главную",
                Location = new Point(880, 600),
                Size = new Size(180, 40),
            };
            btnBack.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(cbTables);
            this.Controls.Add(dgvAdmin);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnBack);
        }

        private void admin_Load(object sender, EventArgs e)
        {
            if (cbTables.Items.Count > 0) cbTables.SelectedIndex = 0;
        }

        private void LoadTable(string tableName)
        {
            try
            {
                string query = $"SELECT * FROM {tableName}";
                // ВАЖНО: Мы вызываем getConnection(), так как поле connectionString обычно private
                SqlConnection con = db.getConnection();
                adapter = new SqlDataAdapter(query, con);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                ds = new DataSet();
                adapter.Fill(ds, tableName);
                dgvAdmin.DataSource = ds.Tables[tableName];
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
        }

        // ОТДЕЛЬНЫЙ МЕТОД ДЛЯ СОХРАНЕНИЯ
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (adapter != null && ds != null && cbTables.SelectedItem != null)
                {
                    dgvAdmin.EndEdit(); // Завершаем редактирование ячейки
                    adapter.Update(ds, cbTables.SelectedItem.ToString());
                    MessageBox.Show("Данные сохранены!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения: " + ex.Message); }
        }

        // ОТДЕЛЬНЫЙ МЕТОД ДЛЯ УДАЛЕНИЯ
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAdmin.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvAdmin.SelectedRows)
                {
                    if (!row.IsNewRow) dgvAdmin.Rows.Remove(row);
                }
                btnSave_Click(null, null); // Сразу применяем изменения
            }
        }

        private void admin_FormClosed(object sender, FormClosedEventArgs e)
        {
            new main().Show();
        }
    }
}