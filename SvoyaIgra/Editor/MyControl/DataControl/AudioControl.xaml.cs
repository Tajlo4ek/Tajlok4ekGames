using DataStore;
using DataStore.Utils.PackUtils;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TagLib.Mpeg;

namespace Editor.MyControl.DataControl
{
    /// <summary>
    /// Логика взаимодействия для AudioVideoControl.xaml
    /// </summary>
    public partial class AudioControl : UserControl, IData
    {
        public Action<EmptyControl.CorrectType, IData> CorrectAction;

        private bool isLocal = true;
        private string path = "";
        private int maxTime;
        private bool isFileLoaded;
        private bool isPlayed;

        public Action ContentChanged;

        private readonly PackManager packManager;

        private readonly DispatcherTimer playTimer;

        public AudioControl(PackManager packManager)
        {
            InitializeComponent();
            this.packManager = packManager;
            maxTime = 0;
            isFileLoaded = false;
            isPlayed = false;

            playTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            playTimer.Tick += PlayTimer_Tick;
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            long currentMediaTicks = mediaElement.Position.Ticks;
            long totalMediaTicks = mediaElement.NaturalDuration.TimeSpan.Ticks;

            if (totalMediaTicks > 0)
            {
                slider.Value = (double)currentMediaTicks / totalMediaTicks * 100;
            }
            else
            {
                slider.Value = 0;
            }
        }

        public Control GetControl()
        {
            return this;
        }

        public Scenario GetData()
        {
            int time = int.Parse(tbTime.Text);

            if (time == 0)
            {
                time = -1;
            }

            return new Scenario((isLocal ? "@" : "") + path, Scenario.ScenarioType.Audio, time);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog
            {
                Filter = "Audio files (*.mp3, *.wav) | *.mp3; *.wav; | All files (*.*)|*.*"
            };

            //fd.Filter = "Video files (*.mp4, *.avi) | *.mp4; *.avi; | All files (*.*)|*.*";


            if (fd.ShowDialog() == true)
            {
                TryLoad(fd.FileName);
                isLocal = true;
            }

        }

        private void TryLoad(string path)
        {
            mediaElement.Dispatcher.Invoke(new Action(() => { mediaElement.Stop(); }));

            AudioFile objAF = new AudioFile(path);
            var len = (int)Math.Ceiling(objAF.Properties.Duration.TotalMilliseconds / 1000);

            if (len != 0)
            {
                this.path = path;

                if (isLocal)
                {
                    path = "@" + path;
                }

                this.path = packManager.WorkDirectory + @"\" + packManager.AddToPackFolder(path);
                isLocal = true;
                isFileLoaded = true;
            }
            else
            {
                this.path = "bad file";
                isFileLoaded = false;
            }

            tbTime.Dispatcher.Invoke(new Action(() =>
            {
                if (len != 0)
                {
                    tbTime.Text = len.ToString();
                    maxTime = len;
                    mediaElement.Source = new Uri(path.Substring(1));
                }
                else
                {
                    tbTime.Text = "0";
                }
            }));

            ContentChanged?.Invoke();
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            CorrectAction?.Invoke(EmptyControl.CorrectType.Up, this);
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            CorrectAction?.Invoke(EmptyControl.CorrectType.Down, this);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            CorrectAction?.Invoke(EmptyControl.CorrectType.Delete, this);
        }

        private void TbTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!tbTime.Text.Equals(""))
            {
                int time = int.Parse(tbTime.Text);
                if (time > maxTime)
                {
                    tbTime.Text = maxTime.ToString();
                }
            }

            ContentChanged?.Invoke();
        }

        private void TbTime_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (var ch in e.Text)
            {
                if (!"0123456789".Contains(ch + ""))
                {
                    e.Handled = true;
                }
            }
        }

        public void Clear()
        {
            mediaElement.Stop();
            mediaElement.Source = null;
        }

        public void Parse(Scenario scenario)
        {
            TryLoad(scenario.Data);
            tbTime.Dispatcher.Invoke(new Action(() =>
            {
                if (scenario.Time != -1)
                {
                    tbTime.Text = scenario.Time.ToString();
                }
                else
                {
                    tbTime.Text = maxTime.ToString();
                }
            }));
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (isFileLoaded)
            {
                if (isPlayed)
                {
                    mediaElement.Volume = 0;
                    mediaElement.Stop();
                    pbPlayPause.Source = new BitmapImage(new Uri("/Editor;component/Resources/IconPlay.png", UriKind.Relative));
                    playTimer.Stop();
                }
                else
                {
                    mediaElement.Volume = 0.5;
                    mediaElement.Play();
                    pbPlayPause.Source = new BitmapImage(new Uri("/Editor;component/Resources/IconPause.png", UriKind.Relative));
                    playTimer.Start();
                }
                isPlayed = !isPlayed;
            }

        }

    }
}
