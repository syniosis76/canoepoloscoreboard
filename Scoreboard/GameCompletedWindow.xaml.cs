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
using System.ComponentModel;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for AddPeriodWindow.xaml
    /// </summary>
    public partial class GameCompletedWindow : Window, INotifyPropertyChanged
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
        public Score Score { get { return _score; } }
            
        private Game _completedGame;
        public Game CompletedGame
        {
            get { return _completedGame; }
            set
            {
                _completedGame = value;
                if (_completedGame.Parent != null)
                {
                    _score = _completedGame.Parent.Parent;
                }
                else
                {
                    _score = null;
                }
                NotifyPropertyChanged("CompletedGame");
                NotifyPropertyChanged("Score");
            }
        }

        private Game _nextGame;
        public Game NextGame
        {
            get { return _nextGame; }
            set
            {
                _nextGame = value;
                NotifyPropertyChanged("NextGame");
                NotifyPropertyChanged("NextGameIsNotNull");
            }
        }

        public bool NextGameIsNull
        {
            get
            {
                return NextGame == null;
            }
        }

        public GameCompletedWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private DateTime _closeAt;
        public int? CloseIn
        {
            get
            {
                if (_closeAt == null)
                {
                    return null;
                }
                else
                {
                    return (int)(_closeAt - DateTime.Now).TotalSeconds;
                }
            }
        }

        protected void SetupCloseAt()
        {
            int closeIn = 90;
            if (NextGame != null && NextGame.StartsIn != null)
            {
                closeIn = Math.Min(closeIn, (int)(NextGame.StartsIn.Value.TotalSeconds / 2));
            }
            _closeAt = DateTime.Now.AddSeconds(closeIn);
        }      

        private static GameCompletedWindow _staticWindow;

        public static void ShowGameCompletedWindow(Window owner, Game completedGame, Game nextGame)
        {
            _staticWindow = new GameCompletedWindow();
            _staticWindow.Owner = owner;
            _staticWindow.CompletedGame = completedGame;
            _staticWindow.NextGame = nextGame;
            _staticWindow.SetupCloseAt();
            _staticWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _staticWindow = null;
        }

        public static void StaticTick()
        {
            if (_staticWindow != null)
            {
                _staticWindow.Tick();
            }
        }

        public void Tick()
        {
            NotifyPropertyChanged("CloseIn");
            if (CloseIn == 0)
            {
                Close();
            }
        }

        public DateTime ParseTime(string time)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (!String.IsNullOrEmpty(time))
            {
                string[] parts = time.Split(':');

                if (parts.Length > 0)
                {
                    hours = int.Parse(parts[0]);
                }

                if (parts.Length > 1)
                {
                    minutes = int.Parse(parts[1]);
                }

                if (parts.Length > 2)
                {
                    seconds = int.Parse(parts[2]);
                }
            }

            return DateTime.Today + new TimeSpan(hours, minutes, seconds);
        }       

        private void StartExtraPeriodClick(object sender, RoutedEventArgs e)
        {
            AddExtraPeriod();         
        }

        private void StartNextGameNowClick(object sender, RoutedEventArgs e)
        {
            StartNextGame(0);
        }

        private void StartNextGame30Click(object sender, RoutedEventArgs e)
        {
            StartNextGame(30);
        }

        private void StartNextGame60Click(object sender, RoutedEventArgs e)
        {
            StartNextGame(60);
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected void AddExtraPeriod()
        {
            if (Score != null)
            {
                Score.AddExtraPeriod(CompletedGame, 60 * 5);
            }
            Close();            
        }

        protected void StartNextGame(int startInSeconds)
        {
            if (Score != null)
            {
                Score.StartGameInSeconds(NextGame, startInSeconds);
            }
            Close();
        }        
    }
}

