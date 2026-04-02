using System;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    // Обязательно partial, чтобы не было конфликтов с дизайнером
    public partial class FormAddProduct : Form
    {
        private ComboBox cbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Location = new Point(130, 20) };
        private TextBox txtBrand = new TextBox { Width = 200, Location = new Point(130, 60) }; // МАРКА (Обязательно)
        private TextBox txtModel = new TextBox { Width = 200, Location = new Point(130, 100) }; // МОДЕЛЬ (Не обязательно)
        private TextBox txtPrice = new TextBox { Width = 200, Location = new Point(130, 140) };
        private NumericUpDown numQty = new NumericUpDown { Width = 200, Location = new Point(130, 180), Maximum = 10000 };
        private TextBox txtDesc = new TextBox { Width = 200, Location = new Point(130, 220), Multiline = true, Height = 60 };

        public FormAddProduct()
        {
            this.Text = "Новый товар";
            this.Size = new Size(380, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            string[] labels = { "Тип товара*", "Марка (Бренд)*", "Модель", "Цена (₽)*", "Количество*", "Описание" };
            for (int i = 0; i < labels.Length; i++)
            {
                this.Controls.Add(new Label
                {
                    Text = labels[i],
                    Location = new Point(20, 20 + i * 40),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9)
                });
            }

            cbType.Items.AddRange(new string[] { "Мотоцикл", "Запчасть" });
            cbType.SelectedIndex = 0;

            Button btnAdd = new Button
            {
                Text = "ДОБАВИТЬ",
                DialogResult = DialogResult.OK,
                Location = new Point(130, 320),
                Size = new Size(120, 40),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            this.Controls.AddRange(new Control[] { cbType, txtBrand, txtModel, txtPrice, numQty, txtDesc, btnAdd });
        }
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtBrand.Text) || !decimal.TryParse(txtPrice.Text.Replace(".", ","), out decimal priceVal))
            {
                MessageBox.Show("Пожалуйста, укажите Марку и корректную Цену!", "Внимание");
                this.DialogResult = DialogResult.None;
                return;
            }

            DataBase db = new DataBase();
            string brand = txtBrand.Text.Trim();
            string model = string.IsNullOrWhiteSpace(txtModel.Text) ? "" : txtModel.Text.Trim();
            string price = priceVal.ToString().Replace(",", ".");
            int qty = (int)numQty.Value;
            string desc = txtDesc.Text.Trim();

            try
            {
                if (cbType.SelectedIndex == 0) // МОТОЦИКЛЫ
                {
                    // Устанавливаем путь по умолчанию для мотоциклов
                    string defaultImg = "/images/motocycles/";

                    db.ExecuteNonQuery($@"INSERT INTO Motorcycles (Brand, Model, Price, Quantity, Description, ImageURL, Year) 
                                 VALUES (N'{brand}', N'{model}', {price}, {qty}, N'{desc}', '{defaultImg}', {DateTime.Now.Year})");
                }
                else // ЗАПЧАСТИ
                {
                    // Устанавливаем путь по умолчанию для запчастей
                    string defaultImg = "/images/parts/";

                    db.ExecuteNonQuery($@"INSERT INTO Parts (Brand, PartName, Price, Quantity, Description, ImageURL) 
                                 VALUES (N'{brand}', N'{model}', {price}, {qty}, N'{desc}', '{defaultImg}')");
                }

                MessageBox.Show("Товар успешно добавлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}