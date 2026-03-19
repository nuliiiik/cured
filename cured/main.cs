using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cured
{
    public partial class main : Form
    {
        DataBase db = new DataBase();
        TableLayoutPanel[] tables = new TableLayoutPanel[3];
        ComboBox sortMain, sortMoto, sortParts, filterBrand;
        TextBox priceFrom, priceTo;
        private bool isDetailWindowOpen = false;

        public main()
        {
            InitializeComponent();
            this.Size = new Size(1750, 735);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (acc_checked.acc_check)
                label5.Text = !string.IsNullOrEmpty(user.full_name) ? user.full_name : user.login_user;
            else
                label5.Text = "Войти";

            label5.Click += Label5_Click;

            label1.Click += (s, ev) => tabControl1.SelectedTab = tabMain;
            label2.Click += (s, ev) => tabControl1.SelectedTab = tabMotocycles;
            label3.Click += (s, ev) => tabControl1.SelectedTab = tabParts;
            label4.Click += (s, ev) => tabControl1.SelectedTab = tabContacts;

            InitMainTab();
            InitMotoTab();
            InitPartsTab();
            InitContactsTab();
            FillBrands();

            LoadData(0); // Главная
            LoadData(1); // Мотоциклы
            LoadData(2); // Запчасти
        }

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

        #region Инициализация UI
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
            CreateLabel(pnl, "Марка:", 20);
            filterBrand = new ComboBox { Location = new Point(80, 18), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            filterBrand.SelectedIndexChanged += (s, e) => LoadData(1);
            pnl.Controls.Add(filterBrand);

            CreateLabel(pnl, "Цена от:", 230);
            priceFrom = new TextBox { Location = new Point(290, 18), Size = new Size(70, 25) };
            CreateLabel(pnl, "до:", 370);
            priceTo = new TextBox { Location = new Point(400, 18), Size = new Size(70, 25) };

            Button btnApply = new Button { Text = "ОК", Location = new Point(480, 16), Size = new Size(40, 30) };
            btnApply.Click += (s, e) => LoadData(1);

            pnl.Controls.Add(priceFrom); pnl.Controls.Add(priceTo); pnl.Controls.Add(btnApply);

            CreateLabel(pnl, "Сортировать:", 540);
            sortMoto = CreateSortCombo(pnl, new string[] { "Дешевле", "Дороже", "Новинки" }, 1);
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
                Text = "Наши контакты:\nТелефон: +7 (900) 000-00-00\nАдрес: г. Москва, ул. Мотоциклетная, д. 1",
                Location = new Point(50, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 14)
            };
            tabContacts.Controls.Add(info);
        }

        private Panel CreateTopPanel(TabPage tab) => new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Gainsboro };
        private void CreateLabel(Panel p, string text, int x) => p.Controls.Add(new Label { Text = text, Location = new Point(x, 22), AutoSize = true });
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
        #endregion

        private void LoadData(int type)
        {
            TableLayoutPanel table = tables[type];
            if (table == null) return;
            table.SuspendLayout();
            table.Controls.Clear();

            string query = "";
            string order = "";

            if (type == 1)
            {
                query = "SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description FROM Motorcycles WHERE Quantity > 0";
                if (filterBrand != null && filterBrand.SelectedIndex > 0) query += " AND Brand = '" + filterBrand.SelectedItem.ToString() + "'";
                if (decimal.TryParse(priceFrom.Text, out decimal pf)) query += " AND Price >= " + pf;
                if (decimal.TryParse(priceTo.Text, out decimal pt)) query += " AND Price <= " + pt;
                order = sortMoto.SelectedIndex == 0 ? " ORDER BY Price ASC" : sortMoto.SelectedIndex == 1 ? " ORDER BY Price DESC" : " ORDER BY Year DESC";
            }
            else if (type == 2)
            {
                query = "SELECT PartID as ID, PartName as Name, Price, ImageURL, 'part' as T, Brand as B, 0 as Year, 'Описание запчасти' as Description FROM Parts WHERE Quantity > 0";
                order = sortParts.SelectedIndex == 0 ? " ORDER BY Price ASC" : sortParts.SelectedIndex == 1 ? " ORDER BY Price DESC" : " ORDER BY PartID DESC";
            }
            else
            {
                query = "SELECT MotorcycleID as ID, Brand + ' ' + Model as Name, Price, ImageURL, 'moto' as T, Brand as B, Year, Description FROM Motorcycles UNION ALL SELECT PartID, PartName, Price, ImageURL, 'part', Brand, 0, 'Описание' FROM Parts";
                order = sortMain.SelectedIndex == 0 ? " ORDER BY Price ASC" : sortMain.SelectedIndex == 1 ? " ORDER BY Price DESC" : " ORDER BY ID DESC";
            }

            try
            {
                DataTable dt = db.ExecuteQuery(query + order);
                foreach (DataRow r in dt.Rows)
                {
                    ProductCard card = new ProductCard();
                    card.labelName.Text = r["Name"].ToString();
                    card.labelPrice.Text = string.Format("{0:N0} ₽", Convert.ToDecimal(r["Price"]));
                    card.labelArticle.Text = "Арт: " + r["ID"].ToString();
                    card.labelDetails.Text = r["B"].ToString() + (r["T"].ToString() == "moto" ? ", " + r["Year"] + " г." : "");

                    int currentId = Convert.ToInt32(r["ID"]);
                    string currentType = r["T"].ToString();
                    string d_desc = r["Description"].ToString();
                    string d_info = card.labelDetails.Text;

                    string path = Path.Combine(Application.StartupPath, r["ImageURL"].ToString().TrimStart('/').Replace('/', '\\'));
                    if (File.Exists(path)) card.pictureBox.Image = Image.FromFile(path);
                    else card.pictureBox.Image = GetPlaceholder(r["B"].ToString());

                    // ПРАВИЛЬНАЯ ЛОГИКА КЛИКА
                    card.Click += (s, e) => {
                        if (isDetailWindowOpen) return;
                        isDetailWindowOpen = true;

                        using (ProductDetails pd = new ProductDetails(
                            card.labelArticle.Text, card.labelName.Text, card.labelPrice.Text,
                            d_info, d_desc, card.pictureBox.Image, currentId, currentType))
                        {
                            pd.ShowDialog();
                        }
                        isDetailWindowOpen = false;
                    };

                    // Перенаправляем клики детей на родителя
                    // Находим этот блок в методе LoadData файла main.cs
                    foreach (Control child in card.Controls)
                    {
                        child.Cursor = Cursors.Hand;
                        child.Click += (s, e) => {
                            // Теперь мы вызываем созданный нами публичный метод
                            card.PerformCardClick();
                        };
                    }
                    card.Cursor = Cursors.Hand;

                    table.Controls.Add(card);
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            table.ResumeLayout();
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

        private void Label5_Click(object sender, EventArgs e)
        {
            if (!acc_checked.acc_check) new login().Show();
            else if (user.role == "admin") new admin().Show();
            else new profile().Show();
            this.Hide();
        }

        private void main_FormClosed(object sender, FormClosedEventArgs e) => Application.Exit();
    }
}