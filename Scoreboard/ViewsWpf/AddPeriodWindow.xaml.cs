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
    public partial class AddPeriodWindow : Window, INotifyPropertyChanged
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
    
        private Game _game;
        public Game Game
        {
            get { return _game; }
            set
            {
                _game = value;                
                NotifyPropertyChanged("Game");
            }
        }

        private string _extraPeriodName;
        public string ExtraPeriodName
        {
            get { return _extraPeriodName; }
            set
            {
                _extraPeriodName = value;
                NotifyPropertyChanged("ExtraPeriodName");
            }
        }

        private string _extraPeriodDuration;
        public string ExtraPeriodDuration
        {
            get { return _extraPeriodDuration; }
            set
            {
                _extraPeriodDuration = value;
                NotifyPropertyChanged("ExtraPeriodDuration");
            }
        }        

        public AddPeriodWindow()
        {
            InitializeComponent();
            DataContext = this;

            ReadSettings();
            _periodDurationTextBox.Focus();
        }

        public static bool AddPeriod(Window owner, Game game)
        {
            AddPeriodWindow windows = new AddPeriodWindow();
            windows.Owner = owner;
            windows.Game = game;
            return windows.ShowDialog() == true ? true : false;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            AddExtraPeriod();

            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected void AddExtraPeriod()
        {
            DateTime startTime = DateTime.Now;
            TimeSpan extraPeriodDuration = Score.ParseTimeSpan(ExtraPeriodDuration);

            GamePeriod gamePeriod = new GamePeriod();
            gamePeriod.IsExtraPeriod = true;
            gamePeriod.Name = ExtraPeriodName;
            gamePeriod.StartTime = startTime;
            gamePeriod.EndTime = startTime + extraPeriodDuration;
            Game.Periods.Add(gamePeriod);

            gamePeriod.ModifyFollowingTimes(gamePeriod.EndTime - DateTime.Now, true);
        }

        private void ReadSettings()
        {
            ExtraPeriodName = Properties.Settings.Default.ExtraPeriodName;
            ExtraPeriodDuration = "3:00"; // Properties.Settings.Default.ExtraPeriodDuration;
        }

        private void WriteSettings()
        {
            Properties.Settings.Default.ExtraPeriodName = ExtraPeriodName;
            Properties.Settings.Default.ExtraPeriodDuration = ExtraPeriodDuration;
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

