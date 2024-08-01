using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
