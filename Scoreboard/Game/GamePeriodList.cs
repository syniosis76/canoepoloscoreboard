using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public class GamePeriodList : BindingList<GamePeriod>, INotifyPropertyChanged
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

        private Game _parent;
        public Game Parent
        {
            get { return _parent; }
        }

        public void SetParent(Game parent)
        {
            _parent = parent;
        }

        public new void Add(GamePeriod gamePeriod)
        {
            base.Add(gamePeriod);
            gamePeriod.SetParent(this);
        }

        #endregion

        public void ModifyTimes(TimeSpan changeBy)
        {
            foreach (GamePeriod gamePeriod in this)
            {
                gamePeriod.ModifyTimes(changeBy);
            }
        }

        public void ModifyFollowingTimes(GamePeriod period, TimeSpan changeBy, Boolean force)
        {
            int periodIndex = period == null ? -1 : IndexOf(period);
            foreach (GamePeriod gamePeriod in this.Skip(periodIndex + 1))
            {
                gamePeriod.ModifyTimes(changeBy);
            }

            if (Parent != null)
            {
                Parent.ModifyFollowingTimes(changeBy, force);
            }
        }

        private int? _currentIndex = null;
        public int? CurrentIndex
        {
            get
            {
                return _currentIndex;
            }
            set
            {
                if (!_currentIndex.Equals(value))
                {
                    GamePeriod oldPeriod = CurrentPeriod;
                    _currentIndex = value;
                    if (oldPeriod != null)
                    {
                        oldPeriod.NotifyPropertyChanged("IsCurrentPeriod");
                    }
                    if (CurrentPeriod != null)
                    {
                        CurrentPeriod.UpdatePeriod();
                        CurrentPeriod.NotifyPropertyChanged("IsCurrentPeriod");
                    }
                    NotifyPropertyChanged("CurrentPeriod");                    
                }
            }
        }

        public DateTime StartTime
        {
            get
            {
                if (Count > 0)
                {
                    return Items[0].StartTime;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public DateTime EndTime
        {
            get
            {
                if (Count > 0)
                {
                    return Items[Count - 1].EndTime;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public GamePeriod CurrentPeriod
        {
            get
            {
                if (CurrentIndex != null && CurrentIndex >= 0 && CurrentIndex < Count)
                {
                    return Items[CurrentIndex.Value];
                }
                else
                {
                    return null;
                }
            }
        }

        public GamePeriod AddPeriod(String name, DateTime startTime, DateTime endTime)
        {
            GamePeriod newPeriod = new GamePeriod();
            newPeriod.Name = name;
            newPeriod.StartTime = startTime;
            newPeriod.EndTime = endTime;
            Add(newPeriod);
            return newPeriod;
        }

        public GamePeriod FirstPeriod
        {
            get
            {
                if (Count > 0)
                {
                    return Items[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public GamePeriod SetFirstPeriod()
        {
            CurrentIndex = 0;            
            return CurrentPeriod;
        }

        public GamePeriod NextPeriod()
        {
            if (CurrentIndex != null)
            {
                if (CurrentIndex.Value >= Count - 1)
                {
                    CurrentIndex = null;
                }
                else
                {
                    CurrentIndex = Math.Max(0, Math.Min(Count - 1, CurrentIndex.Value + 1));
                }
            }
            return CurrentPeriod;
        }

        public GamePeriod PreviousPeriod()
        {
            if (CurrentIndex != null)
            {
                CurrentIndex = Math.Max(0, Math.Min(Count - 1, CurrentIndex.Value - 1)); ;
            }
            return CurrentPeriod;
        }

        public GamePeriod SetLastPeriod()
        {
            CurrentIndex = Math.Max(0, Count - 1);
            return CurrentPeriod;
        }

        public void Clone(GamePeriodList periods)
        {
            Clear();
            if (periods != null)
            {
                foreach (GamePeriod period in periods)
                {
                    GamePeriod newPeriod = new GamePeriod();
                    newPeriod.Clone(period);
                    Add(newPeriod);
                }
            }
        }

        public TimeSpan TotalDuration
        {
            get
            {
                TimeSpan result = new TimeSpan(0, 0, 0);
                foreach (GamePeriod period in Items)
                {
                    result += period.TotalDuration;
                }
                return result;
            }
        }

        public bool HasEnded
        {
            get
            {
                GamePeriod lastPeriod = this.LastOrDefault();
                if (lastPeriod != null && lastPeriod.Status == GamePeriodStatus.Ended)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (GamePeriod gamePeriod in this)
            {
                result.AppendLine(gamePeriod.ToString());
            }

            return result.ToString();
        }

        public void AdjustTimes(TimeSpan adjustment)
        {
            foreach (GamePeriod period in this)
            {
                period.AdjustTimes(adjustment);
            }
        }

        public void ResetPeriod()
        {
            // Todo
        }
    }  
}
