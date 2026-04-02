using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cured
{
    /// <summary>
    /// ГЛАВНОЕ ОКНО ПРИЛОЖЕНИЯ
    /// Управляет витриной товаров (мотоциклы и запчасти), фильтрацией, 
    /// поиском по цене и навигацией между разделами магазина.
    /// </summary>
    public partial class main : Form
    {
        #region 1. ПОЛЯ И ОБЪЕКТЫ

        // Объект для работы с базой данных
        private DataBase db = new DataBase();

        // Массивы для управления элементами на 3-х вкладках (0 - Главная, 1 - Мотоциклы, 2 - Запчасти)
        // Это позволяет использовать единый индекс для доступа к контролам каждой вкладки
        private TableLayoutPanel[] tables = new TableLayoutPanel[3];
        private ComboBox[] sortCombos = new ComboBox[3];
        private TextBox[] priceFromBoxes = new TextBox[3];
        private TextBox[] priceToBoxes = new TextBox[3];

        private ComboBox filterBrandMoto;         // Выпадающий список брендов для мотоциклов
        private bool isDetailWindowOpen = false;  // Флаг для предотвращения многократного открытия карточки

        #endregion

        #region 2. КОНСТРУКТОР И НАСТРОЙКА ИНТЕРФЕЙСА

        public main()
        {
            InitializeComponent();
            ConfigureWindow(); // Применение визуальных настроек окна
        }

        /// <summary>
        /// Базовая настройка геометрии окна и поведения вкладок.
        /// </summary>
        private void ConfigureWindow()
        {
            this.Size = new Size(1750, 735);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;

            // Скрываем стандартные корешки вкладок для создания кастомного меню
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        /// <summary>
        /// Событие загрузки формы: инициализация всех компонентов и данных.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Отображаем статус авторизации (имя пользователя или "Войти")
            UpdateProfileLabel();

            // --- ПРИВЯЗКА СОБЫТИЙ НАВИГАЦИИ (ВЕРХНЕЕ МЕНЮ) ---
            label5.Click += (s, ev) => HandleProfileClick();
            label1.Click += (s, ev) => tabControl1.SelectedTab = tabMain;
            label2.Click += (s, ev) => tabControl1.SelectedTab = tabMotocycles;
            label3.Click += (s, ev) => tabControl1.SelectedTab = tabParts;
            label4.Click += (s, ev) => tabControl1.SelectedTab = tabContacts;

            // --- ПОШАГОВАЯ ИНИЦИАЛИЗАЦИЯ ВКЛАДОК ---
            InitMainTab();
            InitMotoTab();
            InitPartsTab();
            InitContactsTab();

            // Заполнение фильтров и первичная отрисовка товаров
            FillBrands();
            RefreshAllTabs();

            // Автоматическое обновление данных при переключении между вкладками
            tabControl1.SelectedIndexChanged += (s, ev) => {
                int idx = tabControl1.SelectedIndex;
                if (idx >= 0 && idx <= 2) LoadData(idx);
            };
        }

        #endregion

        #region 3. ИНИЦИАЛИЗАЦИЯ ДИНАМИЧЕСКИХ ЭЛЕМЕНТОВ (UI)

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

        private void InitContactsTab()
        {
            tabContacts.Controls.Clear();
            tabContacts.Controls.Add(new Label
            {
                Text = "НАШИ КОНТАКТЫ:\n\n" +
                       "• Телефон: +7 (994) 088-35-45\n" +
                       "• Адрес: г. Барнаул, ул. 80 Гвардейской дивизии, д. 41\n" +
                       "• Режим работы: ПН-ВС с 09:00 до 17:00\n\n" +
                       "• О МАГАЗИНЕ\n" +
                       "Мы предлагаем широкий ассортимент мототехники и запчастей с гарантией качества.",
                Location = new Point(50, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 14)
            });
        }

        #endregion

        #region 4. РАБОТА С ДАННЫМИ (SQL ЛОГИКА)

        /// <summary>
        /// Основной метод загрузки товаров из БД в сетку TableLayoutPanel.
        /// </summary>
        /// <param name="type">Индекс вкладки (0-Все, 1-Мото, 2-Запчасти)</param>
        private void LoadData(int type)
        {
            if (tables[type] == null) return;

            // Очищаем текущую сетку и приостанавливаем отрисовку для скорости
            tables[type].SuspendLayout();
            tables[type].Controls.Clear();

            // Обработка фильтров цены
            decimal pf = 0, pt = 99999999;
            decimal.TryParse(priceFromBoxes[type]?.Text, out pf);
            if (!decimal.TryParse(priceToBoxes[type]?.Text, out pt)) pt = 99999999;

            string orderSql = GetOrderSql(type);
            string query = "";

            if (type == 1) // Вкладка: Мотоциклы
            {
                string brandFilter = (filterBrandMoto?.SelectedIndex > 0) ? $" AND Brand = '{filterBrandMoto.SelectedItem}'" : "";
                query = $@"SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description, Quantity as Stock 
                           FROM Motorcycles WHERE Quantity > 0 AND Price >= {pf} AND Price <= {pt} {brandFilter} {orderSql}";
            }
            else if (type == 2) // Вкладка: Запчасти
            {
                query = $@"SELECT PartID as ID, PartName as Name, Price, ImageURL, 'part' as T, Brand as B, 0 as Year, Description, Quantity as Stock 
                           FROM Parts WHERE Quantity > 0 AND Price >= {pf} AND Price <= {pt} {orderSql}";
            }
            else // Вкладка: Главная (Объединение таблиц)
            {
                query = $@"SELECT * FROM (
                            SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description, Quantity as Stock FROM Motorcycles
                            UNION ALL 
                            SELECT PartID, PartName, Price, ImageURL, 'part', Brand, 0, Description, Quantity FROM Parts
                          ) as Combined WHERE Stock > 0 AND Price >= {pf} AND Price <= {pt} {orderSql.Replace("Year", "ID")}";
            }

            try
            {
                DataTable dt = db.ExecuteQuery(query);
                foreach (DataRow r in dt.Rows)
                {
                    tables[type].Controls.Add(CreateCard(r, type));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            tables[type].ResumeLayout();
        }

        /// <summary>
        /// Генерирует SQL-строку сортировки на основе выбора в ComboBox.
        /// </summary>
        private string GetOrderSql(int type)
        {
            int sortIdx = sortCombos[type]?.SelectedIndex ?? 2;
            if (sortIdx == 0) return " ORDER BY Price ASC";
            if (sortIdx == 1) return " ORDER BY Price DESC";
            return (type == 2) ? " ORDER BY PartID DESC" : " ORDER BY Year DESC";
        }

        #endregion

        #region 5. ГЕНЕРАЦИЯ ВИЗУАЛЬНЫХ КАРТОЧЕК

        /// <summary>
        /// Создает экземпляр пользовательского контрола ProductCard и наполняет его данными.
        /// </summary>
        private ProductCard CreateCard(DataRow r, int type)
        {
            int id = Convert.ToInt32(r["ID"]);
            string t = r["T"].ToString();
            ProductCard card = new ProductCard(id, t, Convert.ToInt32(r["Stock"]));

            card.labelName.Text = r["Name"].ToString();
            card.labelPrice.Text = $"{Convert.ToDecimal(r["Price"]):N0} ₽";
            string brandValue = r["B"].ToString();

            // Формирование текста подзаголовка
            if (t == "moto") card.labelDetails.Text = $"{brandValue}, {r["Year"]} г.";
            else card.labelDetails.Text = $"Бренд: {brandValue}";

            // Загрузка изображения с диска
            LoadCardImage(card, r["ImageURL"].ToString(), brandValue);

            // Логика клика: открытие окна детальной информации
            EventHandler openDetails = (s, e) => {
                if (isDetailWindowOpen) return;
                isDetailWindowOpen = true;

                using (ProductDetails pd = new ProductDetails(card.labelArticle.Text, card.labelName.Text,
                       card.labelPrice.Text, card.labelDetails.Text, r["Description"].ToString(),
                       card.pictureBox.Image, id, t))
                {
                    pd.ShowDialog();
                    RefreshAllTabs(); // Обновляем данные (вдруг товар купили и количество изменилось)
                }
                isDetailWindowOpen = false;
            };

            card.pictureBox.Click += openDetails;
            card.labelName.Click += openDetails;
            return card;
        }

        #endregion

        #region 6. ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (HELPERS)

        private Panel CreateTopPanel(TabPage tab) => new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Gainsboro };

        private void CreateLabel(Panel p, string text, int x) =>
            p.Controls.Add(new Label { Text = text, Location = new Point(x, 22), AutoSize = true, Font = new Font("Segoe UI", 9) });

        private ComboBox CreateSortCombo(Panel p, int idx)
        {
            ComboBox cb = new ComboBox { Location = new Point(110, 18), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cb.Items.AddRange(new string[] { "Цена (возр.)", "Цена (убыв.)", "Новинки" });
            cb.SelectedIndex = 2;
            cb.SelectedIndexChanged += (s, e) => LoadData(idx);
            p.Controls.Add(cb);
            return cb;
        }

        private void AddPriceFilters(Panel pnl, int idx, int startX)
        {
            priceFromBoxes[idx] = new TextBox { Location = new Point(startX + 60, 18), Size = new Size(80, 25) };
            priceToBoxes[idx] = new TextBox { Location = new Point(startX + 180, 18), Size = new Size(80, 25) };
            Button btn = new Button { Text = "ОК", Location = new Point(startX + 270, 16), Size = new Size(40, 30), Cursor = Cursors.Hand };
            btn.Click += (s, e) => LoadData(idx);
            CreateLabel(pnl, "Цена от:", startX);
            CreateLabel(pnl, "до:", startX + 150);
            pnl.Controls.AddRange(new Control[] { priceFromBoxes[idx], priceToBoxes[idx], btn });
        }

        private void SetupTable(TabPage tab, int idx, Panel pnl)
        {
            Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            tables[idx] = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 5, GrowStyle = TableLayoutPanelGrowStyle.AddRows };
            for (int i = 0; i < 5; i++) tables[idx].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            scroll.Controls.Add(tables[idx]);
            tab.Controls.AddRange(new Control[] { scroll, pnl });
        }

        private void FillBrands()
        {
            try
            {
                DataTable dt = db.ExecuteQuery("SELECT DISTINCT Brand FROM Motorcycles");
                filterBrandMoto.Items.Clear();
                filterBrandMoto.Items.Add("Все марки");
                foreach (DataRow r in dt.Rows) filterBrandMoto.Items.Add(r["Brand"].ToString());
                filterBrandMoto.SelectedIndex = 0;
            }
            catch { /* Игнорируем ошибки при пустой базе */ }
        }

        /// <summary>
        /// Создает временную картинку-заглушку, если фото товара не найдено.
        /// </summary>
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

        /// <summary>
        /// Безопасная загрузка изображения из файловой системы.
        /// </summary>
        private void LoadCardImage(ProductCard card, string url, string brand)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) { card.pictureBox.Image = GetPlaceholder(brand); return; }

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

        #endregion

        #region 7. ЛОГИКА НАВИГАЦИИ И СЕССИИ

        /// <summary>
        /// ОБРАБОТЧИК: Нажатие на иконку профиля (pictureBox1)
        /// </summary>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Проверяем, авторизован ли пользователь (глобальный класс user)
            if (user.id_user > 0)
            {
                // Если залогинен — открываем профиль
                profile p = new profile();
                p.Show();
                this.Hide(); // Скрываем главную форму
            }
            else
            {
                // Если не залогинен — отправляем на форму входа
                login l = new login();
                l.Show();
                this.Hide();
            }
        }

        /// <summary>
        /// Управляет поведением при клике на имя пользователя/кнопку входа.
        /// </summary>
        private void HandleProfileClick()
        {
            if (!acc_checked.acc_check)
            {
                new login().Show();
                this.Hide();
            }
            else if (user.role == "admin")
            {
                new admin().Show();
                this.Hide();
            }
            else
            {
                new profile().Show();
                this.Hide();
            }
        }

        /// <summary>
        /// Обновляет текстовую метку профиля в зависимости от статуса входа.
        /// </summary>
        private void UpdateProfileLabel()
        {
            label5.Text = acc_checked.acc_check ? (user.full_name ?? user.login_user) : "Войти";
        }

        /// <summary>
        /// Принудительное обновление всех вкладок с товарами.
        /// </summary>
        private void RefreshAllTabs() { for (int i = 0; i < 3; i++) LoadData(i); }

        /// <summary>
        /// Корректное завершение работы приложения при закрытии главной формы.
        /// </summary>
        private void main_FormClosed(object sender, FormClosedEventArgs e) => Application.Exit();

        #endregion
    }
}