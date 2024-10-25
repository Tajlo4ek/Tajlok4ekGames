using ClientServer.FileUtils;
using LauncherServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using Tajlo4ekUtils;

namespace LauncherClient
{
    public partial class MainForm : Form
    {
        private readonly Controller controller;
        private readonly Dictionary<string, Button> buttonsApp;


        int counNeedLoad = 0;
        int coutReady = 0;

        public MainForm()
        {
            InitializeComponent();

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;


            buttonsApp = new Dictionary<string, Button>();

            controller = new Controller();
            controller.AddNewApplication += CreateApplication;
            controller.OnAppUpdated += AppUpdated;
            controller.OnFileProcess += LoadProcess;
            controller.SendCountNeedLoad += RecvCountNeedLoad;
            controller.ShowMessage += ShowMessage;
        }

        private void ShowMessage(string message)
        {
            this.BeginInvoke(new Action(() =>
            {
                labelLog.Text = message;
            }));
        }

        private void LoadProcess(ProgressFileData data)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (data.State == ProgressFileData.States.End || data.State == ProgressFileData.States.Error)
                {
                    coutReady++;
                }

                if (coutReady == counNeedLoad)
                {
                    ShowMessage("Готово");
                }
                else
                {
                    ShowMessage("Загрузка " + coutReady.ToString() + "/" + counNeedLoad.ToString());
                }

            }));
        }

        private void RecvCountNeedLoad(int count)
        {
            counNeedLoad += count;
        }


        private void AppUpdated(string path)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (buttonsApp.TryGetValue(path, out Button btn))
                {
                    btn.Enabled = true;
                }
            }));
        }

        private void CreateApplication(string name, string path)
        {
            this.BeginInvoke(new Action(() =>
            {
                var parent = new FlowLayoutPanel
                {
                    AutoSize = true
                };

                var label = new Label
                {
                    Text = name,
                    Padding = new Padding(0, 7, 0, 0),
                    MinimumSize = new System.Drawing.Size(100, 20),
                    MaximumSize = new System.Drawing.Size(100, 20)
                };

                var button = new Button
                {
                    Text = "Запуск",
                    Enabled = false
                };

                buttonsApp.Add(path, button);

                button.Click += (object sender, EventArgs e) =>
                {
                    controller.Stop();
                    controller.RunApplication(path);
                    this.Close();
                };

                parent.Controls.Add(label);
                parent.Controls.Add(button);
                flpMain.Controls.Add(parent);
            }));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller?.Stop();
        }

        private void BtnSetting_Click(object sender, EventArgs e)
        {
            var window = new SettingsWindow
            {
                Location = this.Location,
                StartPosition = FormStartPosition.Manual
            };
            window.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            controller.Start();
        }
    }
}
