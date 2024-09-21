using DataStore;
using DataStore.Utils.PackUtils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Editor.MyControl
{
    /// <summary>
    /// Логика взаимодействия для ThemeControl.xaml
    /// </summary>
    public partial class ThemeControl : UserControl
    {
        private readonly List<QuestionControl> questionControls;

        public Action<ThemeControl> DeleteAction;

        public Action ContentChanged;

        private readonly PackManager packManager;

        public ThemeControl(string name, PackManager packManager)
        {
            InitializeComponent();

            expander.IsExpanded = false;

            tbName.Text = name;

            questionControls = new List<QuestionControl>();

            this.packManager = packManager;
        }

        private void TbName_TextChanged(object sender, EventArgs e)
        {
            expander.Header = tbName.Text;
            ContentChanged?.Invoke();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            CreateQuestion();
        }

        public Theme GetData()
        {
            var questions = new List<Question>();

            foreach (var control in questionControls)
            {
                questions.Add(control.GetData());
            }

            return new Theme(tbName.Text, questions);
        }

        public void ParseTheme(Theme theme, Action<string> process = null)
        {
            var questionCount = theme.CountQuestions;
            for (int questionId = 0; questionId < questionCount; questionId++)
            {
                process?.Invoke(string.Format("Вопрос {0} / {1} \n", questionId + 1, questionCount));
                CreateQuestion(theme.GetQuestion(questionId));
            }
        }

        private void CreateQuestion(Question question = null)
        {
            QuestionControl control = null;

            this.Dispatcher.Invoke(new Action(() =>
            {
                control = new QuestionControl(packManager);
                control.DeleteAction += DeleteQuestion;
                control.ContentChanged += () => { ContentChanged?.Invoke(); };
                questionControls.Add(control);
                stackPanel.Children.Add(control);
            }));

            if (question != null)
            {
                control.ParseQuestion(question);
            }

            ContentChanged?.Invoke();
        }

        private void DeleteQuestion(QuestionControl questionControl)
        {
            questionControls.Remove(questionControl);
            stackPanel.Children.Remove(questionControl);

            ContentChanged?.Invoke();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var name = tbName.Text;
            if (name.Length > 20)
            {
                name = name.Substring(0, 20) + "...";
            }

            var dialog = new DialogForm.YesNoDialog("Удалить тему ?", name);
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
            foreach (var control in questionControls)
            {
                control.Clear();
            }
        }

    }
}
