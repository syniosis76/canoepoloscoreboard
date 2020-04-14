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

        private static Object GetItem(ItemCollection list, string value)
        {
            foreach (ListBoxItem item in list)
            {
                if (item.Tag != null && (string)item.Tag == value)
                {
                    return item;
                }
                else if (item.Content is String && (string)item.Content == value)
                {
                    return item;
                }
            }

            return null;
        }

        public static bool SelectCard(Window owner, Game game, ref string team, ref string card, ref string player, ref string infringement, ref string penaltyDuration)
        {
            Cards cards = new Cards();
            cards.Owner = owner;
            cards._teamComboBox.Items.Add(game.Team1);
            cards._teamComboBox.Items.Add(game.Team2);
            if (!String.IsNullOrWhiteSpace(team)) cards._teamComboBox.SelectedItem = team; // GetItem(cards._teamComboBox.Items, team);
            if (!String.IsNullOrWhiteSpace(card)) cards._cardsList.SelectedItem = GetItem(cards._cardsList.Items, card);
            if (!String.IsNullOrWhiteSpace(player)) cards._playersList.SelectedItem = GetItem(cards._playersList.Items, player);
            if (!String.IsNullOrWhiteSpace(infringement)) cards._infringementList.SelectedItem = GetItem(cards._infringementList.Items, infringement);            

            if (cards.ShowDialog() == true && cards.CardSelected)
            {
                card = cards.Card;
                player = cards.Player;
                infringement = cards.Infringement;
                penaltyDuration = cards.PenaltyDuration;
                team = (string)cards._teamComboBox.SelectedItem;
                return true;
            }
            else
            {
                card = string.Empty;
                player = string.Empty;
                infringement = string.Empty;
                penaltyDuration = string.Empty;
                team = string.Empty;
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

