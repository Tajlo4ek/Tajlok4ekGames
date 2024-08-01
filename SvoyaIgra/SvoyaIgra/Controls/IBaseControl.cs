using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvoyaIgra.Controls
{
    interface IBaseControl
    {
        void AddToSizeControl(ref Utils.Controllers.SizeController sizeController);
    }
}
