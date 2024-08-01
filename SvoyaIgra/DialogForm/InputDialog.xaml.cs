using System.Windows;

namespace DialogForm
{
    /// <summary>
    /// Логика взаимодействия для InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window, IDialog
    {
        public Utils.DialogResult Result { get; private set; }

        public string InputData { get { return tbInput.Text; } }

        public InputDialog()
        {
            InitializeComponent();
            Title = "";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result = Utils.DialogResult.Yes;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result = Utils.DialogResult.Cancel;
        }

    }
}
