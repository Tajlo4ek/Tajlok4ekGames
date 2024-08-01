namespace SvoyaIgra.Forms
{
    partial class GameForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            this.mediaTimer = new System.Windows.Forms.Timer(this.components);
            this.imagePlayer = new System.Windows.Forms.PictureBox();
            this.pbRoundData = new System.Windows.Forms.PictureBox();
            this.moveTimer = new System.Windows.Forms.Timer(this.components);
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.tlpPlayers = new System.Windows.Forms.TableLayoutPanel();
            this.adminPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnSP = new System.Windows.Forms.Button();
            this.btnAns = new System.Windows.Forms.Button();
            this.btnSkip = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.tlpAns = new System.Windows.Forms.TableLayoutPanel();
            this.btnAnsFalse = new System.Windows.Forms.Button();
            this.btnAnsTrue = new System.Windows.Forms.Button();
            this.btnAnsHalf = new System.Windows.Forms.Button();
            this.pbAnswer = new System.Windows.Forms.PictureBox();
            this.pbAdminSay = new System.Windows.Forms.PictureBox();
            this.pbAdminImage = new System.Windows.Forms.PictureBox();
            this.pbAdminName = new System.Windows.Forms.PictureBox();
            this.pbProgressBar = new System.Windows.Forms.PictureBox();
            this.userConfigMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.kickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configMoneyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setChoiseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            ((System.ComponentModel.ISupportInitialize)(this.imagePlayer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRoundData)).BeginInit();
            this.adminPanel.SuspendLayout();
            this.tlpAns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAnswer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdminSay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdminImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdminName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbProgressBar)).BeginInit();
            this.userConfigMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // mediaTimer
            // 
            this.mediaTimer.Tick += new System.EventHandler(this.MediaTimer_Tick);
            // 
            // imagePlayer
            // 
            this.imagePlayer.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.imagePlayer.BackgroundImage = global::SvoyaIgra.Properties.Resources.Background;
            this.imagePlayer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.imagePlayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imagePlayer.Location = new System.Drawing.Point(150, 22);
            this.imagePlayer.Margin = new System.Windows.Forms.Padding(0);
            this.imagePlayer.Name = "imagePlayer";
            this.imagePlayer.Size = new System.Drawing.Size(500, 303);
            this.imagePlayer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imagePlayer.TabIndex = 3;
            this.imagePlayer.TabStop = false;
            // 
            // pbRoundData
            // 
            this.pbRoundData.BackgroundImage = global::SvoyaIgra.Properties.Resources.Background;
            this.pbRoundData.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbRoundData.InitialImage = global::SvoyaIgra.Properties.Resources.Background;
            this.pbRoundData.Location = new System.Drawing.Point(150, 22);
            this.pbRoundData.Name = "pbRoundData";
            this.pbRoundData.Size = new System.Drawing.Size(500, 303);
            this.pbRoundData.TabIndex = 4;
            this.pbRoundData.TabStop = false;
            this.pbRoundData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PbRoundData_MouseDown);
            this.pbRoundData.MouseLeave += new System.EventHandler(this.PbRoundData_MouseLeave);
            this.pbRoundData.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PbRoundData_MouseMove);
            // 
            // rtbChat
            // 
            this.rtbChat.Location = new System.Drawing.Point(656, 12);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.Size = new System.Drawing.Size(116, 313);
            this.rtbChat.TabIndex = 5;
            this.rtbChat.TabStop = false;
            this.rtbChat.Text = "";
            // 
            // tlpPlayers
            // 
            this.tlpPlayers.ColumnCount = 1;
            this.tlpPlayers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPlayers.Location = new System.Drawing.Point(150, 328);
            this.tlpPlayers.Name = "tlpPlayers";
            this.tlpPlayers.RowCount = 3;
            this.tlpPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpPlayers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpPlayers.Size = new System.Drawing.Size(500, 121);
            this.tlpPlayers.TabIndex = 6;
            // 
            // adminPanel
            // 
            this.adminPanel.ColumnCount = 1;
            this.adminPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.adminPanel.Controls.Add(this.btnSP, 0, 0);
            this.adminPanel.Controls.Add(this.btnAns, 0, 1);
            this.adminPanel.Controls.Add(this.btnSkip, 0, 2);
            this.adminPanel.Controls.Add(this.btnExit, 0, 3);
            this.adminPanel.Location = new System.Drawing.Point(656, 328);
            this.adminPanel.Name = "adminPanel";
            this.adminPanel.RowCount = 4;
            this.adminPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.adminPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.adminPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.adminPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.adminPanel.Size = new System.Drawing.Size(116, 121);
            this.adminPanel.TabIndex = 7;
            this.adminPanel.TabStop = true;
            // 
            // btnSP
            // 
            this.btnSP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSP.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSP.Location = new System.Drawing.Point(3, 3);
            this.btnSP.Name = "btnSP";
            this.btnSP.Size = new System.Drawing.Size(110, 24);
            this.btnSP.TabIndex = 0;
            this.btnSP.TabStop = false;
            this.btnSP.Text = "старт";
            this.btnSP.UseVisualStyleBackColor = true;
            this.btnSP.Click += new System.EventHandler(this.BtnSP_Click);
            // 
            // btnAns
            // 
            this.btnAns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAns.Location = new System.Drawing.Point(3, 33);
            this.btnAns.Name = "btnAns";
            this.btnAns.Size = new System.Drawing.Size(110, 24);
            this.btnAns.TabIndex = 8;
            this.btnAns.Text = "Ответить";
            this.btnAns.UseVisualStyleBackColor = true;
            this.btnAns.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BtnAns_MouseDown);
            // 
            // btnSkip
            // 
            this.btnSkip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSkip.Enabled = false;
            this.btnSkip.Location = new System.Drawing.Point(3, 63);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(110, 24);
            this.btnSkip.TabIndex = 9;
            this.btnSkip.TabStop = false;
            this.btnSkip.Text = "пропуcтить";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.BtnSkip_Click);
            // 
            // btnExit
            // 
            this.btnExit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExit.Enabled = false;
            this.btnExit.Location = new System.Drawing.Point(3, 93);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(110, 25);
            this.btnExit.TabIndex = 10;
            this.btnExit.TabStop = false;
            this.btnExit.Text = "выход";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // tlpAns
            // 
            this.tlpAns.ColumnCount = 1;
            this.tlpAns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAns.Controls.Add(this.btnAnsFalse, 0, 2);
            this.tlpAns.Controls.Add(this.btnAnsTrue, 0, 0);
            this.tlpAns.Controls.Add(this.btnAnsHalf, 0, 1);
            this.tlpAns.Location = new System.Drawing.Point(9, 239);
            this.tlpAns.Name = "tlpAns";
            this.tlpAns.RowCount = 3;
            this.tlpAns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpAns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpAns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpAns.Size = new System.Drawing.Size(132, 86);
            this.tlpAns.TabIndex = 7;
            // 
            // btnAnsFalse
            // 
            this.btnAnsFalse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAnsFalse.Location = new System.Drawing.Point(3, 59);
            this.btnAnsFalse.Name = "btnAnsFalse";
            this.btnAnsFalse.Size = new System.Drawing.Size(126, 24);
            this.btnAnsFalse.TabIndex = 11;
            this.btnAnsFalse.TabStop = false;
            this.btnAnsFalse.Text = "Не верно";
            this.btnAnsFalse.UseVisualStyleBackColor = true;
            this.btnAnsFalse.Click += new System.EventHandler(this.BtnAnsFalse_Click);
            // 
            // btnAnsTrue
            // 
            this.btnAnsTrue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAnsTrue.Location = new System.Drawing.Point(3, 3);
            this.btnAnsTrue.Name = "btnAnsTrue";
            this.btnAnsTrue.Size = new System.Drawing.Size(126, 22);
            this.btnAnsTrue.TabIndex = 0;
            this.btnAnsTrue.TabStop = false;
            this.btnAnsTrue.Text = "Верно";
            this.btnAnsTrue.UseVisualStyleBackColor = true;
            this.btnAnsTrue.Click += new System.EventHandler(this.BtnAnsTrue_Click);
            // 
            // btnAnsHalf
            // 
            this.btnAnsHalf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAnsHalf.Location = new System.Drawing.Point(3, 31);
            this.btnAnsHalf.Name = "btnAnsHalf";
            this.btnAnsHalf.Size = new System.Drawing.Size(126, 22);
            this.btnAnsHalf.TabIndex = 1;
            this.btnAnsHalf.Text = "Половина";
            this.btnAnsHalf.UseVisualStyleBackColor = true;
            this.btnAnsHalf.Click += new System.EventHandler(this.BtnAnsHalf_Click);
            // 
            // pbAnswer
            // 
            this.pbAnswer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.pbAnswer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbAnswer.InitialImage = global::SvoyaIgra.Properties.Resources.Background;
            this.pbAnswer.Location = new System.Drawing.Point(9, 331);
            this.pbAnswer.Name = "pbAnswer";
            this.pbAnswer.Size = new System.Drawing.Size(132, 118);
            this.pbAnswer.TabIndex = 12;
            this.pbAnswer.TabStop = false;
            this.pbAnswer.Visible = false;
            // 
            // pbAdminSay
            // 
            this.pbAdminSay.BackColor = System.Drawing.Color.Blue;
            this.pbAdminSay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbAdminSay.Location = new System.Drawing.Point(9, 182);
            this.pbAdminSay.Name = "pbAdminSay";
            this.pbAdminSay.Size = new System.Drawing.Size(132, 51);
            this.pbAdminSay.TabIndex = 13;
            this.pbAdminSay.TabStop = false;
            // 
            // pbAdminImage
            // 
            this.pbAdminImage.ErrorImage = global::SvoyaIgra.Properties.Resources.NoImg;
            this.pbAdminImage.InitialImage = null;
            this.pbAdminImage.Location = new System.Drawing.Point(9, 11);
            this.pbAdminImage.Name = "pbAdminImage";
            this.pbAdminImage.Size = new System.Drawing.Size(132, 133);
            this.pbAdminImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAdminImage.TabIndex = 14;
            this.pbAdminImage.TabStop = false;
            // 
            // pbAdminName
            // 
            this.pbAdminName.BackColor = System.Drawing.Color.Transparent;
            this.pbAdminName.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pbAdminName.Location = new System.Drawing.Point(9, 150);
            this.pbAdminName.Name = "pbAdminName";
            this.pbAdminName.Size = new System.Drawing.Size(132, 26);
            this.pbAdminName.TabIndex = 15;
            this.pbAdminName.TabStop = false;
            // 
            // pbProgressBar
            // 
            this.pbProgressBar.ErrorImage = null;
            this.pbProgressBar.InitialImage = null;
            this.pbProgressBar.Location = new System.Drawing.Point(150, 9);
            this.pbProgressBar.Name = "pbProgressBar";
            this.pbProgressBar.Size = new System.Drawing.Size(500, 10);
            this.pbProgressBar.TabIndex = 17;
            this.pbProgressBar.TabStop = false;
            this.pbProgressBar.Visible = false;
            // 
            // userConfigMenu
            // 
            this.userConfigMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.kickToolStripMenuItem,
            this.configMoneyToolStripMenuItem,
            this.setChoiseToolStripMenuItem});
            this.userConfigMenu.Name = "userConfigMenu";
            this.userConfigMenu.Size = new System.Drawing.Size(151, 70);
            // 
            // kickToolStripMenuItem
            // 
            this.kickToolStripMenuItem.Name = "kickToolStripMenuItem";
            this.kickToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.kickToolStripMenuItem.Text = "kick";
            this.kickToolStripMenuItem.Click += new System.EventHandler(this.KickToolStripMenuItem_Click);
            // 
            // configMoneyToolStripMenuItem
            // 
            this.configMoneyToolStripMenuItem.Name = "configMoneyToolStripMenuItem";
            this.configMoneyToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.configMoneyToolStripMenuItem.Text = "Config Money";
            this.configMoneyToolStripMenuItem.Click += new System.EventHandler(this.ConfigMoneyToolStripMenuItem_Click);
            // 
            // setChoiseToolStripMenuItem
            // 
            this.setChoiseToolStripMenuItem.Name = "setChoiseToolStripMenuItem";
            this.setChoiseToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.setChoiseToolStripMenuItem.Text = "Set choice";
            this.setChoiseToolStripMenuItem.Click += new System.EventHandler(this.SetChoiseToolStripMenuItem_Click);
            // 
            // videoPlayer
            // 
            this.videoPlayer.Enabled = true;
            this.videoPlayer.Location = new System.Drawing.Point(150, 25);
            this.videoPlayer.Name = "videoPlayer";
            this.videoPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("videoPlayer.OcxState")));
            this.videoPlayer.Size = new System.Drawing.Size(500, 300);
            this.videoPlayer.TabIndex = 2;
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.pbAdminName);
            this.Controls.Add(this.pbAdminImage);
            this.Controls.Add(this.pbProgressBar);
            this.Controls.Add(this.pbAdminSay);
            this.Controls.Add(this.pbAnswer);
            this.Controls.Add(this.tlpAns);
            this.Controls.Add(this.adminPanel);
            this.Controls.Add(this.tlpPlayers);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.pbRoundData);
            this.Controls.Add(this.imagePlayer);
            this.Controls.Add(this.videoPlayer);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "GameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "tajlo4ek\'s game";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GameForm_FormClosed);
            this.SizeChanged += new System.EventHandler(this.GameForm_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.imagePlayer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRoundData)).EndInit();
            this.adminPanel.ResumeLayout(false);
            this.tlpAns.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbAnswer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdminSay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdminImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdminName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbProgressBar)).EndInit();
            this.userConfigMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.videoPlayer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer mediaTimer;
        private AxWMPLib.AxWindowsMediaPlayer videoPlayer;
        private System.Windows.Forms.PictureBox imagePlayer;
        private System.Windows.Forms.PictureBox pbRoundData;
        private System.Windows.Forms.Timer moveTimer;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.TableLayoutPanel tlpPlayers;
        private System.Windows.Forms.TableLayoutPanel adminPanel;
        private System.Windows.Forms.Button btnSP;
        private System.Windows.Forms.Button btnAns;
        private System.Windows.Forms.Button btnSkip;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TableLayoutPanel tlpAns;
        private System.Windows.Forms.Button btnAnsFalse;
        private System.Windows.Forms.Button btnAnsTrue;
        private System.Windows.Forms.PictureBox pbAnswer;
        private System.Windows.Forms.PictureBox pbAdminSay;
        private System.Windows.Forms.PictureBox pbAdminImage;
        private System.Windows.Forms.PictureBox pbAdminName;
        private System.Windows.Forms.PictureBox pbProgressBar;
        private System.Windows.Forms.ContextMenuStrip userConfigMenu;
        private System.Windows.Forms.ToolStripMenuItem kickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configMoneyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setChoiseToolStripMenuItem;
        private System.Windows.Forms.Button btnAnsHalf;
    }
}