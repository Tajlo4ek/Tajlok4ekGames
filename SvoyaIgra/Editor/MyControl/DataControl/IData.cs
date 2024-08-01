using DataStore;
using System.Windows.Controls;

namespace Editor.MyControl.DataControl
{
    public interface IData
    {
        Scenario GetData();

        Control GetControl();

        void Clear();

        void Parse(Scenario scenario);

    }
}
