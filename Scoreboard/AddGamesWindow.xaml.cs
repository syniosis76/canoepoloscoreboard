﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for AddGamesWindow.xaml
    /// </summary>
    public partial class AddGamesWindow : Window, INotifyPropertyChanged
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
    
        private Score _score;
        public Score Score
        {
            get { return _score; }
            set
            {
                _score = value;                
                NotifyPropertyChanged("Score");
            }
        }

        private GameList _newGames;
        public GameList NewGames
        {
            get { return _newGames; }
            set
            {
                _newGames = value;
                NotifyPropertyChanged("NewGames");
            }
        }

        private string _startTime;
        public string StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                Properties.Settings.Default.StartTime = _startTime;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("StartTime");
            }
        }

        private string _gameDuration;
        public string GameDuration
        {
            get { return _gameDuration; }
            set
            {
                _gameDuration = value;
                Properties.Settings.Default.GameDuration = _gameDuration;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("GameDuration");
            }
        }

        private string _numberOfPeriods;
        public string NumberOfPeriods
        {
            get { return _numberOfPeriods; }
            set
            {
                _numberOfPeriods = value;
                Properties.Settings.Default.NumberOfPeriods = _numberOfPeriods = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("NumberOfPeriods");
            }
        }

        private string _periodDuration;
        public string PeriodDuration
        {
            get { return _periodDuration; }
            set
            {
                _periodDuration = value;
                Properties.Settings.Default.PeriodDuration = _periodDuration;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("PeriodDuration");
            }
        }

        private string _intervalDuration;
        public string IntervalDuration
        {
            get { return _intervalDuration; }
            set
            {
                _intervalDuration = value;
                Properties.Settings.Default.IntervalDuration = _intervalDuration;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("IntervalDuration");
            }
        }

        private bool _removeExistingGames;
        public bool RemoveExistingGames
        {
            get { return _removeExistingGames; }
            set
            {
                _removeExistingGames = value;
                Properties.Settings.Default.RemoveExistingGames = _removeExistingGames;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("RemoveExistingGames");
            }
        }

        private string _pool;
        public string Pool
        {
            get { return _pool; }
            set
            {
                _pool = value;
                NotifyPropertyChanged("Pool");
            }
        }

        private string _team1;
        public string Team1
        {
            get { return _team1; }
            set
            {
                _team1 = value;
                NotifyPropertyChanged("Team1");
            }
        }

        private string _team2;
        public string Team2
        {
            get { return _team2; }
            set
            {
                _team2 = value;
                NotifyPropertyChanged("Team2");
            }
        }
        
        public AddGamesWindow()
        {
            InitializeComponent();
            DataContext = this;

            ReadSettings();
        }

        public static bool AddGames(Window owner, Score score, GameList newGames)
        {
            AddGamesWindow windows = new AddGamesWindow
            {
                Owner = owner,
                Score = score,
                NewGames = newGames
            };
            return windows.ShowDialog() ?? false;
        }

        public void AddGame()
        {
            _team1TextBox.Focus();
            
            string gameText = Team1 + "\t" + Team2 + (String.IsNullOrEmpty(Pool) ? String.Empty : "\t" + Pool);

            AddGamesFromText(gameText.ToString());
            
            Team1 = String.Empty;
            Team2 = String.Empty;
            Pool = String.Empty;            
        }

        public void PasteGames()
        {
            if (Clipboard.ContainsText())
            {
                AddGamesFromText(Clipboard.GetText());
            }
        }

        public void AddGame(string pool, string team1, string team2)
        {
            DateTime startTime = Score.ParseTime(StartTime);
            TimeSpan gameDuration = Score.ParseTimeSpan(GameDuration);

            if (!int.TryParse(NumberOfPeriods, out int numberOfPeriods))
            {
                numberOfPeriods = 2;
            }

            TimeSpan periodDuration = Score.ParseTimeSpan(PeriodDuration);
            TimeSpan intervalDuration = Score.ParseTimeSpan(IntervalDuration);

            if (periodDuration.TotalSeconds > 0)
            {
                string team1Flag = String.Empty;
                string team2Flag = String.Empty;

                if (team1.Contains("|"))
                {
                    team1Flag = team1.Split('|')[0].ToLower();
                    team1 = team1.Split('|')[1];
                }
                if (team2.Contains("|"))
                {
                    team2Flag = team2.Split('|')[0].ToLower();
                    team2 = team2.Split('|')[1];
                }
                Game game = new Game(pool, team1, team2, null)
                {
                    Team1Flag = team1Flag,
                    Team2Flag = team2Flag
                };

                DateTime periodTime = startTime;              

                for (int periodNumber = 1; periodNumber <= numberOfPeriods; periodNumber++)
                {
                    string periodName = "Period " + periodNumber.ToString();
                    game.Periods.AddPeriod(periodName, periodTime, periodTime + periodDuration);
                    periodTime += periodDuration;
                    if (intervalDuration.TotalSeconds > 0) periodTime += intervalDuration;
                }

                NewGames.Add(game);
            }

            // Setup StartTime for the next game
            DateTime endTime = startTime + gameDuration;
            StartTime = endTime.ToString("HH:mm:ss");
        }

        public void AddGamesFromText(string text)
        {
            char splitChar = ',';

            if (text.Contains('\t'))
            {
                splitChar = '\t';
            }

            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    if (parts[0].Equals("#"))
                    {
                        SetTemplateFromTextV1(parts);
                    }
                    else if (parts[0].Equals("#2"))
                    {
                        SetTemplateFromTextV2(parts);
                    }
                    else
                    {
                        AddGameFromText(parts);
                    }                                    
                }
            }
        }

        public void SetTemplateFromTextV1(string[] parts)
        {
            StartTime = parts.Length > 1 ? parts[1] : null;
            GameDuration = parts.Length > 2 ? parts[2] : null;
            NumberOfPeriods = "2"; // Default to 2 periods
            PeriodDuration = parts.Length > 3 ? parts[3] : null;
            IntervalDuration = parts.Length > 4 ? parts[4] : null;            
        }

        public void SetTemplateFromTextV2(string[] parts)
        {
            StartTime = parts.Length > 1 ? parts[1] : null;
            GameDuration = parts.Length > 2 ? parts[2] : null;
            NumberOfPeriods = parts.Length > 3 ? parts[3] : null;
            PeriodDuration = parts.Length > 4 ? parts[4] : null;
            IntervalDuration = parts.Length > 5 ? parts[5] : null;
        }

        public void AddGameFromText(string[] parts)
        {            
            string team1 = string.Empty;
            string team2 = string.Empty;
            string pool = string.Empty;
           
            if (parts.Length >= 2)
            {
                team1 = parts[0];
                team2 = parts[1];
                if (parts.Length >= 3)
                {
                    pool = parts[2];
                }
            }

            if (!String.IsNullOrEmpty(team1) && !String.IsNullOrEmpty(team2))
            {
                AddGame(pool, team1, team2);
            }
        }        

        private void AddGameButtonClick(object sender, RoutedEventArgs e)
        {
            AddGame();
        }

        private void PasteGamesButtonClick(object sender, RoutedEventArgs e)
        {
            PasteGames();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (RemoveExistingGames)
            {
                Score.RemoveAllGames();
            }
            DialogResult = true;
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GamesList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteGames();
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddGame();
            }
        }        

        private void ReadSettings()
        {
            StartTime = DateTime.Now.AddSeconds(70).ToString("HH:mm");          
            GameDuration = Properties.Settings.Default.GameDuration;
            NumberOfPeriods = Properties.Settings.Default.NumberOfPeriods;
            PeriodDuration = Properties.Settings.Default.PeriodDuration;
            IntervalDuration = Properties.Settings.Default.IntervalDuration;            
            RemoveExistingGames = Properties.Settings.Default.RemoveExistingGames;            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
