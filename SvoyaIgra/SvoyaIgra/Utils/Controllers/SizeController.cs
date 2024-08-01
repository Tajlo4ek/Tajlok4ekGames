using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SvoyaIgra.Utils.Controllers
{
    public class SizeController
    {
        private readonly Size baseSize;

        private readonly List<Component> controls;

        public enum CorrectSizeType
        {
            All,
            WidthOnly,
            HeightOnly,
            Nothing,
        }

        public SizeController(Size size)
        {
            baseSize = size;

            controls = new List<Component>();
        }

        public void AddControl(Control control, CorrectSizeType correctType = CorrectSizeType.All, bool correctPosition = true)
        {
            bool correctWidth = (correctType == CorrectSizeType.WidthOnly || correctType == CorrectSizeType.All);
            bool correctHeight = (correctType == CorrectSizeType.HeightOnly || correctType == CorrectSizeType.All);

            controls.Add(new Component(control, correctWidth, correctHeight, correctPosition, baseSize));
        }

        public void ResizeAll(Size size)
        {
            var dx = (float)size.Width / baseSize.Width;
            var dy = (float)size.Height / baseSize.Height;

            foreach (var component in controls)
            {
                component.Resize(size, dx, dy);
            }
        }

    }
}
