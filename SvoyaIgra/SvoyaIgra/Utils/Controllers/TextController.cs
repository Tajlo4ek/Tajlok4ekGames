using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SvoyaIgra.Utils.Controllers
{
    public class TextController
    {
        private static readonly Font mainFont = new Font("Arial", 40, FontStyle.Regular, GraphicsUnit.Point);

        private string text = "";

        private readonly Utils.Controllers.ImageController imageController;

        public TextController(PictureBox pictureBox, Color color)
        {
            imageController = new Utils.Controllers.ImageController(
                pictureBox,
                new Timer(),
                () => { },
                () => Utils.DrawUtils.GenerateShowText(text, true, pictureBox.Size, mainFont, color)
            );
            imageController.ShowImg(Utils.DrawUtils.GenerateShowText("", true, pictureBox.Size, mainFont, color), -1);
        }

        public void SetText(string text)
        {
            this.text = text;
            imageController.Resize();
        }

        public void Resize()
        {
            imageController.Resize();
        }

        public void Dispose()
        {
            imageController.Dispose();
            mainFont.Dispose();
        }

    }
}
