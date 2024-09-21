using DataStore;
using DataStore.Utils.PackUtils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Editor.MyControl
{
    /// <summary>
    /// Логика взаимодействия для RoundControl.xaml
    /// </summary>
    public partial class RoundControl : UserControl
    {
        private readonly List<ThemeControl> themeControls;

        private const string normalString = "Обычный";
        private const string finalString = "Финал";

        public Action<RoundControl> DeleteAction;

        public Action ContentChanged;

        private readonly PackManager packManager;

        public RoundControl(string name, PackManager packManager)
        {
            InitializeComponent();


            expander.IsExpanded = false;

            tbName.Text = name;

            themeControls = new List<ThemeControl>();

            cbType.Items.Add(normalString);
            cbType.Items.Add(finalString);

            cbType.Text = normalString;

            this.packManager = packManager;
        }

        private void TbName_TextChanged(object sender, EventArgs e)
        {
            expander.Header = tbName.Text;
            ContentChanged?.Invoke();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            CreateTheme();
        }

        public Round GetData()
        {
            var themes = new List<Theme>();

            foreach (var control in themeControls)
            {
                var theme = control.GetData();

                if (theme.CountQuestions != 0)
                {
                    themes.Add(theme);
                }
            }

            return new Round(tbName.Text, themes, cbType.Text.Equals(finalString));
        }

        public void ParseRound(Round round, Action<string> process = null)
        {
            if (round.IsFinal)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    cbType.Text = finalString;
                }));
            }

            int themeCount = round.CountThemes;

            for (int themeId = 0; themeId < themeCount; themeId++)
            {
                var curString = string.Format("Тема {0} / {1} \n", themeId + 1, themeCount);
                CreateTheme(round.GetTheme(themeId), (text) =>
                {
                    process?.Invoke(string.Format("{0} {1} \n", curString, text));
                });
            }
        }

        private void CreateTheme(Theme theme = null, Action<string> process = null)
        {
            var name = theme == null ? "Тема" + (themeControls.Count + 1) : theme.Name;

            ThemeControl control = null;

            this.Dispatcher.Invoke(new Action(() =>
            {
                control = new ThemeControl(name, packManager);
                control.DeleteAction += DeleteTheme;
                control.ContentChanged += () => { ContentChanged?.Invoke(); };
                themeControls.Add(control);
                stackPanel.Children.Add(control);
            }));

            if (theme != null)
            {
                control.ParseTheme(theme, process);
            }

            ContentChanged?.Invoke();
        }

        private void DeleteTheme(ThemeControl themeControl)
        {
            themeControls.Remove(themeControl);
            stackPanel.Children.Remove(themeControl);

            ContentChanged?.Invoke();
        }

        private void CbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ContentChanged?.Invoke();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var name = tbName.Text;
            if (name.Length > 20)
            {
                name = name.Substring(0, 20) + "...";
            }

            var dialog = new DialogForm.YesNoDialog("Удалить раунд?", name);
            if (dialog.ShowDialog() == true)
            {
                if (dialog.Result == DialogForm.Utils.DialogResult.Yes)
                {
                    DeleteAction?.Invoke(this);
                }
            }


        }

        public void Clear()
        {
            foreach (var control in themeControls)
            {
                control.Clear();
            }
        }

    }
}
