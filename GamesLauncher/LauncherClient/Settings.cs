using LauncherServer;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Tajlo4ekUtils;

namespace LauncherClient
{
    public partial class SettingsWindow : Form
    {
        public SettingsWindow()
        {
            InitializeComponent();

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            if (ConfigSaver<Config>.Load(Config.ConfigName, out Config config) == false)
            {
                config = new Config();
            }

            tbBrowse.Text = config.ProgramPath;
            tbIp.Text = config.ServerIp;
            tbPort.Text = config.ServerPort.ToString();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(tbBrowse.Text) &&
                IPAddress.TryParse(tbIp.Text, out _) &&
                int.TryParse(tbPort.Text, out int port))
            {
                Config config = new Config
                {
                    ServerIp = tbIp.Text,
                    ServerPort = port,
                    ProgramPath = tbBrowse.Text
                };

                ConfigSaver<Config>.Save(Config.ConfigName, config);
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = tbBrowse.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    tbBrowse.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
