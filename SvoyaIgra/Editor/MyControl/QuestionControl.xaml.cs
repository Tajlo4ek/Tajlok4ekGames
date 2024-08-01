using DataStore;
using DataStore.Utils.PackUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.MyControl
{
    /// <summary>
    /// Логика взаимодействия для QuestionControl.xaml
    /// </summary>
    public partial class QuestionControl : UserControl
    {
        private readonly EmptyControl questionView;
        private readonly EmptyControl answerView;

        private const string typeNormalString = "Обычный";
        private const string typeCatString = "Кот в мешке";
        private const string typeAuctionString = "Аукцион";
        private const string typeNoRiskString = "Без риска";

        public Action<QuestionControl> DeleteAction;

        public Action ContentChanged;

        public QuestionControl(PackManager packManager)
        {
            InitializeComponent();

            tbCost.Text = "100";

            questionView = new EmptyControl("Вопрос", packManager);
            answerView = new EmptyControl("Ответ", packManager);

            questionView.ContentChanged += () => { ContentChanged?.Invoke(); };
            answerView.ContentChanged += () => { ContentChanged?.Invoke(); };

            stackPanel.Children.Add(questionView);
            stackPanel.Children.Add(answerView);

            cbType.Items.Add(typeNormalString);
            cbType.Items.Add(typeCatString);
            cbType.Items.Add(typeAuctionString);
            cbType.Items.Add(typeNoRiskString);

            cbType.SelectedItem = typeNormalString;

            expander.IsExpanded = false;
        }

        private void TbCost_TextChanged(object sender, EventArgs e)
        {
            expander.Header = tbCost.Text;
            ContentChanged?.Invoke();
        }

        public Question GetData()
        {
            if (tbCost.Text == "")
            {
                tbCost.Text = "0";
            }

            if (tbCatCost.Text == "")
            {
                tbCatCost.Text = "0";
            }

            var answers = answerView.GetData();
            var questions = questionView.GetData();

            if (answers.FindIndex((x) => x.Type == Scenario.ScenarioType.Text) == -1)
            {
                answers.Add(new Scenario("no text answer", Scenario.ScenarioType.Text));
            }

            Question.QuestionType type = Question.QuestionType.Normal;

            if (cbType.Text.Equals(typeCatString))
            {
                type = Question.QuestionType.Bagcat;
            }
            else if (cbType.Text.Equals(typeAuctionString))
            {
                type = Question.QuestionType.Auction;
            }
            else if (cbType.Text.Equals(typeNoRiskString))
            {
                type = Question.QuestionType.NoRisk;
            }

            if (type == Question.QuestionType.Normal)
            {
                return new Question(
                    questions,
                    answers,
                    int.Parse(tbCost.Text),
                    type);
            }
            else
            {
                return new Question(
                   questions,
                   answers,
                   int.Parse(tbCost.Text),
                   type,
                   tbCatTheme.Text,
                   int.Parse(tbCatCost.Text));
            }


        }


        public void Clear()
        {
            questionView.Clear();
            answerView.Clear();
        }

        public void ParseQuestion(Question question)
        {
            var scenarioCount = question.CountScenarios;
            var answerCount = question.CountAnswer;
            var totalCount = scenarioCount + answerCount;

            for (int scenarioId = 0; scenarioId < scenarioCount; scenarioId++)
            {
                questionView.ParseScenario(question.GetScenario(scenarioId));
            }

            for (int scenarioId = 0; scenarioId < answerCount; scenarioId++)
            {
                answerView.ParseScenario(question.GetAnswer(scenarioId));
            }

            this.Dispatcher.Invoke(new Action(() =>
            {
                tbCost.Text = question.Cost.ToString();

                if (question.IsAuction)
                {
                    cbType.Text = typeAuctionString;
                    pCat.Visibility = Visibility.Collapsed;
                }
                else if (question.IsBagcat)
                {
                    cbType.Text = typeCatString;
                    pCat.Visibility = Visibility.Visible;
                }
                else if (question.IsNoRisk)
                {
                    cbType.Text = typeNoRiskString;
                    pCat.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cbType.Text = typeNormalString;
                    pCat.Visibility = Visibility.Collapsed;
                }

                tbCatTheme.Text = question.ThemeName;
                tbCatCost.Text = question.SpecialCost.ToString();
            }));

        }

        private void CbType_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString().Equals(typeCatString))
            {
                pCat.Visibility = Visibility.Visible;
            }
            else
            {
                pCat.Visibility = Visibility.Collapsed;
            }
            ContentChanged?.Invoke();
        }

        private void TbCatCost_TextChanged(object sender, EventArgs e)
        {
            ContentChanged?.Invoke();
        }

        private void TbCatTheme_TextChanged(object sender, EventArgs e)
        {
            ContentChanged?.Invoke();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var dialog = new DialogForm.YesNoDialog("Удалить вопрос?", tbCost.Text);
            if (dialog.ShowDialog() == true)
            {
                if (dialog.Result == DialogForm.Utils.DialogResult.Yes)
                {
                    DeleteAction?.Invoke(this);
                }
            }
        }

        private void TbCost_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) < 0;
        }

    }

}
