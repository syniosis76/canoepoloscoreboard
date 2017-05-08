using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace Scoreboard
{    
    public class Game : INotifyPropertyChanged
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

        #region Parent

        private GameList _parent;
        [System.Xml.Serialization.XmlIgnore]
        public GameList Parent
        {
            get { return _parent; }
        }

        public void SetParent(GameList parent)
        {
            _parent = parent;
            Periods.SetParent(this);
        }

        #endregion

        public void SaveGames()
        {
            if (Parent != null)
            {
                Parent.SaveGames();
            }
        }

        public void ModifyTimes(TimeSpan changeBy)
        {
            Periods.ModifyTimes(changeBy);
        }

        public void ModifyFollowingTimes(TimeSpan changeBy)
        {
            if (Parent != null)
            {
                Parent.ModifyFollowingTimes(this, changeBy);
            }
        }

        public bool IsCurrentGame
        {
            get
            {
                return Parent != null ? Parent.CurrentGame == this : false;
            }
            set
            {
                NotifyPropertyChanged("IsCurrentGame");
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

        private string _team1Color;
        public string Team1Color
        {
            get { return _team1Color; }
            set
            {
                _team1Color = value;
                NotifyPropertyChanged("Team1Color");
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

        private string _team2Color;
        public string Team2Color
        {
            get { return _team2Color; }
            set
            {
                _team2Color = value;
                NotifyPropertyChanged("Team2Color");
            }
        }

        private int _team1Score;
        public int Team1Score
        {
            get { return _team1Score; }
            set
            {
                _team1Score = value;
                NotifyPropertyChanged("Team1Score");
            }
        }

        private int _team2Score;
        public int Team2Score
        {
            get { return _team2Score; }
            set
            {
                _team2Score = value;
                NotifyPropertyChanged("Team2Score");
            }
        }

        private GamePeriodList _periods = new GamePeriodList();
        public GamePeriodList Periods
        {
            get
            {
                return _periods;
            }
        }

        public DateTime? StartTime
        {
            get
            {
                if (Periods.Count > 0)
                {
                    return Periods.StartTime;
                }
                else
                {
                    return null;
                }
            }
        }       

        public DateTime? EndTime
        {
            get
            {
                if (Periods.Count > 0)
                {
                    return Periods.Last().EndTime;
                }
                return null;
            }
        }

        private GameResult _result = GameResult.None;
        public GameResult Result
        {
            get { return _result; }
            set
            {
                if (!_result.Equals(value))
                {
                    _result = value;
                    NotifyPropertyChanged("Result");
                    NotifyPropertyChanged("IsDraw");
                }
            }
        }

        public string ResultDescription
        {
            get
            {
                if (Result == GameResult.Draw) return "Draw";
                else if (Result == GameResult.Team1) return Team1 + " Won";
                else if (Result == GameResult.Team2) return Team2 + " Won";
                else return "None";
            }
        }

        public int Team1Points
        {
            get
            {
                if (Result == GameResult.Draw) return 1;
                else if (Result == GameResult.Team1) return 3;
                else if (Result == GameResult.Team2) return 0;
                else return 0;
            }
        }

        public int Team2Points
        {
            get
            {
                if (Result == GameResult.Draw) return 1;
                else if (Result == GameResult.Team1) return 0;
                else if (Result == GameResult.Team2) return 3;
                else return 0;
            }
        }

        public Boolean IsDraw { get { return Result == GameResult.Draw; } }

        public GameResult CalculateResult()
        {
            if (HasEnded)
            {
                if (Team1Score > Team2Score)
                {
                    Result = GameResult.Team1;
                }
                else if (Team2Score > Team1Score)
                {
                    Result = GameResult.Team2;
                }
                else
                {
                    Result = GameResult.Draw;
                }
            }
            else
            {
                Result = GameResult.None;
            }

            return Result;
        }

        public void ClearStatus()
        {
            Team1Score = 0;
            Team2Score = 0;
            Result = GameResult.None;
            GameEvents.Clear();
        }

        public void ResetGame()
        {
            Periods.ResetPeriod();
        }

        private bool _hasStarted = false;
        [System.Xml.Serialization.XmlIgnore]
        public bool HasStarted
        {
            get { return _hasStarted; }
            set
            {
                if (_hasStarted != value)
                {
                    _hasStarted = value;
                    NotifyPropertyChanged("HasStarted");
                }
            }
        }

        public bool HasEnded
        {
            get
            {
                return Periods.HasEnded;
            }
        }

        private bool _hasCompleted = false;
        [System.Xml.Serialization.XmlIgnore]
        public bool HasCompleted
        {
            get { return _hasCompleted; }
            set
            {
                if (_hasCompleted != value)
                {
                    _hasCompleted = value;
                    NotifyPropertyChanged("HasCompleted");
                }
            }
        }

        private GameEventList _gameEvents = new GameEventList();
        public GameEventList GameEvents
        {
            get
            {
                return _gameEvents;
            }
        }

        public void LogEvent(string eventType, string team, string player, string notes)
        {
            GameEvents.Add(new GameEvent(DateTime.Now, eventType, team, player, notes));
            SaveGames();
        }

        public void LogEvent(string eventType, string team, string player)
        {
            LogEvent(eventType, team, player, null);
        }

        public void LogEvent(string eventType, string notes)
        {
            LogEvent(eventType, null, null, notes);
        }

        public void LogEvent(string eventType)
        {
            LogEvent(eventType, null, null, null);            
        }

        public void StartFirstPeriodInSeconds(int seconds)
        {
            if (StartTime != null)
            {
                TimeSpan difference = DateTime.Now - StartTime.Value + TimeSpan.FromSeconds(seconds);
                Periods.ModifyFollowingTimes(null, difference);
            }
        }
        
        public Game()
        {

        }

        public Game(string pool, string team1, string team2, GamePeriodList periods, int team1Score, int team2Score)
        {
            _pool = pool;
            _team1 = team1;
            _team2 = team2;
            _periods.Clone(periods);
            _team1Score = team1Score;
            _team2Score = team2Score;
        }

        public Game(string pool, string team1, string team2, GamePeriodList periodList)
            : this(pool, team1, team2, periodList, 0, 0)
        {
            // Overload constructor.
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(Periods.StartTime.ToString("hh:mm:ss"));
            if (!String.IsNullOrEmpty(Pool))
            {
                result.Append(" " + Pool.ToString());
            }
            result.Append(": " + Team1 + " " + Team1Score.ToString() + " vs " + Team2Score.ToString() + " " + Team2);

            return result.ToString();
        }

        public string ToJson()
        {
            StringBuilder result = new StringBuilder();
            result.Append("{");
            result.Append("\"startTime\": \"" + (StartTime.HasValue ? StartTime.Value.ToString("HH:mm") : "--:--") + "\"");
            result.Append(", \"team1\": \"" + Team1 + "\"");
            result.Append(", \"team1Score\": " + Team1Score.ToString());
            result.Append(", \"team2\": \"" + Team2 + "\"");
            result.Append(", \"team2Score\": " + Team2Score.ToString());
            if (!String.IsNullOrEmpty(Pool))
            {
                result.Append(", \"pool\": \"" + Pool + "\"");
            }
            else
            {
                result.Append(", \"pool\": \"\"");
            }
            if (Periods.CurrentPeriod != null)
            {
                result.Append(", \"period\": \"" + Periods.CurrentPeriod.Name + "\"");
                result.Append(", \"periodIsActive\": \"" + (Periods.CurrentPeriod.Status == GamePeriodStatus.Active ? "1" : "0") + "\"");
                result.Append(", \"timeRemaining\": \"" + GameTimeConverter.ToString(Periods.CurrentPeriod.TimeRemaining) + "\"");                
            }
            else
            {
                result.Append(", \"period\": \"None\"");
                result.Append(", \"periodIsActive\": false");
                result.Append(", \"timeRemaining\": \"0:00\"");
            }
            if (Parent != null && Parent.Parent != null)
            {
                result.Append(", \"shotClockTime\": \"" + Parent.Parent.ShotTime.ToString() + "\"");
            }
            else
            {
                result.Append(", \"shotClockTime\": \"0\"");
            }            

            if (Result != GameResult.None)
            {
                result.Append(", \"hasCompleted\": true");
                result.Append(", \"result\": \"" + ResultDescription + "\"");
                result.Append(", \"team1Points\": " + Team1Points.ToString());
                result.Append(", \"team2Points\": " + Team2Points.ToString());
            }
            else
            {
                result.Append(", \"hasCompleted\": false");
            }

            result.Append("}");

            return result.ToString();
        }

        public string CurrentPeriodDescription
        {
            get
            {
                if (Periods.CurrentPeriod == null)
                {
                    return "None";
                }
                else
                {
                    return Periods.CurrentPeriod.Description;
                }
            }
        }

        public string CurrentPeriodTimeDescription
        {
            get
            {
                if (Periods.CurrentPeriod == null)
                {
                    return "None";
                }
                else
                {
                    return GameTimeConverter.ToString(Periods.CurrentPeriod.TimeRemaining);
                }
            }
        }

        public TimeSpan? StartsIn
        {
            get
            {
                if (Periods.FirstPeriod != null && Periods.FirstPeriod.Status == GamePeriodStatus.Pending)
                {
                    return Periods.FirstPeriod.TimeRemaining;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
