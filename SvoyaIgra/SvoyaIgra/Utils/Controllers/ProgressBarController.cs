using System.Drawing;
using System.Windows.Forms;

namespace SvoyaIgra.Utils.Controllers
{
    public class ProgressBarController
    {
        private readonly Brush brush;

        private readonly PictureBox pictureBox;

        private float value;

        private float maxValue;

        public ProgressBarController(PictureBox pictureBox, Color color)
        {
            this.brush = new SolidBrush(color);
            this.pictureBox = pictureBox;
            pictureBox.Paint += Paint;
        }

        private void Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (value != 0)
            {
                var size = pictureBox.Size;
                e.Graphics.FillRectangle(brush, 0, 0, size.Width * (value / maxValue), size.Height);
            }
        }

        public void SetValue(float value)
        {
            this.value = value;
            pictureBox.Refresh();
        }

        public void Resize()
        {
            pictureBox.Refresh();
        }

        public void SetMaxValue(float value)
        {
            this.maxValue = value;
        }

    }

}
