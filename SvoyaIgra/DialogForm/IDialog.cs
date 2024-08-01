using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogForm
{
    public interface IDialog
    {
        Utils.DialogResult Result { get; }

    }
}
