using System;
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
                NotifyPropertyChanged("IntervalDuration");
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

        private bool _removeExistingGames;
        public bool RemoveExistingGames
        {
            get { return _removeExistingGames; }
            set
            {
                _removeExistingGames = value;
                NotifyPropertyChanged("RemoveExistingGames");
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
            AddGamesWindow windows = new AddGamesWindow();
            windows.Owner = owner;
            windows.Score = score;
            windows.NewGames = newGames;
            return windows.ShowDialog() == true ? true : false;
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

            int numberOfPeriods = 2;
            int.TryParse(NumberOfPeriods, out numberOfPeriods);
            TimeSpan periodDuration = Score.ParseTimeSpan(PeriodDuration);
            TimeSpan intervalDuration = Score.ParseTimeSpan(IntervalDuration);

            if (periodDuration.TotalSeconds > 0)
            {
                Game game = new Game(pool, team1, team2, null);
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

        private void _gamesList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteGames();
            }
        }

        private void _textBoxKeyUp(object sender, KeyEventArgs e)
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

        private void WriteSettings()
        {
            Properties.Settings.Default.StartTime = StartTime;
            Properties.Settings.Default.GameDuration = GameDuration;
            Properties.Settings.Default.NumberOfPeriods = NumberOfPeriods;
            Properties.Settings.Default.PeriodDuration = PeriodDuration;
            Properties.Settings.Default.IntervalDuration = IntervalDuration;
            Properties.Settings.Default.RemoveExistingGames = RemoveExistingGames;
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
