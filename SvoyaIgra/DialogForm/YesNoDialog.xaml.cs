using System.Windows;

namespace DialogForm
{
    /// <summary>
    /// Логика взаимодействия для YesNoDialog.xaml
    /// </summary>
    public partial class YesNoDialog : Window, IDialog
    {
        public Utils.DialogResult Result { get; private set; }

        public YesNoDialog(string data, string formName)
        {
            InitializeComponent();
            this.Title = formName;
            label.Content = data;
        }

        public YesNoDialog() : this("Подтвердите действие", "")
        {

        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result = Utils.DialogResult.Yes;
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result = Utils.DialogResult.No;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result = Utils.DialogResult.Cancel;
        }

    }
}
