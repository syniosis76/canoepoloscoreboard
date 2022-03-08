using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;

namespace Scoreboard
{
    public class Card : INotifyPropertyChanged
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

        public Brush DisplayColor
        {
            get
            {
                Color color;
                if (GameEvent.EventType == "Green Card")
                {
                    color = Color.FromArgb(0xFF, 0x3A, 0xC8, 0x2B);
                }
                else if (GameEvent.EventType == "Yellow Card")
                {
                    color = Color.FromArgb(0xFF, 0xF7, 0xCA, 0x16);
                }
                else if (GameEvent.EventType == "Red Card")
                {
                    color = Color.FromArgb(0xFF, 0xD3, 0x26, 0x28);
                }
                else
                {
                    color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                }
                return new SolidColorBrush(color);
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
