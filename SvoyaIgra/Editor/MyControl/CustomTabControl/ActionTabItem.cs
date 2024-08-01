using System;
using System.Windows.Controls;

namespace Editor.MyControl.CustomTabControl
{
    public class ActionTabItem
    {
        public ActionTabItem()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; private set; }

        public TextBlock Header { get; set; }

        public UserControl Content { get; set; }
    }
}
