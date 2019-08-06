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

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for Players.xaml
    /// </summary>
    public partial class Players : Window
    {
        public const String Unknown = "Unkown";

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

        public Players()
        {
            InitializeComponent();
            _playersList.SelectedIndex = 0;
            _playersList.Focus();
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

        public static bool SelectPlayer(Window owner, Game game, ref string team, ref string player)
        {
            Players players = new Players();
            players.Owner = owner;

            players._teamComboBox.Items.Add(game.Team1);
            players._teamComboBox.Items.Add(game.Team2);

            if (!String.IsNullOrWhiteSpace(team)) players._teamComboBox.SelectedItem = team; 
            if (!String.IsNullOrWhiteSpace(player)) players._playersList.SelectedItem = GetItem(players._playersList.Items, player);

            if (players.ShowDialog() == true)
            {
                team = (string)players._teamComboBox.SelectedItem;
                player = players.Player;
                return true;
            }
            else
            {
                team = string.Empty;
                player = String.Empty;
                return false;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
            {
                ChoosePlayer(1);
            }
            else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
            {
                ChoosePlayer(2);
            }
            else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
            {
                ChoosePlayer(3);
            }
            else if (e.Key == Key.D4 || e.Key == Key.NumPad4)
            {
                ChoosePlayer(4);
            }
            else if (e.Key == Key.D5 || e.Key == Key.NumPad5)
            {
                ChoosePlayer(5);
            }
            else if (e.Key == Key.D6 || e.Key == Key.NumPad6)
            {
                ChoosePlayer(6);
            }
            else if (e.Key == Key.D7 || e.Key == Key.NumPad7)
            {
                ChoosePlayer(7);
            }
            else if (e.Key == Key.D8 || e.Key == Key.NumPad8)
            {
                ChoosePlayer(8);
            }
            else if (e.Key == Key.D9 || e.Key == Key.NumPad9)
            {
                ChoosePlayer(9);
            }
            else if (e.Key == Key.D0 || e.Key == Key.NumPad0)
            {
                ChoosePlayer(10);
            }
        }       

        private void ChoosePlayer(int value)
        {
            _playersList.SelectedIndex = value;
            DialogResult = true;
        }        

        private void _okButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
        }
    }
}
