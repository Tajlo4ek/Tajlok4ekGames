using DataStore;
using DataStore.Utils.PackUtils;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Editor.MyControl.DataControl
{
    /// <summary>
    /// Логика взаимодействия для ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl, IData
    {
        public Action<EmptyControl.CorrectType, IData> CorrectAction;

        private bool isLocal = true;
        private string path = "";

        public Action ContentChanged;

        private readonly PackManager packManager;

        public ImageControl(PackManager packManager)
        {
            InitializeComponent();
            this.packManager = packManager;
        }

        public Control GetControl()
        {
            return this;
        }

        public Scenario GetData()
        {
            return new Scenario((isLocal ? "@" : "") + path, Scenario.ScenarioType.Image);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png, *gif) | *.jpg; *.jpeg; *.jpe; *.png; *.gif | All files (*.*)|*.*"
            };

            if (fd.ShowDialog() == true)
            {
                isLocal = true;
                TryLoad(fd.FileName);
            }

        }

        private void BtnWeb_Click(object sender, EventArgs e)
        {
            var dialog = new DialogForm.InputDialog();

            if (dialog.ShowDialog() == true)
            {
                if (dialog.Result == DialogForm.Utils.DialogResult.Yes)
                {
                    isLocal = false;
                    TryLoad(dialog.InputData);
                }
            }

        }

        private void TryLoad(string path)
        {


            try
            {
                if (isLocal)
                {
                    path = "@" + path;
                }

                this.path = packManager.WorkDirectory + @"\" + packManager.AddToPackFolder(path);
                isLocal = true;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.UriSource = new Uri(this.path);
                    bmp.EndInit();

                    pbImage.Source = bmp;
                }));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                this.path = "bad file";

                this.Dispatcher.Invoke(new Action(() =>
                {
                    pbImage.Source = new BitmapImage(new Uri("/Editor;component/Resources/exclamation.png", UriKind.Relative));
                }));
            }

            ContentChanged?.Invoke();
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            CorrectAction?.Invoke(EmptyControl.CorrectType.Up, this);
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            CorrectAction?.Invoke(EmptyControl.CorrectType.Down, this);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            CorrectAction?.Invoke(EmptyControl.CorrectType.Delete, this);
        }

        public void Clear()
        {
            pbImage.Source = null;
        }

        public void Parse(Scenario scenario)
        {
            isLocal = true;
            TryLoad(scenario.Data);
        }
    }
}
