using System;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    /// <summary>
    /// Форма для добавления новых позиций в базу данных.
    /// Поддерживает два режима: добавление мотоцикла или запчасти.
    /// </summary>
    public partial class FormAddProduct : Form
    {
        #region ЭЛЕМЕНТЫ УПРАВЛЕНИЯ

        // Выбор типа (Мотоцикл/Запчасть)
        private ComboBox cbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Location = new Point(140, 20) };

        // Текстовые поля для основных характеристик
        private TextBox txtBrand = new TextBox { Width = 220, Location = new Point(140, 60) };
        private TextBox txtModel = new TextBox { Width = 220, Location = new Point(140, 100) };
        private TextBox txtPrice = new TextBox { Width = 220, Location = new Point(140, 140) };

        // Выбор количества (от 0 до 10000)
        private NumericUpDown numQty = new NumericUpDown { Width = 220, Location = new Point(140, 180), Maximum = 10000 };

        // Поле для указания пути к изображению
        private TextBox txtImagePath = new TextBox { Width = 220, Location = new Point(140, 220) };

        // Выбор года выпуска (актуально только для мотоциклов)
        private NumericUpDown numYear = new NumericUpDown { Width = 220, Location = new Point(140, 260), Minimum = 1900, Maximum = 2100, Value = DateTime.Now.Year };

        // Поле описания товара (многострочное)
        private TextBox txtDesc = new TextBox { Width = 220, Location = new Point(140, 300), Multiline = true, Height = 60 };

        // Метки, текст которых будет меняться динамически
        private Label lblModel = new Label { Location = new Point(20, 100), AutoSize = true };
        private Label lblYear = new Label { Text = "Год выпуска*", Location = new Point(20, 260), AutoSize = true };

        #endregion

        #region КОНСТРУКТОР И НАСТРОЙКА ФОРМЫ

        public FormAddProduct()
        {
            // Базовые настройки окна
            this.Text = "Добавление товара";
            this.Size = new Size(400, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Генерация статических текстовых меток через цикл для экономии кода
            string[] staticLabels = { "Тип товара*", "Марка/Бренд*", "Цена (₽)*", "Количество*", "Путь к фото", "Описание" };
            Point[] locations = { new Point(20, 20), new Point(20, 60), new Point(20, 140), new Point(20, 180), new Point(20, 220), new Point(20, 300) };

            for (int i = 0; i < staticLabels.Length; i++)
            {
                this.Controls.Add(new Label { Text = staticLabels[i], Location = locations[i], AutoSize = true, Font = new Font("Segoe UI", 9) });
            }

            // Добавление динамических меток
            this.Controls.Add(lblModel);
            this.Controls.Add(lblYear);

            // Настройка списка типов
            cbType.Items.AddRange(new string[] { "Мотоцикл", "Запчасть" });
            cbType.SelectedIndexChanged += CbType_SelectedIndexChanged; // Привязка события переключения
            cbType.SelectedIndex = 0; // По умолчанию выбран Мотоцикл

            // Кнопка сохранения
            Button btnAdd = new Button
            {
                Text = "СОХРАНИТЬ",
                DialogResult = DialogResult.OK, // Форма закроется с результатом OK при успешном нажатии
                Location = new Point(140, 400),
                Size = new Size(120, 40),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAdd.Click += BtnAdd_Click;

            // Вывод всех созданных элементов на форму
            this.Controls.AddRange(new Control[] { cbType, txtBrand, txtModel, txtPrice, numQty, txtImagePath, numYear, txtDesc, btnAdd });
        }

        #endregion

        #region ЛОГИКА ПЕРЕКЛЮЧЕНИЯ ТИПА ТОВАРА

        /// <summary>
        /// Изменяет интерфейс в зависимости от того, что добавляет пользователь.
        /// </summary>
        private void CbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbType.SelectedIndex == 0) // ВЫБРАН МОТОЦИКЛ
            {
                lblModel.Text = "Модель*";
                lblYear.Visible = true;
                numYear.Visible = true;
                txtImagePath.Text = "/images/motocycles/"; // Подсказка пути
            }
            else // ВЫБРАНА ЗАПЧАСТЬ
            {
                lblModel.Text = "Назв. детали*";
                lblYear.Visible = false;  // Скрываем выбор года
                numYear.Visible = false;
                txtImagePath.Text = "/images/parts/"; // Подсказка пути
            }
        }

        #endregion

        #region СОХРАНЕНИЕ В БАЗУ ДАННЫХ

        /// <summary>
        /// Обработка нажатия кнопки "Сохранить": валидация и SQL-запрос.
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // 1. Проверка на заполнение обязательных полей
            if (string.IsNullOrWhiteSpace(txtBrand.Text) || string.IsNullOrWhiteSpace(txtModel.Text) ||
                !decimal.TryParse(txtPrice.Text.Replace(".", ","), out decimal priceVal))
            {
                MessageBox.Show("Заполните Марку, Модель/Название и Цену!", "Внимание");
                this.DialogResult = DialogResult.None; // Не закрываем форму при ошибке
                return;
            }

            // 2. Сбор данных из полей
            DataBase db = new DataBase();
            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();
            string price = priceVal.ToString().Replace(",", "."); // Приводим к формату SQL (через точку)
            string imagePath = txtImagePath.Text.Trim();
            int qty = (int)numQty.Value;
            string desc = txtDesc.Text.Trim();

            try
            {
                // 3. Выполнение SQL-запроса в зависимости от типа
                if (cbType.SelectedIndex == 0) // ДОБАВЛЯЕМ МОТОЦИКЛ
                {
                    int year = (int)numYear.Value;
                    db.ExecuteNonQuery($@"INSERT INTO Motorcycles (Brand, Model, Price, Quantity, Description, ImageURL, Year) 
                                         VALUES (N'{brand}', N'{model}', {price}, {qty}, N'{desc}', '{imagePath}', {year})");
                }
                else // ДОБАВЛЯЕМ ЗАПЧАСТЬ
                {
                    db.ExecuteNonQuery($@"INSERT INTO Parts (Brand, PartName, Price, Quantity, Description, ImageURL) 
                                         VALUES (N'{brand}', N'{model}', {price}, {qty}, N'{desc}', '{imagePath}')");
                }
                MessageBox.Show("Успешно добавлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка БД: " + ex.Message);
                this.DialogResult = DialogResult.None; // Оставляем форму открытой при ошибке БД
            }
        }

        #endregion
    }
}