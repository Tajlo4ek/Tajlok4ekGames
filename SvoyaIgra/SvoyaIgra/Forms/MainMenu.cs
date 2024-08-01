using System;
using System.IO;
using System.Windows.Forms;

namespace SvoyaIgra.Forms
{
    public partial class MainMenu : Form
    {
        private readonly string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\myGame\";

        private const string ConfigName = @"config.cfg";

        private static MainMenu instance;

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
            if (File.Exists(ConfigPath + ConfigName))
            {
                using (StreamReader sr = new StreamReader(ConfigPath + ConfigName))
                {
                    tbInputNick.Text = sr.ReadLine();
                    tbInputIp.Text = sr.ReadLine();
                    tbImg.Text = sr.ReadLine();
                }
            }
            else
            {
                tbInputNick.Text = "randomUser" + new Random().Next(100000);
                tbInputIp.Text = "127.0.0.1";
                tbImg.Text = "";
                Save();
            }

            if (!Directory.Exists(Application.StartupPath + @"\logs\server"))
            {
                Directory.CreateDirectory(Application.StartupPath + @"\logs\server");
            }

            if (!Directory.Exists(Application.StartupPath + @"\logs\client"))
            {
                Directory.CreateDirectory(Application.StartupPath + @"\logs\client");
            }
        }

        private void Save()
        {
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }

            using (StreamWriter sw = new StreamWriter(ConfigPath + ConfigName))
            {
                sw.WriteLine(tbInputNick.Text);
                sw.WriteLine(tbInputIp.Text);
                sw.WriteLine(tbImg.Text);
            }

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
