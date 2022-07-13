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
            PropertyChanged?.Invoke(this, e);
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

        public void ModifyFollowingTimes(TimeSpan changeBy, Boolean force)
        {
            if (Parent != null)
            {
                Parent.ModifyFollowingTimes(this, changeBy, force);
            }
        }

        public bool IsCurrentGame
        {
            get
            {
                return Parent != null && Parent.CurrentGame == this;
            }
            set
            {
                NotifyPropertyChanged("IsCurrentGame");
            }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public bool NeedsSending;

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

                if (!String.IsNullOrWhiteSpace(_team1) && String.IsNullOrWhiteSpace(_team1Original))
                {
                    _team1Original = _team1;
                    NotifyPropertyChanged("Team1Original");
                }
            }
        }

        private string _team1Original;
        public string Team1Original
        {
            get { return _team1Original;  }
        }

        private string _team1Flag;
        public string Team1Flag
        {
            get { return _team1Flag; }
            set
            {
                _team1Flag = value;
                NotifyPropertyChanged("Team1Flag");
                NotifyPropertyChanged("Team1GlagImage");
            }
        }
        public string Team1FlagImage
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(Team1Flag))
                {
                    return "/Scoreboard;component/flags/" + Team1Flag + ".png";
                }
                return String.Empty;
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

                if (!String.IsNullOrWhiteSpace(_team2) && String.IsNullOrWhiteSpace(_team2Original))
                {
                    _team2Original = _team2;
                    NotifyPropertyChanged("Team2Original");
                }
            }
        }

        private string _team2Original;
        public string Team2Original
        {
            get { return _team2Original; }
        }

        private string _team2Flag;
        public string Team2Flag
        {
            get { return _team2Flag; }
            set
            {
                _team2Flag = value;
                NotifyPropertyChanged("Team2Flag");
                NotifyPropertyChanged("Team2FlagImage");
            }
        }
        public string Team2FlagImage
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(Team2Flag))
                {
                    return "/Scoreboard;component/flags/" + Team2Flag + ".png";
                }
                return String.Empty;
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

        public bool NameChanged = false;

        private int _team1Score;
        public int Team1Score
        {
            get { return _team1Score; }
            set
            {
                _team1Score = value;
                NotifyPropertyChanged("Team1Score");
                CalculateResult();
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
                CalculateResult();
            }
        }

        private readonly GamePeriodList _periods = new GamePeriodList();
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

        private int _skipCalculateResult = 0;

        public void BeginSkipCalculateResult()
        {
            _skipCalculateResult++;
        }

        public void EndSkipCalculateResult()
        {
            _skipCalculateResult--;
            if (_skipCalculateResult < 0)
            {
                _skipCalculateResult = 0;
            }
        }

        public Boolean IsDraw { get { return Result == GameResult.Draw; } }

        public GameResult CalculateResult()
        {
            if (_skipCalculateResult == 0)
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
                    if (Parent != null && Parent.Parent != null)
                    {
                        Parent.Parent.Tourney.ApplyGame(this);
                    }
                }
                else
                {
                    Result = GameResult.None;
                }
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

        private readonly GameEventList _gameEvents = new GameEventList();
        public GameEventList GameEvents
        {
            get
            {
                return _gameEvents;
            }
        }

        private readonly BindingList<GameEventView> _gameEventsView = new BindingList<GameEventView>();
        public BindingList<GameEventView> GameEventsView
        {
            get
            {
                return _gameEventsView;
            }
        }

        public void FilterGameEvents()
        {
            GameEventsView.Clear();
            foreach (GameEvent gameEvent in GameEvents)
            {
                if (!String.IsNullOrEmpty(gameEvent.Team))
                {
                    string player = '#' + gameEvent.Player;
                    if (player == "#Unknown") player = "#?";
                    GameEventsView.Add(new GameEventView(gameEvent, gameEvent.Time, gameEvent.EventType
                            , gameEvent.Team == Team1 || gameEvent.Team == Team1Original ? player : String.Empty
                            , gameEvent.Team == Team2 || gameEvent.Team == Team2Original ? player : String.Empty));
                }
            }
        }

        public void Loaded()
        {
            FilterGameEvents();
        }

        public GameEvent LogEvent(string eventType, string team, string player, string notes, bool apply = true)
        {
            GameEvent gameEvent = new GameEvent(DateTime.Now, eventType, team, player, notes);
            GameEvents.Add(gameEvent);
            FilterGameEvents();            
            SaveGames();
            if (apply)
            {
                Parent?.Parent?.Tourney?.ApplyGame(this);
            }
            return gameEvent;
        }

        public GameEvent LogEvent(string eventType, string team, string player, bool apply = true)
        {
            return LogEvent(eventType, team, player, null, apply);
        }

        public GameEvent LogEvent(string eventType, string notes, bool apply = true)
        {
            return LogEvent(eventType, null, null, notes, apply);
        }

        public GameEvent LogEvent(string eventType, bool apply = true)
        {
            return LogEvent(eventType, null, null, null, apply);         
        }

        public GameEvent FindLastGameEvent(string team, string eventType)
        {
            return GameEvents.Where(x => x.Team == team && x.EventType == eventType).LastOrDefault();
        }

        public void StartFirstPeriodInSeconds(int seconds)
        {
            if (StartTime != null)
            {
                TimeSpan difference = DateTime.Now - StartTime.Value + TimeSpan.FromSeconds(seconds);
                Periods.ModifyFollowingTimes(null, difference, false);
            }
        }
        
        public Game()
        {

        }

        public Game(string pool, string team1, string team2, GamePeriodList periods, int team1Score, int team2Score)
        {
            _pool = pool;
            _team1 = team1;
            _team1Original = team1;
            _team2 = team2;
            _team2Original = team2;
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
            result.Append('{');
            result.Append("\"startTime\": \"" + (StartTime.HasValue ? StartTime.Value.ToString("HH:mm") : "--:--") + "\"");
            result.Append(", \"team1\": \"" + Team1 + "\"");
            if (!String.IsNullOrEmpty(Team1Flag))
            {
                result.Append(", \"team1Flag\": \"" + Team1Flag + "\"");
            }
            result.Append(", \"team1Score\": " + Team1Score.ToString());
            result.Append(", \"team2\": \"" + Team2 + "\"");
            if (!String.IsNullOrEmpty(Team2Flag))
            {
                result.Append(", \"team2Flag\": \"" + Team2Flag + "\"");
            }
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

            result.Append('}');

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

        public void CalculateScoreFromEvents()
        {
            BeginSkipCalculateResult();
            try
            {
                int team1Score = 0;
                int team2Score = 0;

                foreach (GameEvent gameEvent in GameEvents)
                {
                    if (gameEvent.EventType == "Goal")
                    {
                        if (gameEvent.Team == Team1 || gameEvent.Team == Team1Original)
                        {
                            team1Score += 1;
                        }
                        else if (gameEvent.Team == Team2 || gameEvent.Team == Team2Original)
                        {
                            team2Score += 1;
                        }
                    }
                }

                Team1Score = team1Score;
                Team2Score = team2Score;
            }
            finally
            {
                EndSkipCalculateResult();
                CalculateResult();
            }            
        }
    }
}
