
using System.Windows.Forms;

namespace SvoyaIgra.Extensions
{
    public static class ControlCollectionExtensions
    {
        public static void SetOnTop(this Control.ControlCollection collection, Control bottom, Control upper)
        {
            var index = collection.GetChildIndex(bottom) - 1;
            collection.SetChildIndex(upper, index);
        }
    }


}
