using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public class GameEventView
    {
        private GameEvent _gameEvent;
        public GameEvent GameEvent
        {
            get { return _gameEvent; }
        }

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
        }

        private string _eventType;
        public string EventType
        {
            get { return _eventType; }
        }

        private string _image;
        public string Image
        {
            get { return _image; }
        }        

        private string _team1Player;
        public string Team1Player
        {
            get { return _team1Player; }
        }

        private string _team2Player;
        public string Team2Player
        {
            get { return _team2Player; }
        }

        public GameEventView()
        {

        }

        public GameEventView(GameEvent gameEvent, DateTime time, string eventType, string team1Player, string team2Player)
        {
            _gameEvent = gameEvent;
            _time = time;
            _eventType = eventType;
            if (EventType == "Goal") _image = "/Scoreboard;component/images/goal.png";
            else if (EventType == "Green Card") _image = "/Scoreboard;component/images/green_card.png";
            else if (EventType == "Yellow Card") _image = "/Scoreboard;component/images/yellow_card.png";
            else if (EventType == "Red Card") _image = "/Scoreboard;component/images/red_card.png";
            else if (EventType == "Ejection Red Card") _image = "/Scoreboard;component/images/ejection_red_card.png";
            _team1Player = team1Player;
            _team2Player = team2Player;
        }

        public override string ToString()
        {
            return Time.ToString("HH:mm:ss")
                + "\t" + EventType
                + "\t" + Team1Player
                + "\t" + Team2Player;
        }
    }    
}
