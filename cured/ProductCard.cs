using System;
using System.Drawing;
using System.Windows.Forms;

namespace cured
{
    public class ProductCard : UserControl
    {
        public PictureBox pictureBox;
        public Label labelName, labelPrice, labelDetails, labelArticle;

        public ProductCard()
        {
            this.Size = new Size(220, 280);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Margin = new Padding(10);
            this.Cursor = Cursors.Hand;

            pictureBox = new PictureBox { Size = new Size(200, 150), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
            labelName = new Label { Location = new Point(10, 170), Size = new Size(200, 40), Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            labelArticle = new Label { Location = new Point(10, 210), Size = new Size(200, 15), Font = new Font("Segoe UI", 7), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };
            labelPrice = new Label { Location = new Point(10, 225), Size = new Size(200, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Green, TextAlign = ContentAlignment.MiddleCenter };
            labelDetails = new Label { Location = new Point(10, 250), Size = new Size(200, 20), Font = new Font("Segoe UI", 7), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleCenter };

            this.Controls.Add(pictureBox);
            this.Controls.Add(labelName);
            this.Controls.Add(labelArticle);
            this.Controls.Add(labelPrice);
            this.Controls.Add(labelDetails);
        }
        public void PerformCardClick()
        {
            // Вызываем событие Click самой карточки
            this.OnClick(EventArgs.Empty);
        }
    }
}