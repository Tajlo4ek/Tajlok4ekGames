using DataStore;
using DataStore.Utils.PackUtils;
using Editor.MyControl.DataControl;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Editor.MyControl
{
    /// <summary>
    /// Логика взаимодействия для EmptyControl.xaml
    /// </summary>
    public partial class EmptyControl : UserControl
    {
        public enum CorrectType
        {
            Up,
            Down,
            Delete,
        }

        public Action ContentChanged;

        private readonly PackManager packManager;

        public EmptyControl(string name, PackManager packManager)
        {
            InitializeComponent();

            expander.Header = name;

            expander.IsExpanded = false;

            this.packManager = packManager;
        }

        public EmptyControl(PackManager packManager) :
            this("name", packManager)
        {
        }

        private void BtnAddText_Click(object sender, EventArgs e)
        {
            var control = new DataControl.TextControl();
            control.CorrectAction += Correct;
            control.ContentChanged += () => ContentChanged?.Invoke();
            stackPanel.Children.Add(control);

            ContentChanged?.Invoke();
        }

        private void BtnAddImg_Click(object sender, EventArgs e)
        {
            var control = new DataControl.ImageControl(packManager);
            control.CorrectAction += Correct;
            control.ContentChanged += () => ContentChanged?.Invoke();
            stackPanel.Children.Add(control);

            ContentChanged?.Invoke();
        }

        private void BtnAddAudio_Click(object sender, EventArgs e)
        {
            var control = new DataControl.AudioControl(packManager);
            control.CorrectAction += Correct;
            control.ContentChanged += () => ContentChanged?.Invoke();
            stackPanel.Children.Add(control);

            ContentChanged?.Invoke();
        }

        private void BtnAddVideo_Click(object sender, EventArgs e)
        {
            var control = new DataControl.VideoControl(packManager);
            control.CorrectAction += Correct;
            control.ContentChanged += () => ContentChanged?.Invoke();
            stackPanel.Children.Add(control);

            ContentChanged?.Invoke();
        }

        public void Correct(CorrectType type, DataControl.IData data)
        {
            switch (type)
            {
                case CorrectType.Up:
                    {
                        int ind = stackPanel.Children.IndexOf(data.GetControl());
                        stackPanel.Children.RemoveAt(ind);
                        if (ind <= 1)
                        {
                            ind++;
                        }
                        stackPanel.Children.Insert(ind - 1, data.GetControl());
                    }
                    break;
                case CorrectType.Down:
                    {
                        int ind = stackPanel.Children.IndexOf(data.GetControl());
                        stackPanel.Children.RemoveAt(ind);
                        if (ind >= stackPanel.Children.Count)
                        {
                            ind--;
                        }
                        stackPanel.Children.Insert(ind + 1, data.GetControl());
                    }
                    break;
                case CorrectType.Delete:
                    {
                        stackPanel.Children.Remove(data.GetControl());
                    }
                    break;
            }

            ContentChanged?.Invoke();
        }

        public void Clear()
        {
            for (int i = 1; i < stackPanel.Children.Count; i++)
            {
                ((IData)stackPanel.Children[i]).Clear();
            }
        }

        public void ParseScenario(Scenario scenario)
        {
            IData data = null;

            this.Dispatcher.Invoke(new Action(() =>
            {
                switch (scenario.Type)
                {
                    case Scenario.ScenarioType.Text:
                        {
                            var control = new DataControl.TextControl();
                            control.CorrectAction += Correct;
                            stackPanel.Children.Add(control);
                            data = control;
                        }
                        break;
                    case Scenario.ScenarioType.Video:
                        {
                            var control = new DataControl.VideoControl(packManager);
                            control.CorrectAction += Correct;
                            stackPanel.Children.Add(control);
                            data = control;
                        }
                        break;
                    case Scenario.ScenarioType.Audio:
                        {
                            var control = new DataControl.AudioControl(packManager);
                            control.CorrectAction += Correct;
                            stackPanel.Children.Add(control);
                            data = control;
                        }
                        break;
                    case Scenario.ScenarioType.Image:
                        {
                            var control = new DataControl.ImageControl(packManager);
                            control.CorrectAction += Correct;
                            stackPanel.Children.Add(control);
                            data = control;
                        }
                        break;
                }
            }));

            data.Parse(scenario);
            ContentChanged?.Invoke();
        }

        public List<Scenario> GetData()
        {
            var data = new List<Scenario>();

            for (int i = 1; i < stackPanel.Children.Count; i++)
            {
                data.Add(((IData)stackPanel.Children[i]).GetData());
            }

            return data;
        }

    }
}
