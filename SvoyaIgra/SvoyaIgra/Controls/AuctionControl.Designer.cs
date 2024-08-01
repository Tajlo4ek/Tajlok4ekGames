namespace SvoyaIgra.Controls
{
    partial class AuctionControl
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
            this.gbAuction = new System.Windows.Forms.GroupBox();
            this.tbAuctionRate = new System.Windows.Forms.TextBox();
            this.btnAuctionSet = new System.Windows.Forms.Button();
            this.btnAuctionAllIn = new System.Windows.Forms.Button();
            this.btnAuctionPass = new System.Windows.Forms.Button();
            this.trBarAuctionRate = new System.Windows.Forms.TrackBar();
            this.gbAuction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trBarAuctionRate)).BeginInit();
            this.SuspendLayout();
            // 
            // gbAuction
            // 
            this.gbAuction.Controls.Add(this.tbAuctionRate);
            this.gbAuction.Controls.Add(this.btnAuctionSet);
            this.gbAuction.Controls.Add(this.btnAuctionAllIn);
            this.gbAuction.Controls.Add(this.btnAuctionPass);
            this.gbAuction.Controls.Add(this.trBarAuctionRate);
            this.gbAuction.Location = new System.Drawing.Point(3, 3);
            this.gbAuction.Name = "gbAuction";
            this.gbAuction.Size = new System.Drawing.Size(200, 100);
            this.gbAuction.TabIndex = 20;
            this.gbAuction.TabStop = false;
            this.gbAuction.Text = "Аукцион";
            // 
            // tbAuctionRate
            // 
            this.tbAuctionRate.Enabled = false;
            this.tbAuctionRate.Location = new System.Drawing.Point(6, 45);
            this.tbAuctionRate.Name = "tbAuctionRate";
            this.tbAuctionRate.Size = new System.Drawing.Size(187, 20);
            this.tbAuctionRate.TabIndex = 4;
            this.tbAuctionRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnAuctionSet
            // 
            this.btnAuctionSet.Location = new System.Drawing.Point(135, 71);
            this.btnAuctionSet.Name = "btnAuctionSet";
            this.btnAuctionSet.Size = new System.Drawing.Size(58, 23);
            this.btnAuctionSet.TabIndex = 3;
            this.btnAuctionSet.Text = "ставка";
            this.btnAuctionSet.UseVisualStyleBackColor = true;
            this.btnAuctionSet.Click += new System.EventHandler(this.BtnAuctionSet_Click);
            // 
            // btnAuctionAllIn
            // 
            this.btnAuctionAllIn.Location = new System.Drawing.Point(71, 71);
            this.btnAuctionAllIn.Name = "btnAuctionAllIn";
            this.btnAuctionAllIn.Size = new System.Drawing.Size(58, 23);
            this.btnAuctionAllIn.TabIndex = 2;
            this.btnAuctionAllIn.Text = "ва-банк";
            this.btnAuctionAllIn.UseVisualStyleBackColor = true;
            this.btnAuctionAllIn.Click += new System.EventHandler(this.BtnAuctionAllIn_Click);
            // 
            // btnAuctionPass
            // 
            this.btnAuctionPass.Location = new System.Drawing.Point(6, 71);
            this.btnAuctionPass.Name = "btnAuctionPass";
            this.btnAuctionPass.Size = new System.Drawing.Size(59, 23);
            this.btnAuctionPass.TabIndex = 1;
            this.btnAuctionPass.Text = "пас";
            this.btnAuctionPass.UseVisualStyleBackColor = true;
            this.btnAuctionPass.Click += new System.EventHandler(this.BtnAuctionPass_Click);
            // 
            // trBarAuctionRate
            // 
            this.trBarAuctionRate.AutoSize = false;
            this.trBarAuctionRate.LargeChange = 50;
            this.trBarAuctionRate.Location = new System.Drawing.Point(6, 20);
            this.trBarAuctionRate.Maximum = 50;
            this.trBarAuctionRate.Name = "trBarAuctionRate";
            this.trBarAuctionRate.Size = new System.Drawing.Size(188, 26);
            this.trBarAuctionRate.SmallChange = 50;
            this.trBarAuctionRate.TabIndex = 0;
            this.trBarAuctionRate.TickFrequency = 100;
            this.trBarAuctionRate.ValueChanged += new System.EventHandler(this.TrBarAuctionRate_ValueChanged);
            // 
            // AuctionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.gbAuction);
            this.Name = "AuctionControl";
            this.Size = new System.Drawing.Size(206, 106);
            this.gbAuction.ResumeLayout(false);
            this.gbAuction.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trBarAuctionRate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbAuction;
        private System.Windows.Forms.TextBox tbAuctionRate;
        private System.Windows.Forms.Button btnAuctionSet;
        private System.Windows.Forms.Button btnAuctionAllIn;
        private System.Windows.Forms.Button btnAuctionPass;
        private System.Windows.Forms.TrackBar trBarAuctionRate;
    }
}
