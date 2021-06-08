using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Utilities;
using System.Windows.Input;

namespace Scoreboard
{
    public class GamePeriod : INotifyPropertyChanged
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

        private GamePeriodList _parent;
        private GamePeriodList Parent
        {
            get { return _parent; }
        }

        public void SetParent(GamePeriodList parent)
        {
            _parent = parent;
        }

        #endregion

        private String _name;
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (!String.Equals(_name, value))
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                    SetupDescription();                    
                }
            }
        }

        private String _description;
        public String Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (!String.Equals(_description, value))
                {
                    _description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        private void SetupDescription()
        {
            if (Status == GamePeriodStatus.Ended)
            {
                Description = "Ended";
            }
            else
            {
                Description = Name;
            }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                if (!_startTime.Equals(value))
                {
                    _startTime = value;
                    UpdatePeriod();
                    NotifyPropertyChanged("StartTime");
                    if (Parent != null)
                    {
                        Parent.NotifyPropertyChanged("StartTime");
                    }
                }
            }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                if (!_endTime.Equals(value))
                {
                    _endTime = value;
                    UpdatePeriod();
                    NotifyPropertyChanged("EndTime");
                }
            }
        }

        private bool _isExtraPeriod = false;
        public bool IsExtraPeriod
        {
            get { return _isExtraPeriod; }
            set
            {
                if (!_isExtraPeriod.Equals(value))
                {
                    _isExtraPeriod = value;
                    UpdatePeriod();
                    NotifyPropertyChanged("IsExtraPeriod");
                    NotifyPropertyChanged("IsActiveExtraPeriod");                    
                }
            }
        }

        public bool IsActiveExtraPeriod
        {
            get
            {
                return IsExtraPeriod && Status == GamePeriodStatus.Active;
            }
        }

        public bool IsCurrentPeriod
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.CurrentPeriod == this;
                }
                else
                {

                    return Status == GamePeriodStatus.Active;
                }
            }
        }

        public void Clone(GamePeriod period)
        {
            Name = period.Name;
            StartTime = period.StartTime;
            EndTime = period.EndTime;
            IsExtraPeriod = period.IsExtraPeriod;
        }

        public TimeSpan TotalDuration
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        private GamePeriodStatus _status = GamePeriodStatus.Pending;
        public GamePeriodStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (!_status.Equals(value))
                {
                    _status = value;
                    SetupDescription();
                    NotifyPropertyChanged("Status");
                    NotifyPropertyChanged("IsActiveExtraPeriod");
                    NotifyPropertyChanged("IsCurrentPeriod");
                    LogStatusChange();
                    if (Status == GamePeriodStatus.Active)
                    {
                        Score score = Parent?.Parent?.Parent?.Parent;
                        if (score != null && score.StartPaused)
                        {
                            RoundTimeRemainingUpToSecond();
                            score.Pause();
                        }
                    }
                }
            }
        }    
        
        private void RoundTimeRemainingUpToSecond()
        {
            TimeRemaining = TimeRemaining.Add(new TimeSpan(0, 0, 0, 0, 1000 - TimeRemaining.Milliseconds));
        }

        private void LogStatusChange()
        {
            if (Status == GamePeriodStatus.Active)
            {
                if (Parent != null && Parent.Parent != null)
                {
                    Parent.Parent.LogEvent("Start " + Name);
                }
            }
            else if (Status == GamePeriodStatus.Ended)
            {
                if (Parent != null && Parent.Parent != null)
                {
                    Parent.Parent.LogEvent("End " + Name);
                }
            }
        }

        private TimeSpan _timeRemaining = new TimeSpan(0, 0, 0);
        public TimeSpan TimeRemaining
        {
            get
            {
                return _timeRemaining;
            }
            set
            {
               if (!_timeRemaining.Equals(value))
                {
                    _timeRemaining = value;
                    NotifyPropertyChanged("TimeRemaining");
                    if (Parent != null && Parent.Parent != null)
                    {
                        Parent.Parent.NotifyPropertyChanged("StartsIn");
                    }
                } 
            }
        }

        #region Commands

        RelayCommand _decrementStartTimeCommand;
        public ICommand DecrementStartTimeCommand
        {
            get
            {
                if (_decrementStartTimeCommand == null)
                {
                    _decrementStartTimeCommand = new RelayCommand(param => this.ModifyStartTime(-10, 10, true), null);
                }
                return _decrementStartTimeCommand;
            }
        }

        RelayCommand _incrementStartTimeCommand;
        public ICommand IncrementStartTimeCommand
        {
            get
            {
                if (_incrementStartTimeCommand == null)
                {
                    _incrementStartTimeCommand = new RelayCommand(param => this.ModifyStartTime(10, 10, true), null);
                }
                return _incrementStartTimeCommand;
            }
        }

        RelayCommand _decrementEndTimeCommand;
        public ICommand DecrementEndTimeCommand
        {
            get
            {
                if (_decrementEndTimeCommand == null)
                {
                    _decrementEndTimeCommand = new RelayCommand(param => this.ModifyEndTime(-10, 10, true), null);
                }
                return _decrementEndTimeCommand;
            }
        }

        RelayCommand _incrementEndTimeCommand;
        public ICommand IncrementEndTimeCommand
        {
            get
            {
                if (_incrementEndTimeCommand == null)
                {
                    _incrementEndTimeCommand = new RelayCommand(param => this.ModifyEndTime(10, 10, true), null);
                }
                return _incrementEndTimeCommand;
            }
        }

        RelayCommand _incremenGameTimeCommand;
        public ICommand IncrementGameTimeCommand
        {
            get
            {
                if (_incremenGameTimeCommand == null)
                {
                    _incremenGameTimeCommand = new RelayCommand(param => this.ModifyGameTime(1, false), null);
                }
                return _incremenGameTimeCommand;
            }
        }

        RelayCommand _decrementGameTimeCommand;
        public ICommand DecrementGameTimeCommand
        {
            get
            {
                if (_decrementGameTimeCommand == null)
                {
                    _decrementGameTimeCommand = new RelayCommand(param => this.ModifyGameTime(-1, false), null);
                }
                return _decrementGameTimeCommand;
            }
        }

        RelayCommand _endNowCommand;
        public ICommand EndNowCommand
        {
            get
            {
                if (_endNowCommand == null)
                {
                    _endNowCommand = new RelayCommand(param => this.EndNow(), null);
                }
                return _endNowCommand;
            }
        }

        #endregion

        protected TimeSpan ModifyByTimeSpan(DateTime time, int changeBy, int roundToNearest)
        {            
            DateTime newTime = time + new TimeSpan(0, 0, changeBy);
            DateTime newRoundedTime = new DateTime(newTime.Year, newTime.Month, newTime.Day, newTime.Hour, newTime.Minute, newTime.Second);

            int seconds = newTime.Second;
            int remainder = seconds % roundToNearest;            

            if (remainder > roundToNearest / 2)
            {
                newRoundedTime += new TimeSpan(0, 0, roundToNearest - remainder);
            }
            else
            {
                newRoundedTime -= new TimeSpan(0, 0, remainder);
            }

            return newRoundedTime - time;
        }

        public void ModifyStartTime(int changeBy, int roundToNearest, Boolean force)
        {                     
            TimeSpan changeByTimeSpan = ModifyByTimeSpan(StartTime, changeBy, roundToNearest);
            ModifyStartTime(changeByTimeSpan, force);            
        }

        public void ModifyStartTime(TimeSpan changeBy, Boolean force)
        {            
            StartTime += changeBy;
            EndTime += changeBy;
            ModifyFollowingTimes(changeBy, force);
        }

        public void ModifyEndTime(int changeBy, int roundToNearest, Boolean force)
        {
            TimeSpan changeByTimeSpan = ModifyByTimeSpan(EndTime, changeBy, roundToNearest);
            ModifyEndTime(changeByTimeSpan, force);
        }

        public void ModifyEndTime(TimeSpan changeBy, Boolean force)
        {            
            EndTime += changeBy;
            ModifyFollowingTimes(changeBy, force);
        }

        public void ModifyTimes(TimeSpan changeBy)
        {
            StartTime += changeBy;
            EndTime += changeBy;
        }

        public void ModifyFollowingTimes(TimeSpan changeBy, Boolean force)
        {
            if (Parent != null)
            {
                Parent.ModifyFollowingTimes(this, changeBy, force);
            }
        }

        public void ModifyGameTime(int changeBy, Boolean force)
        {
            DateTime targetTime = DateTime.Now + TimeRemaining + new TimeSpan(0, 0, changeBy);            

            if (Status == GamePeriodStatus.Pending)
            {                                
                ModifyStartTime(targetTime - StartTime, force);
            }
            else if (Status == GamePeriodStatus.Active)
            {
                ModifyEndTime(targetTime - EndTime, force);
            }
        }

        public void EndNow()
        {
            DateTime now = DateTime.Now;
            if (now < EndTime)
            {
                //TimeSpan difference = now - EndTime;
                EndTime = now;
                //ModifyFollowingTimes(difference);
            }
        }

        public GamePeriodStatus UpdatePeriod()
        {
            DateTime now = DateTime.Now;
            if (now < StartTime)
            {
                TimeRemaining = StartTime - now;
                Status = GamePeriodStatus.Pending;
            }
            else if (now < EndTime)
            {
                TimeRemaining = EndTime - now;
                Status = GamePeriodStatus.Active;
                if (Parent != null && Parent.Parent != null)
                {
                    Parent.Parent.HasStarted = true;
                }
            }
            else
            {
                TimeRemaining = new TimeSpan(0, 0, 0);
                Status = GamePeriodStatus.Ended;
            }            
            return Status;
        }

        public void AdjustTimes(TimeSpan adjustment)
        {
            StartTime += adjustment ;            
            EndTime += adjustment;            
        }
    }
}
