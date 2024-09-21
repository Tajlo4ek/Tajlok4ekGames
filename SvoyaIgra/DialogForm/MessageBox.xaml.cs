using System.Windows;

namespace DialogForm
{
    /// <summary>
    /// Логика взаимодействия для MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window, IDialog
    {
        public MessageBox() : this("info")
        {

        }

        public MessageBox(string info)
        {
            InitializeComponent();
            textBlock.Text = info;
        }

        public Utils.DialogResult Result { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result = Utils.DialogResult.Yes;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Result = Utils.DialogResult.Yes;
        }

    }
}
