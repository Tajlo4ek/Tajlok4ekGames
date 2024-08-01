namespace SvoyaIgra.Controls
{
    partial class UserEditControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbConfigUser = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnM1000 = new System.Windows.Forms.Button();
            this.btnP100 = new System.Windows.Forms.Button();
            this.btnP1000 = new System.Windows.Forms.Button();
            this.btnP500 = new System.Windows.Forms.Button();
            this.btnM100 = new System.Windows.Forms.Button();
            this.btnM500 = new System.Windows.Forms.Button();
            this.tbConfigMoney = new System.Windows.Forms.TextBox();
            this.btnConfigCancel = new System.Windows.Forms.Button();
            this.btnConfigSet = new System.Windows.Forms.Button();
            this.gbConfigUser.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbConfigUser
            // 
            this.gbConfigUser.Controls.Add(this.tableLayoutPanel1);
            this.gbConfigUser.Controls.Add(this.tbConfigMoney);
            this.gbConfigUser.Controls.Add(this.btnConfigCancel);
            this.gbConfigUser.Controls.Add(this.btnConfigSet);
            this.gbConfigUser.Location = new System.Drawing.Point(3, 3);
            this.gbConfigUser.Name = "gbConfigUser";
            this.gbConfigUser.Size = new System.Drawing.Size(217, 164);
            this.gbConfigUser.TabIndex = 19;
            this.gbConfigUser.TabStop = false;
            this.gbConfigUser.Text = "gbConfigUser";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.btnM1000, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnP100, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnP1000, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnP500, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnM100, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnM500, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 61);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(205, 62);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // btnM1000
            // 
            this.btnM1000.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnM1000.Location = new System.Drawing.Point(139, 34);
            this.btnM1000.Name = "btnM1000";
            this.btnM1000.Size = new System.Drawing.Size(63, 25);
            this.btnM1000.TabIndex = 9;
            this.btnM1000.Text = "-1000";
            this.btnM1000.UseVisualStyleBackColor = true;
            this.btnM1000.Click += new System.EventHandler(this.BtnM1000_Click);
            // 
            // btnP100
            // 
            this.btnP100.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnP100.Location = new System.Drawing.Point(3, 3);
            this.btnP100.Name = "btnP100";
            this.btnP100.Size = new System.Drawing.Size(62, 25);
            this.btnP100.TabIndex = 4;
            this.btnP100.Text = "+100";
            this.btnP100.UseVisualStyleBackColor = true;
            this.btnP100.Click += new System.EventHandler(this.BtnP100_Click);
            // 
            // btnP1000
            // 
            this.btnP1000.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnP1000.Location = new System.Drawing.Point(139, 3);
            this.btnP1000.Name = "btnP1000";
            this.btnP1000.Size = new System.Drawing.Size(63, 25);
            this.btnP1000.TabIndex = 10;
            this.btnP1000.Text = "+1000";
            this.btnP1000.UseVisualStyleBackColor = true;
            this.btnP1000.Click += new System.EventHandler(this.BtnP1000_Click);
            // 
            // btnP500
            // 
            this.btnP500.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnP500.Location = new System.Drawing.Point(71, 3);
            this.btnP500.Name = "btnP500";
            this.btnP500.Size = new System.Drawing.Size(62, 25);
            this.btnP500.TabIndex = 8;
            this.btnP500.Text = "+500";
            this.btnP500.UseVisualStyleBackColor = true;
            this.btnP500.Click += new System.EventHandler(this.BtnP500_Click);
            // 
            // btnM100
            // 
            this.btnM100.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnM100.Location = new System.Drawing.Point(3, 34);
            this.btnM100.Name = "btnM100";
            this.btnM100.Size = new System.Drawing.Size(62, 25);
            this.btnM100.TabIndex = 3;
            this.btnM100.Text = "-100";
            this.btnM100.UseVisualStyleBackColor = true;
            this.btnM100.Click += new System.EventHandler(this.BtnM100_Click);
            // 
            // btnM500
            // 
            this.btnM500.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnM500.Location = new System.Drawing.Point(71, 34);
            this.btnM500.Name = "btnM500";
            this.btnM500.Size = new System.Drawing.Size(62, 25);
            this.btnM500.TabIndex = 7;
            this.btnM500.Text = "-500";
            this.btnM500.UseVisualStyleBackColor = true;
            this.btnM500.Click += new System.EventHandler(this.BtnM500_Click);
            // 
            // tbConfigMoney
            // 
            this.tbConfigMoney.Location = new System.Drawing.Point(6, 35);
            this.tbConfigMoney.Name = "tbConfigMoney";
            this.tbConfigMoney.Size = new System.Drawing.Size(205, 20);
            this.tbConfigMoney.TabIndex = 2;
            this.tbConfigMoney.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TbConfigMoney_KeyPress);
            // 
            // btnConfigCancel
            // 
            this.btnConfigCancel.Location = new System.Drawing.Point(136, 135);
            this.btnConfigCancel.Name = "btnConfigCancel";
            this.btnConfigCancel.Size = new System.Drawing.Size(75, 23);
            this.btnConfigCancel.TabIndex = 1;
            this.btnConfigCancel.Text = "Отмена";
            this.btnConfigCancel.UseVisualStyleBackColor = true;
            this.btnConfigCancel.Click += new System.EventHandler(this.BtnConfigCancel_Click);
            // 
            // btnConfigSet
            // 
            this.btnConfigSet.Location = new System.Drawing.Point(6, 135);
            this.btnConfigSet.Name = "btnConfigSet";
            this.btnConfigSet.Size = new System.Drawing.Size(75, 23);
            this.btnConfigSet.TabIndex = 0;
            this.btnConfigSet.Text = "Установить";
            this.btnConfigSet.UseVisualStyleBackColor = true;
            this.btnConfigSet.Click += new System.EventHandler(this.BtnConfigSet_Click);
            // 
            // UserEditControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.gbConfigUser);
            this.Name = "UserEditControl";
            this.Size = new System.Drawing.Size(223, 170);
            this.gbConfigUser.ResumeLayout(false);
            this.gbConfigUser.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbConfigUser;
        private System.Windows.Forms.TextBox tbConfigMoney;
        private System.Windows.Forms.Button btnConfigCancel;
        private System.Windows.Forms.Button btnConfigSet;
        private System.Windows.Forms.Button btnP1000;
        private System.Windows.Forms.Button btnM1000;
        private System.Windows.Forms.Button btnP500;
        private System.Windows.Forms.Button btnM500;
        private System.Windows.Forms.Button btnP100;
        private System.Windows.Forms.Button btnM100;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
