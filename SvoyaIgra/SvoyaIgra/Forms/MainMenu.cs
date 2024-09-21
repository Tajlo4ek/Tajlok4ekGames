using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Tajlo4ekUtils;

namespace SvoyaIgra.Forms
{
    public partial class MainMenu : Form
    {
        private static readonly string ConfigName = @"config";

        private static MainMenu instance;

        private class ConfigData
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Ip { get; set; }

            [DataMember]
            public string Image { get; set; }
        }

        public static void ShowMain()
        {
            instance.Show();
        }

        public MainMenu()
        {
            InitializeComponent();
            instance = this;
        }

        private bool ValidateIp()
        {
            return System.Net.IPAddress.TryParse(tbInputIp.Text, out var _);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            Save();

            string packPath = "";

            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "pack files |*" + DataStore.Utils.PackUtils.PackManager.SiqPackExtension + "; *" + DataStore.Utils.PackUtils.PackManager.MyPackExtension;

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    packPath = fd.FileName;
                }
                else
                {
                    return;
                }
            }

            if (ValidateIp())
            {
                new Utils.Controllers.GameController(true, tbInputIp.Text, tbInputNick.Text, tbImg.Text, packPath);
                this.Hide();
            }
            else
            {
                MessageBox.Show("bad ip");
            }
        }

        private void BtnJoin_Click(object sender, EventArgs e)
        {
            Save();
            if (ValidateIp())
            {
                new Utils.Controllers.GameController(false, tbInputIp.Text, tbInputNick.Text, tbImg.Text);
                this.Hide();
            }
            else
            {
                MessageBox.Show("bad ip");
            }
        }

        private void MainMenu_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            if (ConfigSaver<ConfigData>.Load(ConfigName, out ConfigData configData))
            {
                tbInputNick.Text = configData.Name;
                tbInputIp.Text = configData.Ip;
                tbImg.Text = configData.Image;
            }
            else
            {
                tbInputNick.Text = "randomUser" + new Random().Next(100000);
                tbInputIp.Text = "127.0.0.1";
                tbImg.Text = "";
                Save();
            }
        }

        private void Save()
        {
            ConfigSaver<ConfigData>.Save(
                ConfigName,
                new ConfigData
                {
                    Name = tbInputNick.Text,
                    Ip = tbInputIp.Text,
                    Image = tbImg.Text
                });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                fd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png;";

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    tbImg.Text = fd.FileName;
                    Save();
                }
            }
        }


    }
}
