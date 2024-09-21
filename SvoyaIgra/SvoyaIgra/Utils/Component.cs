using System;
using System.Windows.Forms;

namespace SvoyaIgra.Utils
{
    public class Component
    {
        private readonly Control control;

        private readonly int baseWidth;

        private readonly int baseHeight;

        private readonly System.Drawing.PointF basePositonCenter;

        private readonly bool correctWidth;
        private readonly bool correctHeight;
        private readonly bool correctPosition;

        public Component(Control control, bool correctWidth, bool correctHeight, bool correctPosition, System.Drawing.Size baseParentSize)
        {
            this.control = control;
            baseWidth = control.Size.Width;
            baseHeight = control.Size.Height;

            this.correctWidth = correctWidth;
            this.correctHeight = correctHeight;
            this.correctPosition = correctPosition;

            basePositonCenter = new System.Drawing.PointF(
               (control.Left + control.Right) / 2.0f / baseParentSize.Width,
               (control.Top + control.Bottom) / 2.0f / baseParentSize.Height);
        }

        public void Resize(System.Drawing.Size newParentSize, float dw, float dh)
        {
            int newWidth = correctWidth ? (int)Math.Round(dw * baseWidth) : control.Size.Width;
            int newHeight = correctHeight ? (int)Math.Round(dh * baseHeight) : control.Size.Height;

            control.Size = new System.Drawing.Size(newWidth, newHeight);

            if (correctPosition)
            {
                control.Left = (int)(newParentSize.Width * basePositonCenter.X - newWidth / 2);
                control.Top = (int)(newParentSize.Height * basePositonCenter.Y - newHeight / 2);
            }
        }

    }
}
