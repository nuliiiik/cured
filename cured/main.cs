using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace cured
{
    public partial class main : Form
    {
        #region ПОЛЯ И ПЕРЕМЕННЫЕ

        private DataBase db = new DataBase();
        private TableLayoutPanel[] tables = new TableLayoutPanel[3];

        // Массивы для управления элементами фильтрации
        private ComboBox[] sortCombos = new ComboBox[3];
        private ComboBox filterBrandMoto; // Специальный фильтр только для вкладки Мотоциклы
        private TextBox[] priceFromBoxes = new TextBox[3];
        private TextBox[] priceToBoxes = new TextBox[3];

        // Флаг для предотвращения множественного открытия окон деталей
        private bool isDetailWindowOpen = false;

        #endregion

        #region КОНСТРУКТОР И ЗАГРУЗКА ФОРМЫ

        public main()
        {
            InitializeComponent();
            ConfigureWindow();
        }

        /// <summary>
        /// Первичная настройка внешнего вида окна
        /// </summary>
        private void ConfigureWindow()
        {
            this.Size = new Size(1750, 735);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;

            // Настройка TabControl: скрываем заголовки вкладок для кастомной навигации
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateProfileLabel();

            // --- ПРИВЯЗКА НАВИГАЦИИ ---
            label5.Click += Label5_Click; // Профиль / Вход
            label1.Click += (s, ev) => tabControl1.SelectedTab = tabMain;
            label2.Click += (s, ev) => tabControl1.SelectedTab = tabMotocycles;
            label3.Click += (s, ev) => tabControl1.SelectedTab = tabParts;
            label4.Click += (s, ev) => tabControl1.SelectedTab = tabContacts;

            // --- ИНИЦИАЛИЗАЦИЯ ИНТЕРФЕЙСА ВКЛАДОК ---
            InitMainTab();
            InitMotoTab();
            InitPartsTab();
            InitContactsTab();

            FillBrands(); // Загрузка списка брендов из БД

            // Первичная загрузка данных во все таблицы
            RefreshAllTabs();

            // Автоматическое обновление данных при переключении между вкладками
            tabControl1.SelectedIndexChanged += (s, ev) => {
                int idx = tabControl1.SelectedIndex;
                if (idx >= 0 && idx <= 2) LoadData(idx);
            };
        }

        /// <summary>
        /// Обновляет текст кнопки профиля (ФИО пользователя или "Войти")
        /// </summary>
        private void UpdateProfileLabel()
        {
            if (acc_checked.acc_check)
                label5.Text = !string.IsNullOrEmpty(user.full_name) ? user.full_name : user.login_user;
            else
                label5.Text = "Войти";
        }

        #endregion

        #region ИНИЦИАЛИЗАЦИЯ ИНТЕРФЕЙСА (UI)

        private void InitMainTab()
        {
            Panel pnl = CreateTopPanel(tabMain);
            CreateLabel(pnl, "Сортировка:", 20);
            sortCombos[0] = CreateSortCombo(pnl, 0);

            AddPriceFilters(pnl, 0, 250);
            SetupTable(tabMain, 0, pnl);
        }

        private void InitMotoTab()
        {
            Panel pnl = CreateTopPanel(tabMotocycles);
            CreateLabel(pnl, "Сортировка:", 20);
            sortCombos[1] = CreateSortCombo(pnl, 1);

            CreateLabel(pnl, "Марка:", 250);
            filterBrandMoto = new ComboBox { Location = new Point(310, 18), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            filterBrandMoto.SelectedIndexChanged += (s, e) => LoadData(1);
            pnl.Controls.Add(filterBrandMoto);

            AddPriceFilters(pnl, 1, 460);
            SetupTable(tabMotocycles, 1, pnl);
        }

        private void InitPartsTab()
        {
            Panel pnl = CreateTopPanel(tabParts);
            CreateLabel(pnl, "Сортировка:", 20);
            sortCombos[2] = CreateSortCombo(pnl, 2);

            AddPriceFilters(pnl, 2, 250);
            SetupTable(tabParts, 2, pnl);
        }

        /// <summary>
        /// Создает текстовые поля для фильтрации по цене
        /// </summary>
        private void AddPriceFilters(Panel pnl, int idx, int startX)
        {
            CreateLabel(pnl, "Цена от:", startX);
            priceFromBoxes[idx] = new TextBox { Location = new Point(startX + 60, 18), Size = new Size(80, 25) };

            CreateLabel(pnl, "до:", startX + 150);
            priceToBoxes[idx] = new TextBox { Location = new Point(startX + 180, 18), Size = new Size(80, 25) };

            Button btn = new Button { Text = "ОК", Location = new Point(startX + 270, 16), Size = new Size(40, 30), Cursor = Cursors.Hand };
            btn.Click += (s, e) => LoadData(idx);

            pnl.Controls.Add(priceFromBoxes[idx]);
            pnl.Controls.Add(priceToBoxes[idx]);
            pnl.Controls.Add(btn);
        }

        #endregion

        #region РАБОТА С ДАННЫМИ (SQL & LOGIC)

        /// <summary>
        /// Основной метод загрузки данных из БД и создания карточек товаров
        /// </summary>
        private void LoadData(int type)
        {
            if (tables[type] == null) return;
            tables[type].SuspendLayout();
            tables[type].Controls.Clear();

            string filterSql = " WHERE Quantity > 0";

            // Фильтр по бренду (применяется только для вкладки Мотоциклы)
            if (type == 1 && filterBrandMoto?.SelectedIndex > 0)
                filterSql += $" AND Brand = '{filterBrandMoto.SelectedItem}'";

            // Парсинг цен для фильтрации
            if (decimal.TryParse(priceFromBoxes[type]?.Text, out decimal pf)) filterSql += $" AND Price >= {pf}";
            if (decimal.TryParse(priceToBoxes[type]?.Text, out decimal pt)) filterSql += $" AND Price <= {pt}";

            string orderSql = GetOrderSql(type);
            string query = "";

            if (type == 1) // Мотоциклы
                query = "SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description, Quantity as Stock FROM Motorcycles" + filterSql + orderSql;
            else if (type == 2) // Запчасти
                query = "SELECT PartID as ID, PartName as Name, Price, ImageURL, 'part' as T, Brand as B, 0 as Year, Description, Quantity as Stock FROM Parts" + filterSql + orderSql;
            else // Главная (Объединение таблиц)
                query = $@"SELECT * FROM (
                            SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description, Quantity as Stock FROM Motorcycles
                            UNION ALL 
                            SELECT PartID, PartName, Price, ImageURL, 'part', Brand, 0, Description, Quantity FROM Parts
                          ) as Combined" + filterSql.Replace("Quantity", "Stock") + orderSql.Replace("Year", "ID");

            try
            {
                DataTable dt = db.ExecuteQuery(query);
                foreach (DataRow r in dt.Rows)
                    tables[type].Controls.Add(CreateCard(r, type));
            }
            catch (Exception ex) { MessageBox.Show("Ошибка при получении данных: " + ex.Message); }

            tables[type].ResumeLayout();
        }

        private string GetOrderSql(int type)
        {
            int sortIdx = sortCombos[type]?.SelectedIndex ?? 2;
            if (sortIdx == 0) return " ORDER BY Price ASC";
            if (sortIdx == 1) return " ORDER BY Price DESC";
            return (type == 2) ? " ORDER BY PartID DESC" : " ORDER BY Year DESC";
        }

        /// <summary>
        /// Создание объекта карточки товара на основе строки из БД
        /// </summary>
        private ProductCard CreateCard(DataRow r, int type)
        {
            int id = Convert.ToInt32(r["ID"]);
            string t = r["T"].ToString();
            ProductCard card = new ProductCard(id, t, Convert.ToInt32(r["Stock"]));

            card.labelName.Text = r["Name"].ToString();
            card.labelPrice.Text = $"{Convert.ToDecimal(r["Price"]):N0} ₽";
            card.labelDetails.Text = r["B"].ToString() + (t == "moto" ? $", {r["Year"]} г." : "");

            LoadCardImage(card, r["ImageURL"].ToString(), r["B"].ToString());

            // Логика открытия окна подробностей
            EventHandler open = (s, e) => {
                if (isDetailWindowOpen) return;
                isDetailWindowOpen = true;
                using (ProductDetails pd = new ProductDetails(card.labelArticle.Text, card.labelName.Text, card.labelPrice.Text, card.labelDetails.Text, r["Description"].ToString(), card.pictureBox.Image, id, t))
                {
                    pd.ShowDialog();
                    RefreshAllTabs(); // Синхронизация количества во всех вкладках после закрытия окна
                }
                isDetailWindowOpen = false;
            };

            card.pictureBox.Click += open;
            card.labelName.Click += open;
            return card;
        }

        private void RefreshAllTabs()
        {
            for (int i = 0; i < 3; i++) LoadData(i);
        }

        #endregion

        #region ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (HELPERS)

        private void LoadCardImage(ProductCard card, string url, string brand)
        {
            try
            {
                string path = url.TrimStart('/').Replace('/', '\\');
                string fullPath = Path.Combine(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\")), path);

                if (File.Exists(fullPath))
                {
                    using (var ms = new MemoryStream(File.ReadAllBytes(fullPath)))
                        card.pictureBox.Image = Image.FromStream(ms);
                }
                else card.pictureBox.Image = GetPlaceholder(brand);
            }
            catch { card.pictureBox.Image = GetPlaceholder(brand); }
        }

        private void FillBrands()
        {
            DataTable dt = db.ExecuteQuery("SELECT DISTINCT Brand FROM Motorcycles");
            filterBrandMoto.Items.Clear();
            filterBrandMoto.Items.Add("Все марки");
            foreach (DataRow r in dt.Rows) filterBrandMoto.Items.Add(r["Brand"].ToString());
            filterBrandMoto.SelectedIndex = 0;
        }

        private ComboBox CreateSortCombo(Panel p, int idx)
        {
            ComboBox cb = new ComboBox { Location = new Point(110, 18), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cb.Items.AddRange(new string[] { "Цена (возр.)", "Цена (убыв.)", "Новинки" });
            cb.SelectedIndex = 2;
            cb.SelectedIndexChanged += (s, e) => LoadData(idx);
            p.Controls.Add(cb);
            return cb;
        }

        private Panel CreateTopPanel(TabPage tab) => new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Gainsboro };

        private void CreateLabel(Panel p, string text, int x) =>
            p.Controls.Add(new Label { Text = text, Location = new Point(x, 22), AutoSize = true, Font = new Font("Segoe UI", 9) });

        private void SetupTable(TabPage tab, int idx, Panel pnl)
        {
            Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
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

        private void InitContactsTab()
        {
            tabContacts.Controls.Clear();
            tabContacts.Controls.Add(new Label
            {
                Text = "НАШИ КОНТАКТЫ:\n\n" +
                       "• Телефон: +7 (994) 088-35-45\n" +
                       "• Адрес: г. Барнаул, ул. 80 Гвардейской дивизии, д. 41\n" +
                       "• Режим работы: ПН-ВС с 09:00 до 17:00\n\n" +
                       "• О НАС\n" +
                       "Наш магазин мототехники специализируется на продаже оригинальных мотоциклов и запчастей с 2026 года. " +
                       "За это время мы накопили бесценный опыт в сфере розничной торговли и профессионального подбора комплектующих.\n\n" +
                       "Все товары проходят строгую проверку качества. Безопасность наших клиентов — главный приоритет.",
                Location = new Point(50, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 14)
            });
        }

        #endregion

        #region СОБЫТИЯ И ПЕРЕХОДЫ МЕЖДУ ФОРМАМИ

        private void Label5_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) new login().Show();
            else if (user.role == "admin") new admin().Show();
            else new profile().Show();
            this.Hide();
        }

        // Клик по иконке профиля дублирует клик по надписи
        private void pictureBox1_Click(object sender, EventArgs e) => Label5_Click(sender, e);

        private void main_FormClosed(object sender, FormClosedEventArgs e) => Application.Exit();

        #endregion
    }
}