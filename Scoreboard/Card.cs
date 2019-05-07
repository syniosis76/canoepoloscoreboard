using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public class Card : INotifyPropertyChanged
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
               
        private int _time;
        public int Time
        {
            get
            {
                return _time;
            }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    NotifyPropertyChanged("Time");  
                }
            }
        }

        private GameEvent _gameEvent;
        public GameEvent GameEvent
        {
            get
            {
                return _gameEvent;
            }
        }

        public Card(int time, GameEvent gameEvent)
        {
            _time = time;
            _gameEvent = gameEvent;
        }
    }
}
