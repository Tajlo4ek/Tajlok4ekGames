using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Editor.MyControl.CustomTabControl
{
    public class ActionTabViewModal
    {
        // These Are the tabs that will be bound to the TabControl 
        private readonly ObservableCollection<ActionTabItem> tabs;

        public int TabCount
        {
            get { return tabs.Count; }
        }

        public ActionTabViewModal()
        {
            tabs = new ObservableCollection<ActionTabItem>();
        }

        public string Add(TextBlock header, UserControl userControl)
        {
            var tab = new ActionTabItem { Header = header, Content = userControl };
            tabs.Add(tab);
            return tab.Id;
        }

        public void Bind(TabControl tabControl)
        {
            tabControl.ItemsSource = tabs;
        }

        public void Remove(string id)
        {
            foreach (var tab in tabs)
            {
                if (tab.Id.Equals(id))
                {
                    tabs.Remove(tab);
                    return;
                }
            }
        }

        public UserControl GetUserControl(string id)
        {
            foreach (var tab in tabs)
            {
                if (tab.Id.Equals(id))
                {
                    return tab.Content;
                }
            }
            return null;
        }

        public List<UserControl> GetAllUserControls()
        {
            List<UserControl> res = new List<UserControl>();

            foreach (var tab in tabs)
            {
                res.Add(tab.Content);
            }

            return res;
        }

    }

}
