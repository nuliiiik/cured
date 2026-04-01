namespace cured
{
    partial class profile
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(profile));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabAcc = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tabCart = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.btnAddOrder = new System.Windows.Forms.Button();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.dgvCart = new System.Windows.Forms.DataGridView();
            this.tabOrders = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.DeleteOrderBtn = new System.Windows.Forms.Button();
            this.dgvOrders = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnClearCart = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabAcc.SuspendLayout();
            this.tabCart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCart)).BeginInit();
            this.tabOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabAcc);
            this.tabControl1.Controls.Add(this.tabCart);
            this.tabControl1.Controls.Add(this.tabOrders);
            this.tabControl1.Location = new System.Drawing.Point(193, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(772, 507);
            this.tabControl1.TabIndex = 0;
            // 
            // tabAcc
            // 
            this.tabAcc.Controls.Add(this.label11);
            this.tabAcc.Controls.Add(this.button1);
            this.tabAcc.Controls.Add(this.maskedTextBox1);
            this.tabAcc.Controls.Add(this.textBox2);
            this.tabAcc.Controls.Add(this.textBox3);
            this.tabAcc.Controls.Add(this.textBox1);
            this.tabAcc.Controls.Add(this.label6);
            this.tabAcc.Controls.Add(this.label5);
            this.tabAcc.Controls.Add(this.label7);
            this.tabAcc.Controls.Add(this.label8);
            this.tabAcc.Location = new System.Drawing.Point(4, 22);
            this.tabAcc.Name = "tabAcc";
            this.tabAcc.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabAcc.Size = new System.Drawing.Size(764, 481);
            this.tabAcc.TabIndex = 0;
            this.tabAcc.Text = "tabPage1";
            this.tabAcc.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label11.Location = new System.Drawing.Point(287, 63);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(214, 37);
            this.label11.TabIndex = 24;
            this.label11.Text = "Ваш профиль";
            // 
            // button1
            // 
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(296, 331);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(205, 34);
            this.button1.TabIndex = 23;
            this.button1.Text = "Сохранить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.maskedTextBox1.Location = new System.Drawing.Point(404, 266);
            this.maskedTextBox1.Mask = "7-000-000-00-00";
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(169, 31);
            this.maskedTextBox1.TabIndex = 22;
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox2.Location = new System.Drawing.Point(308, 191);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(266, 31);
            this.textBox2.TabIndex = 21;
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox3.Location = new System.Drawing.Point(308, 229);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(266, 31);
            this.textBox3.TabIndex = 20;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox1.Location = new System.Drawing.Point(308, 154);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(266, 31);
            this.textBox1.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(215, 269);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(183, 25);
            this.label6.TabIndex = 18;
            this.label6.Text = "Номер телефона";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(230, 191);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 25);
            this.label5.TabIndex = 17;
            this.label5.Text = "Логин";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.Location = new System.Drawing.Point(215, 229);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 25);
            this.label7.TabIndex = 16;
            this.label7.Text = "Пароль";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label8.Location = new System.Drawing.Point(248, 154);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 25);
            this.label8.TabIndex = 15;
            this.label8.Text = "Имя";
            // 
            // tabCart
            // 
            this.tabCart.Controls.Add(this.btnClearCart);
            this.tabCart.Controls.Add(this.label10);
            this.tabCart.Controls.Add(this.btnAddOrder);
            this.tabCart.Controls.Add(this.lblTotal);
            this.tabCart.Controls.Add(this.btnDelete);
            this.tabCart.Controls.Add(this.dgvCart);
            this.tabCart.Location = new System.Drawing.Point(4, 22);
            this.tabCart.Name = "tabCart";
            this.tabCart.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabCart.Size = new System.Drawing.Size(764, 481);
            this.tabCart.TabIndex = 1;
            this.tabCart.Text = "tabPage2";
            this.tabCart.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label10.Location = new System.Drawing.Point(310, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(145, 37);
            this.label10.TabIndex = 25;
            this.label10.Text = "Корзина";
            // 
            // btnAddOrder
            // 
            this.btnAddOrder.Location = new System.Drawing.Point(653, 435);
            this.btnAddOrder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAddOrder.Name = "btnAddOrder";
            this.btnAddOrder.Size = new System.Drawing.Size(104, 24);
            this.btnAddOrder.TabIndex = 17;
            this.btnAddOrder.Text = "Оформить заказ";
            this.btnAddOrder.UseVisualStyleBackColor = true;
            this.btnAddOrder.Click += new System.EventHandler(this.btnAddOrder_Click);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTotal.Location = new System.Drawing.Point(6, 435);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(69, 25);
            this.lblTotal.TabIndex = 2;
            this.lblTotal.Text = "Итого";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(579, 18);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(70, 24);
            this.btnDelete.TabIndex = 14;
            this.btnDelete.Text = "Удалить";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // dgvCart
            // 
            this.dgvCart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCart.Location = new System.Drawing.Point(5, 66);
            this.dgvCart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvCart.Name = "dgvCart";
            this.dgvCart.RowHeadersWidth = 51;
            this.dgvCart.RowTemplate.Height = 24;
            this.dgvCart.Size = new System.Drawing.Size(754, 346);
            this.dgvCart.TabIndex = 0;
            // 
            // tabOrders
            // 
            this.tabOrders.Controls.Add(this.label12);
            this.tabOrders.Controls.Add(this.DeleteOrderBtn);
            this.tabOrders.Controls.Add(this.dgvOrders);
            this.tabOrders.Location = new System.Drawing.Point(4, 22);
            this.tabOrders.Name = "tabOrders";
            this.tabOrders.Size = new System.Drawing.Size(764, 481);
            this.tabOrders.TabIndex = 2;
            this.tabOrders.Text = "tabPage3";
            this.tabOrders.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label12.Location = new System.Drawing.Point(332, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(131, 37);
            this.label12.TabIndex = 25;
            this.label12.Text = "Заказы";
            // 
            // DeleteOrderBtn
            // 
            this.DeleteOrderBtn.Location = new System.Drawing.Point(664, 448);
            this.DeleteOrderBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DeleteOrderBtn.Name = "DeleteOrderBtn";
            this.DeleteOrderBtn.Size = new System.Drawing.Size(97, 24);
            this.DeleteOrderBtn.TabIndex = 15;
            this.DeleteOrderBtn.Text = "Отменить заказ";
            this.DeleteOrderBtn.UseVisualStyleBackColor = true;
            this.DeleteOrderBtn.Click += new System.EventHandler(this.DeleteOrderBtn_Click);
            // 
            // dgvOrders
            // 
            this.dgvOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOrders.Location = new System.Drawing.Point(4, 60);
            this.dgvOrders.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvOrders.Name = "dgvOrders";
            this.dgvOrders.ReadOnly = true;
            this.dgvOrders.RowHeadersWidth = 51;
            this.dgvOrders.RowTemplate.Height = 24;
            this.dgvOrders.Size = new System.Drawing.Size(756, 378);
            this.dgvOrders.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(12, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Аккаунт";
            this.label1.Click += new System.EventHandler(this.panel3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(12, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Корзина";
            this.label2.Click += new System.EventHandler(this.panel4_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(193, 21);
            this.panel1.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DarkRed;
            this.panel2.Controls.Add(this.label3);
            this.panel2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel2.Location = new System.Drawing.Point(0, 21);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(193, 50);
            this.panel2.TabIndex = 10;
            this.panel2.Click += new System.EventHandler(this.panel2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(31, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 33);
            this.label3.TabIndex = 11;
            this.label3.Text = "Главная";
            this.label3.Click += new System.EventHandler(this.panel2_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DarkRed;
            this.panel3.Controls.Add(this.label1);
            this.panel3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel3.Location = new System.Drawing.Point(36, 197);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(117, 42);
            this.panel3.TabIndex = 11;
            this.panel3.Click += new System.EventHandler(this.panel3_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.DarkRed;
            this.panel4.Controls.Add(this.label2);
            this.panel4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel4.Location = new System.Drawing.Point(36, 245);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(117, 42);
            this.panel4.TabIndex = 12;
            this.panel4.Click += new System.EventHandler(this.panel4_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(16, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 25);
            this.label4.TabIndex = 12;
            this.label4.Text = "Заказы";
            this.label4.Click += new System.EventHandler(this.panel5_Click);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.DarkRed;
            this.panel5.Controls.Add(this.label4);
            this.panel5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel5.Location = new System.Drawing.Point(36, 293);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(117, 42);
            this.panel5.TabIndex = 13;
            this.panel5.Click += new System.EventHandler(this.panel5_Click);
            // 
            // btnClearCart
            // 
            this.btnClearCart.Location = new System.Drawing.Point(653, 12);
            this.btnClearCart.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearCart.Name = "btnClearCart";
            this.btnClearCart.Size = new System.Drawing.Size(104, 37);
            this.btnClearCart.TabIndex = 26;
            this.btnClearCart.Text = "Очистить корзину";
            this.btnClearCart.UseVisualStyleBackColor = true;
            this.btnClearCart.Click += new System.EventHandler(this.btnClearCart_Click);
            // 
            // profile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(965, 518);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(662, 553);
            this.Name = "profile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ваш профиль";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.profile_FormClosed);
            this.Load += new System.EventHandler(this.profile_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabAcc.ResumeLayout(false);
            this.tabAcc.PerformLayout();
            this.tabCart.ResumeLayout(false);
            this.tabCart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCart)).EndInit();
            this.tabOrders.ResumeLayout(false);
            this.tabOrders.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabAcc;
        private System.Windows.Forms.TabPage tabCart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TabPage tabOrders;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.DataGridView dgvCart;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnAddOrder;
        private System.Windows.Forms.DataGridView dgvOrders;
        private System.Windows.Forms.Button DeleteOrderBtn;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnClearCart;
    }
}