using System;
using System.Windows.Forms;

namespace SvoyaIgra.Controls
{
    public partial class UserEditControl : UserControl
    {
        private string selectedUserToken;
        public Action<string, int> OnAcceptUserMoney;

        public UserEditControl()
        {
            InitializeComponent();

            selectedUserToken = "";
        }

        public void ShowEdit(string selectedUserToken, string name, int nowMoney)
        {
            gbConfigUser.Text = name;
            tbConfigMoney.Text = nowMoney.ToString();
            this.selectedUserToken = selectedUserToken;
        }

        private void BtnConfigSet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbConfigMoney.Text, out int money))
            {
                OnAcceptUserMoney(selectedUserToken, money);
            }
            selectedUserToken = "";
            Hide();
        }

        private void BtnConfigCancel_Click(object sender, EventArgs e)
        {
            selectedUserToken = "";
            Hide();
        }

        private void TbConfigMoney_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;

            if (!char.IsDigit(number))
            {
                e.Handled = true;
            }

        }

        private void AddValue(int count)
        {
            if (int.TryParse(tbConfigMoney.Text, out int money))
            {
                tbConfigMoney.Text = (money + count).ToString();
            }
        }

        private void BtnM100_Click(object sender, EventArgs e)
        {
            AddValue(-100);
        }

        private void BtnM500_Click(object sender, EventArgs e)
        {
            AddValue(-500);
        }

        private void BtnM1000_Click(object sender, EventArgs e)
        {
            AddValue(-1000);
        }

        private void BtnP100_Click(object sender, EventArgs e)
        {
            AddValue(100);
        }

        private void BtnP500_Click(object sender, EventArgs e)
        {
            AddValue(500);
        }

        private void BtnP1000_Click(object sender, EventArgs e)
        {
            AddValue(1000);
        }
    }
}
