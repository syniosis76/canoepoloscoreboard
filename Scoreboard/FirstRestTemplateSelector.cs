using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Scoreboard
{
    public class FirstRestTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FirstTemplate { get; set; }
        public DataTemplate RestTemplate { get; set; }

        public bool IsFirstItem(object item, DependencyObject container)
        {
            DependencyObject parent = container;
            ListView listView= null;
            while (parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is ListView)
                {
                    listView = parent as ListView;
                    break;
                }
            }

            if (listView != null)
            {
                object firstObject = null;
                foreach (object listObject in listView.ItemsSource)
                {
                    firstObject = listObject;
                    break;
                }
                return item == firstObject;
            }
            
            return false;            
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (IsFirstItem(item, container))
            {
                return FirstTemplate;
            }
            else
            {
                return RestTemplate;
            }
        }
    }
}
