using SvoyaIgra.Utils.Controllers;
using System;
using System.Windows.Forms;

namespace SvoyaIgra.Controls
{
    public partial class FinalAnsControl : UserControl
    {
        public Action<string> FinalAnsClick;

        public FinalAnsControl()
        {
            InitializeComponent();
        }

        private void BtnAnsFinal_Click(object sender, EventArgs e)
        {
            FinalAnsClick(rtbAns.Text);
        }
    }
}
