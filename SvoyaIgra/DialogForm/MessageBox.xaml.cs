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
