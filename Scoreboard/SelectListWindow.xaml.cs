using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;

namespace Scoreboard
{
    public class SelectItem
    {
        public string Id { get; set; }
        public string Name { get; set; }        

        public SelectItem(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Interaction logic for SelectListWindow.xaml
    /// </summary>
    public partial class SelectListWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion  

        private BindingList<SelectItem> _items = new BindingList<SelectItem>();
        public BindingList<SelectItem> Items
        {
            get { return _items; }
        }

        private string _selectedId;
        public string SelectedId
        {
            get { return _selectedId; }
            set
            {
                _selectedId = value;
                NotifyPropertyChanged("SelectedId");
            }
        }

        public SelectListWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;           
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void _list_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogResult = true;
        }
    }
}
