using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.ComponentModel;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for Cards.xaml
    /// </summary>
    public partial class Cards : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion

        public bool CardSelected
        {
            get
            {
                return !String.IsNullOrEmpty(Card);
            }
        }

        public string CardDescription
        {
            get
            {
                if (CardSelected)
                {
                    string result = Card;
                    if (!String.IsNullOrEmpty(Player))
                    {
                        result += "," + Player;
                    }
                    if (!String.IsNullOrEmpty(Infringement))
                    {
                        result += "," + Infringement;
                    }
                    return result;
                }
                else
                {
                    return String.Empty;
                }
            }
        }
        
        public string Card
        {
            get
            {
                ListBoxItem item = (ListBoxItem)_cardsList.SelectedItem;
                if (item != null)
                {
                    return item.Tag.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Player
        {
            get
            {
                ListBoxItem item = (ListBoxItem)_playersList.SelectedItem;
                if (item != null)
                {
                    return item.Content.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Infringement
        {
            get
            {
                ListBoxItem item = (ListBoxItem)_infringementList.SelectedItem;
                if (item != null)
                {
                    return item.Content.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private string _penaltyDuration;
        public string PenaltyDuration
        {
            get { return _penaltyDuration;  }
            set
            {
                if (_penaltyDuration != value)
                {
                    _penaltyDuration = value;
                    NotifyPropertyChanged("PenaltyDuration");
                }
            }
        }

        public Cards()
        {
            InitializeComponent();

            DataContext = this;

            ReadSettings();

            _cardsList.SelectedIndex = 0;
            _cardsList.Focus();            
        }

        public static bool SelectCard(Window owner, Game game, string team, out string card, out string player, out string infringement, out string penaltyDuration, out string selectedTeam)
        {
            Cards cards = new Cards();
            cards.Owner = owner;
            cards._teamComboBox.Items.Add(game.Team1);
            cards._teamComboBox.Items.Add(game.Team2);
            cards._teamComboBox.SelectedItem = team;
            if (cards.ShowDialog() == true && cards.CardSelected)
            {
                card = cards.Card;
                player = cards.Player;
                infringement = cards.Infringement;
                penaltyDuration = cards.PenaltyDuration;
                selectedTeam = (string)cards._teamComboBox.SelectedItem;
                return true;
            }
            else
            {
                card = string.Empty;
                player = string.Empty;
                infringement = string.Empty;
                penaltyDuration = string.Empty;
                selectedTeam = string.Empty;
                return false;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.G)
            {
                _cardsList.SelectedIndex = 0;
            }
            else if (e.Key == Key.Y)
            {
                _cardsList.SelectedIndex = 1;
            }
            else if (e.Key == Key.R)
            {
                _cardsList.SelectedIndex = 2;
            }
        }

        private void _okButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
        }

        private void ReadSettings()
        {            
            PenaltyDuration = Properties.Settings.Default.PenaltyDuration;
        }

        private void WriteSettings()
        {
            Properties.Settings.Default.PenaltyDuration = PenaltyDuration;
            Properties.Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DialogResult == true)
            {
                WriteSettings();
            }
        }        
    }
}

