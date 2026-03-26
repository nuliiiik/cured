using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace cured
{
    /// <summary>
    /// Главное окно приложения. Реализует витрину товаров, 
    /// систему фильтрации, сортировки и навигацию по категориям.
    /// </summary>
    public partial class main : Form
    {
        #region Поля и Переменные

        DataBase db = new DataBase();

        // Массив таблиц для каждой вкладки (0 - Главная, 1 - Мотоциклы, 2 - Запчасти)
        TableLayoutPanel[] tables = new TableLayoutPanel[3];

        // Элементы фильтрации и сортировки
        ComboBox sortMain, sortMoto, sortParts, filterBrand;
        TextBox priceFrom, priceTo;

        // Флаг для предотвращения открытия нескольких окон одного товара
        private bool isDetailWindowOpen = false;

        #endregion

        #region Конструктор и Загрузка

        public main()
        {
            InitializeComponent();
            ConfigureWindow();
        }

        /// <summary>
        /// Первичная настройка внешнего вида окна.
        /// </summary>
        private void ConfigureWindow()
        {
            this.Size = new Size(1750, 735);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;

            // Скрываем стандартные заголовки вкладок для кастомного меню
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Настройка отображения профиля (Логин или Имя)
            if (acc_checked.acc_check)
                label5.Text = !string.IsNullOrEmpty(user.full_name) ? user.full_name : user.login_user;
            else
                label5.Text = "Войти";

            // Привязка событий навигации
            label5.Click += Label5_Click;
            label1.Click += (s, ev) => tabControl1.SelectedTab = tabMain;
            label2.Click += (s, ev) => tabControl1.SelectedTab = tabMotocycles;
            label3.Click += (s, ev) => tabControl1.SelectedTab = tabParts;
            label4.Click += (s, ev) => tabControl1.SelectedTab = tabContacts;

            // Инициализация вкладок
            InitMainTab();
            InitMotoTab();
            InitPartsTab();
            InitContactsTab();
            FillBrands();

            // Первичная загрузка данных во все категории
            LoadData(0); // Главная
            LoadData(1); // Мотоциклы
            LoadData(2); // Запчасти
        }

        #endregion

        #region Инициализация Интерфейса (UI)

        private void InitMainTab()
        {
            Panel pnl = CreateTopPanel(tabMain);
            CreateLabel(pnl, "Сортировка:", 20);
            sortMain = CreateSortCombo(pnl, new string[] { "Цена (возр.)", "Цена (убыв.)", "Новинки" }, 0);
            sortMain.SelectedIndex = 2;
            SetupTable(tabMain, 0, pnl);
        }

        private void InitMotoTab()
        {
            Panel pnl = CreateTopPanel(tabMotocycles);

            // Фильтр по бренду
            CreateLabel(pnl, "Марка:", 20);
            filterBrand = new ComboBox { Location = new Point(80, 18), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            filterBrand.SelectedIndexChanged += (s, e) => LoadData(1);
            pnl.Controls.Add(filterBrand);

            // Фильтр по цене
            CreateLabel(pnl, "Цена от:", 230);
            priceFrom = new TextBox { Location = new Point(290, 18), Size = new Size(70, 25) };
            CreateLabel(pnl, "до:", 370);
            priceTo = new TextBox { Location = new Point(400, 18), Size = new Size(70, 25) };

            Button btnApply = new Button { Text = "ОК", Location = new Point(480, 16), Size = new Size(40, 30), Cursor = Cursors.Hand };
            btnApply.Click += (s, e) => LoadData(1);

            pnl.Controls.Add(priceFrom); pnl.Controls.Add(priceTo); pnl.Controls.Add(btnApply);

            // Сортировка для мотоциклов
            CreateLabel(pnl, "Сортировать:", 540);
            sortMoto = CreateSortCombo(pnl, new string[] { "Цена (возр.)", "Цена (убыв.)", "Новинки" }, 1);
            sortMoto.Left = 640;
            sortMoto.SelectedIndex = 2;

            SetupTable(tabMotocycles, 1, pnl);
        }

        private void InitPartsTab()
        {
            Panel pnl = CreateTopPanel(tabParts);
            CreateLabel(pnl, "Сортировка:", 20);
            sortParts = CreateSortCombo(pnl, new string[] { "Цена (возр.)", "Цена (убыв.)", "Новинки" }, 2);
            sortParts.SelectedIndex = 2;
            SetupTable(tabParts, 2, pnl);
        }

        private void InitContactsTab()
        {
            tabContacts.Controls.Clear();
            Label info = new Label
            {
                Text = "НАШИ КОНТАКТЫ:\n\n" +
                       "• Телефон: +7 (994) 088-35-45\n" +
                       "• Адрес: г. Барнаул, ул. 80 Гвардейской дивизии, д. 41\n" +
                       "• Режим работы: ПН-ВС с 09:00 до 17:00\n\n" +
                       "• О нас\n" +
                       "Наш Магазин мототехники специализируется на продаже мотоциклов, оригинальных запчастей с 2026 года.\n" +
                       "За это время мы накопили бесценный опыт в сфере розничной торговли мототехникой и профессиональном подборе комплектующих,\n" +
                       "обеспечивая полный цикл обслуживания — от выбора «железного коня» до его бесперебойной эксплуатации." +
                       "\r\n\r\nВсе представленные мотоциклы и запасные части проходят строгую проверку. Безопасность клиентов для нас всегда стоит на первом месте.",
                Location = new Point(50, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.DarkSlateGray
            };
            tabContacts.Controls.Add(info);
        }

        #endregion

        #region Логика обработки данных и SQL

        /// <summary>
        /// Универсальный метод загрузки данных. 
        /// Генерирует SQL-запрос в зависимости от типа вкладки и выбранных фильтров.
        /// </summary>
        private void LoadData(int type)
        {
            TableLayoutPanel table = tables[type];
            if (table == null) return;

            table.SuspendLayout();
            table.Controls.Clear();

            string query = "";
            string order = "";

            if (type == 1) // Вкладка Мотоциклы
            {
                query = "SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description, Quantity as Stock FROM Motorcycles WHERE Quantity > 0";

                if (filterBrand != null && filterBrand.SelectedIndex > 0)
                    query += $" AND Brand = '{filterBrand.SelectedItem}'";

                if (decimal.TryParse(priceFrom.Text, out decimal pf)) query += $" AND Price >= {pf}";
                if (decimal.TryParse(priceTo.Text, out decimal pt)) query += $" AND Price <= {pt}";

                order = sortMoto.SelectedIndex == 0 ? " ORDER BY Price ASC" : sortMoto.SelectedIndex == 1 ? " ORDER BY Price DESC" : " ORDER BY Year DESC";
            }
            else if (type == 2) // Вкладка Запчасти
            {
                query = "SELECT PartID as ID, PartName as Name, Price, ImageURL, 'part' as T, Brand as B, 0 as Year, Description, Quantity as Stock FROM Parts WHERE Quantity > 0";
                order = sortParts.SelectedIndex == 0 ? " ORDER BY Price ASC" : sortParts.SelectedIndex == 1 ? " ORDER BY Price DESC" : " ORDER BY PartID DESC";
            }
            else // Главная (Объединение всех товаров через UNION ALL)
            {
                query = @"SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description, Quantity as Stock FROM Motorcycles 
                          UNION ALL 
                          SELECT PartID, PartName, Price, ImageURL, 'part', Brand, 0, Description, Quantity FROM Parts";
                order = sortMain.SelectedIndex == 0 ? " ORDER BY Price ASC" : sortMain.SelectedIndex == 1 ? " ORDER BY Price DESC" : " ORDER BY ID DESC";
            }

            try
            {
                DataTable dt = db.ExecuteQuery(query + order);
                foreach (DataRow r in dt.Rows)
                {
                    int currentId = Convert.ToInt32(r["ID"]);
                    string currentType = r["T"].ToString();
                    int currentStock = Convert.ToInt32(r["Stock"]);

                    // Создаем карточку товара
                    ProductCard card = new ProductCard(currentId, currentType, currentStock);
                    card.labelName.Text = r["Name"].ToString();
                    card.labelPrice.Text = string.Format("{0:N0} ₽", Convert.ToDecimal(r["Price"]));
                    card.labelDetails.Text = r["B"].ToString() + (currentType == "moto" ? ", " + r["Year"] + " г." : "");

                    string d_desc = r["Description"].ToString();
                    string d_info = card.labelDetails.Text;

                    // Попытка загрузки изображения
                    string imagePathFromDB = r["ImageURL"].ToString();

                    // 1. Убираем лишние слэши и нормализуем путь (из БД может прийти /images/...)
                    string cleanPath = imagePathFromDB.TrimStart('/').Replace('/', '\\');

                    string solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));

                    // 3. Формируем итоговый путь к картинке
                    string finalPath = Path.Combine(solutionRoot, cleanPath);

                    if (File.Exists(finalPath))
                    {
                        try
                        {
                            // Читаем байты, чтобы не блокировать файл
                            using (var ms = new MemoryStream(File.ReadAllBytes(finalPath)))
                            {
                                card.pictureBox.Image = Image.FromStream(ms);
                            }
                        }
                        catch
                        {
                            card.pictureBox.Image = GetPlaceholder("Ошибка чтения");
                        }
                    }
                    else
                    {
                        // Если файл не найден, выводим заглушку с текстом бренда
                        card.pictureBox.Image = GetPlaceholder(r["B"].ToString());
                    }

                    // Логика открытия подробностей
                    EventHandler openDetails = (s, e) => {
                        if (isDetailWindowOpen) return;
                        isDetailWindowOpen = true;
                        using (ProductDetails pd = new ProductDetails(
                            card.labelArticle.Text, card.labelName.Text, card.labelPrice.Text,
                            d_info, d_desc, card.pictureBox.Image, currentId, currentType))
                        {
                            pd.ShowDialog();
                            LoadData(type); // Обновляем витрину после закрытия (на случай покупки)
                        }
                        isDetailWindowOpen = false;
                    };

                    card.pictureBox.Click += openDetails;
                    card.labelName.Click += openDetails;

                    table.Controls.Add(card);
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки данных: " + ex.Message); }
            table.ResumeLayout();
        }

        /// <summary>
        /// Динамическое заполнение списка брендов из базы данных.
        /// </summary>
        private void FillBrands()
        {
            if (filterBrand == null) return;
            filterBrand.Items.Clear();
            filterBrand.Items.Add("Все марки");
            try
            {
                DataTable dt = db.ExecuteQuery("SELECT DISTINCT Brand FROM Motorcycles");
                foreach (DataRow r in dt.Rows) filterBrand.Items.Add(r["Brand"].ToString());
                filterBrand.SelectedIndex = 0;
            }
            catch { }
        }

        #endregion

        #region Вспомогательные методы UI

        private Panel CreateTopPanel(TabPage tab) => new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Gainsboro };

        private void CreateLabel(Panel p, string text, int x) => p.Controls.Add(new Label { Text = text, Location = new Point(x, 22), AutoSize = true, Font = new Font("Segoe UI", 9) });

        private ComboBox CreateSortCombo(Panel p, string[] items, int idx)
        {
            ComboBox cb = new ComboBox { Location = new Point(110, 18), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cb.Items.AddRange(items);
            cb.SelectedIndexChanged += (s, e) => LoadData(idx);
            p.Controls.Add(cb);
            return cb;
        }

        private void SetupTable(TabPage tab, int idx, Panel pnl)
        {
            Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10), BackColor = Color.White };
            tables[idx] = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 5 };
            for (int i = 0; i < 5; i++) tables[idx].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            scroll.Controls.Add(tables[idx]);
            tab.Controls.Add(scroll);
            tab.Controls.Add(pnl);
        }

        private Bitmap GetPlaceholder(string txt)
        {
            Bitmap b = new Bitmap(200, 150);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(Color.Gainsboro);
                g.DrawString(txt, this.Font, Brushes.Gray, 10, 60);
            }
            return b;
        }

        #endregion

        #region Переходы и Закрытие

        private void Label5_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) new login().Show();
            else if (user.role == "admin") new admin().Show();
            else new profile().Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e) => Label5_Click(sender, e);

        private void main_FormClosed(object sender, FormClosedEventArgs e) => Application.Exit();

        #endregion
    }
}