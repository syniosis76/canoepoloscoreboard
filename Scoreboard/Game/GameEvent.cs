using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public class GameEvent : INotifyPropertyChanged
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

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _eventType;
        public string EventType
        {
            get { return _eventType; }
            set
            {
                if (_eventType != value)
                {
                    _eventType = value;
                    NotifyPropertyChanged("EventType");
                }
            }
        }

        private string _team;
        public string Team
        {
            get { return _team; }
            set
            {
                if (_team != value)
                {
                    _team = value;
                    NotifyPropertyChanged("Team");
                }
            }
        }

        private string _player;
        public string Player
        {
            get { return _player; }
            set
            {
                if (_player != value)
                {
                    _player = value;
                    NotifyPropertyChanged("Player");
                }
            }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    NotifyPropertyChanged("Notes");
                }
            }
        }

        public GameEvent()
        {

        }

        public GameEvent(DateTime time, string eventType, string team, string player, string notes)
        {
            Time = time;
            EventType = eventType;
            Team = team;
            Player = player;
            Notes = notes;
        }

        public override string ToString()
        {
            return Time.ToString("HH:mm:ss")
                + "\t" + EventType
                + "\t" + Team
                + "\t" + Player
                + "\t" + Notes;
        }
    }    
}
