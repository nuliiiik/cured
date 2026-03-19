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
            CreateAdminInterface(); // Создаем дизайн через код
        }

        private void CreateAdminInterface()
        {
            // 1. Настройка формы
            this.Text = "Панель администратора";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48); // Темная тема

            // 2. Заголовок
            lblTitle = new Label
            {
                Text = "Управление базами данных",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // 3. Выбор таблицы
            cbTables = new ComboBox
            {
                Location = new Point(20, 60),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cbTables.Items.AddRange(new string[] { "Users", "Motorcycles", "Parts", "Orders", "OrderItems", "Cart" });
            cbTables.SelectedIndexChanged += (s, e) => LoadTable(cbTables.SelectedItem.ToString());

            // 4. Таблица (DataGridView)
            dgvAdmin = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(1040, 480),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.AliceBlue }
            };

            // 5. Кнопка Сохранить
            btnSave = new Button
            {
                Text = "Сохранить изменения",
                Location = new Point(20, 600),
                Size = new Size(180, 40),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += btnSave_Click;

            // 6. Кнопка Удалить
            btnDelete = new Button
            {
                Text = "Удалить выбранное",
                Location = new Point(210, 600),
                Size = new Size(180, 40),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDelete.Click += btnDelete_Click;

            // 7. Кнопка Назад
            btnBack = new Button
            {
                Text = "Вернуться на главную",
                Location = new Point(880, 600),
                Size = new Size(180, 40),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnBack.Click += (s, e) => this.Close();

            // Добавляем все на форму
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
                SqlConnection con = new SqlConnection(db.connectionString);
                adapter = new SqlDataAdapter(query, con);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                ds