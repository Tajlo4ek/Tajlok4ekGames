using System;
using System.Windows.Forms;

namespace SvoyaIgra.Controls
{
    public partial class AuctionControl : UserControl
    {
        private readonly int step;
        public Action<int> AuctionMove;

        public AuctionControl(int step)
        {
            InitializeComponent();

            this.step = step;


            trBarAuctionRate.SmallChange = step;
            trBarAuctionRate.LargeChange = step;
            trBarAuctionRate.TickFrequency = step;
        }

        private void BtnAuctionPass_Click(object sender, EventArgs e)
        {
            AuctionMove(0);
        }

        private void BtnAuctionAllIn_Click(object sender, EventArgs e)
        {
            AuctionMove(-1);
        }

        private void BtnAuctionSet_Click(object sender, EventArgs e)
        {
            AuctionMove(trBarAuctionRate.Value * step);
        }

        private void TrBarAuctionRate_ValueChanged(object sender, EventArgs e)
        {
            tbAuctionRate.Text = (trBarAuctionRate.Value * step).ToString();
        }

        public void ShowAuction(int minValue, int maxValue, bool canPass, bool canAllIn, bool canSet)
        {
            trBarAuctionRate.Minimum = (int)Math.Round((float)minValue / step);
            tbAuctionRate.Text = (trBarAuctionRate.Minimum * step).ToString();

            if (canAllIn && !canSet)
            {
                trBarAuctionRate.Minimum = maxValue;
                trBarAuctionRate.Maximum = maxValue;
                tbAuctionRate.Text = trBarAuctionRate.Minimum.ToString();
            }
            else
            {
                if (minValue < maxValue)
                {
                    trBarAuctionRate.Maximum = maxValue / step;
                }
                else if (minValue == maxValue)
                {
                    trBarAuctionRate.Minimum = minValue;
                    trBarAuctionRate.Maximum = minValue;
                    tbAuctionRate.Text = trBarAuctionRate.Minimum.ToString();
                }
                else
                {
                    trBarAuctionRate.Maximum = trBarAuctionRate.Minimum;
                }
            }


            trBarAuctionRate.Value = trBarAuctionRate.Minimum;

            btnAuctionAllIn.Enabled = canAllIn;
            btnAuctionPass.Enabled = canPass;
            btnAuctionSet.Enabled = canSet;
            trBarAuctionRate.Enabled = canSet;
        }
    }
}
