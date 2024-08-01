using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvoyaIgra.Utils
{
    public static class DrawUtils
    {
        public static Image GenerateShowText(string text, bool isOnOneScreen, Size size, Font font, Color color)
        {
            int countLines = CountLine(text);

            int dy;

            if (isOnOneScreen)
            {
                dy = (int)((float)size.Height / countLines);
            }
            else
            {
                dy = (int)(size.Height * 0.15f);
            }

            if (countLines == 1)
            {
                dy = size.Height;
            }

            int testHeight = dy * countLines;

            var bmp = new Bitmap(size.Width, Math.Max(size.Height, testHeight));
            var g = Graphics.FromImage(bmp);

            g.Clear(Color.Empty);

            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            Brush brush = new SolidBrush(color);

            var splitText = text.Split('\n');


            for (int id = 0; id < countLines; id++)
            {
                Rectangle rect = new Rectangle(0, id * dy, bmp.Width, dy);

                var printText = splitText[id];

                var bufFont = GetAdjustedFont(g, printText, font, new SizeF(rect.Width, rect.Height), 40, 5);

                g.DrawString(printText, bufFont, brush, rect, stringFormat);
            }

            return bmp;
        }

        private static int CountLine(string str)
        {
            int count = 0;

            foreach (var ch in str)
                if (ch == '\n') count++;

            return count + 1;
        }

        public static Font GetAdjustedFont(Graphics g, string graphicString, Font originalFont, SizeF layout, int maxFontSize, int minFontSize)
        {
            Font testFont = null;
            for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
            {
                testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                SizeF adjustedSizeNew = g.MeasureString(graphicString, testFont, (int)layout.Width);

                if (layout.Width > Convert.ToInt32(adjustedSizeNew.Width) && layout.Height > Convert.ToInt32(adjustedSizeNew.Height))
                {
                    return testFont;
                }
            }
            return testFont;
        }

    }
}
