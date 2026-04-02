using System;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public partial class FormAddProduct : Form
    {
        // Основные элементы
        private ComboBox cbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Location = new Point(140, 20) };
        private TextBox txtBrand = new TextBox { Width = 220, Location = new Point(140, 60) };
        private TextBox txtModel = new TextBox { Width = 220, Location = new Point(140, 100) };
        private TextBox txtPrice = new TextBox { Width = 220, Location = new Point(140, 140) };
        private NumericUpDown numQty = new NumericUpDown { Width = 220, Location = new Point(140, 180), Maximum = 10000 };

        // Поле для пути к картинке (теперь редактируемое)
        private TextBox txtImagePath = new TextBox { Width = 220, Location = new Point(140, 220) };

        // Поле для года (нужно только мотоциклам)
        private NumericUpDown numYear = new NumericUpDown { Width = 220, Location = new Point(140, 260), Minimum = 1900, Maximum = 2100, Value = DateTime.Now.Year };

        private TextBox txtDesc = new TextBox { Width = 220, Location = new Point(140, 300), Multiline = true, Height = 60 };

        // Метки (Labels), которые будем менять
        private Label lblModel = new Label { Location = new Point(20, 100), AutoSize = true };
        private Label lblYear = new Label { Text = "Год выпуска*", Location = new Point(20, 260), AutoSize = true };

        public FormAddProduct()
        {
            this.Text = "Добавление товара";
            this.Size = new Size(400, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Статичные метки
            string[] staticLabels = { "Тип товара*", "Марка/Бренд*", "Цена (₽)*", "Количество*", "Путь к фото", "Описание" };
            Point[] locations = { new Point(20, 20), new Point(20, 60), new Point(20, 140), new Point(20, 180), new Point(20, 220), new Point(20, 300) };

            for (int i = 0; i < staticLabels.Length; i++)
            {
                this.Controls.Add(new Label { Text = staticLabels[i], Location = locations[i], AutoSize = true, Font = new Font("Segoe UI", 9) });
            }

            // Добавляем динамические метки
            this.Controls.Add(lblModel);
            this.Controls.Add(lblYear);

            // Настройка ComboBox
            cbType.Items.AddRange(new string[] { "Мотоцикл", "Запчасть" });
            cbType.SelectedIndexChanged += CbType_SelectedIndexChanged; // Событие смены типа
            cbType.SelectedIndex = 0;

            // Кнопка
            Button btnAdd = new Button
            {
                Text = "СОХРАНИТЬ",
                DialogResult = DialogResult.OK,
                Location = new Point(140, 400),
                Size = new Size(120, 40),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { cbType, txtBrand, txtModel, txtPrice, numQty, txtImagePath, numYear, txtDesc, btnAdd });
        }

        private void CbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbType.SelectedIndex == 0) // МОТОЦИКЛ
            {
                lblModel.Text = "Модель*";
                lblYear.Visible = true;
                numYear.Visible = true;
                txtImagePath.Text = "/images/motocycles/photo.jpg"; // Путь по умолчанию
            }
            else // ЗАПЧАСТЬ
            {
                lblModel.Text = "Назв. детали*";
                lblYear.Visible = false;
                numYear.Visible = false;
                txtImagePath.Text = "/images/parts/photo.jpg"; // Путь по умолчанию
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtBrand.Text) || string.IsNullOrWhiteSpace(txtModel.Text) ||
                !decimal.TryParse(txtPrice.Text.Replace(".", ","), out decimal priceVal))
            {
                MessageBox.Show("Заполните Марку, Модель/Название и Цену!", "Внимание");
                this.DialogResult = DialogResult.None;
                return;
            }

            DataBase db = new DataBase();
            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();
            string price = priceVal.ToString().Replace(",", ".");
            string imagePath = txtImagePath.Text.Trim();
            int qty = (int)numQty.Value;
            string desc = txtDesc.Text.Trim();

            try
            {
                if (cbType.SelectedIndex == 0) // МОТОЦИКЛ
                {
                    int year = (int)numYear.Value;
                    db.ExecuteNonQuery($@"INSERT INTO Motorcycles (Brand, Model, Price, Quantity, Description, ImageURL, Year) 
                                         VALUES (N'{brand}', N'{model}', {price}, {qty}, N'{desc}', '{imagePath}', {year})");
                }
                else // ЗАПЧАСТЬ
                {
                    db.ExecuteNonQuery($@"INSERT INTO Parts (Brand, PartName, Price, Quantity, Description, ImageURL) 
                                         VALUES (N'{brand}', N'{model}', {price}, {qty}, N'{desc}', '{imagePath}')");
                }
                MessageBox.Show("Успешно добавлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка БД: " + ex.Message);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}