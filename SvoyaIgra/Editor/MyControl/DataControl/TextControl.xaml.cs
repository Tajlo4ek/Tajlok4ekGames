using System;
using System.Windows;
using System.Windows.Controls;
using DataStore;

namespace Editor.MyControl.DataControl
{
    /// <summary>
    /// Логика взаимодействия для TextControl.xaml
    /// </summary>
    public partial class TextControl : UserControl, IData
    {
        public Action<EmptyControl.CorrectType, IData> CorrectAction;

        public Action ContentChanged;

        public TextControl()
        {
            InitializeComponent();
        }

        public Control GetControl()
        {
            return this;
        }

        public Scenario GetData()
        {
            return new Scenario(rtbData.Text, Scenario.ScenarioType.Text);
        }

        private void Up_Click(object sender, EventArgs e)
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

        private void RtbData_TextChanged(object sender, RoutedEventArgs e)
        {
            ContentChanged?.Invoke();
        }

        public void Clear()
        {

        }

        public void Parse(Scenario scenario)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                rtbData.Text = scenario.Data;
            }));
        }
    }
}
