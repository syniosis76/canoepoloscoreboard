using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace Scoreboard
{
    public class GameList : BindingList<Game>, INotifyPropertyChanged, IXmlSerializable
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

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            return (null);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("TournamentId", TournamentId);
            writer.WriteAttributeString("GameDateId", GameDateId);
            writer.WriteAttributeString("PitchId", PitchId);

            if (Count > 0)
            {
                XmlSerializer inner = new XmlSerializer(typeof(Game));
                for (int gameIndex = 0; gameIndex < Count; gameIndex++)
                {
                    inner.Serialize(writer, this[gameIndex]);
                }
            }        
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }

            TournamentId = reader.GetAttribute("TournamentId");
            GameDateId = reader.GetAttribute("GameDateId");
            PitchId = reader.GetAttribute("PitchId");

            reader.Read();

            XmlSerializer inner = new XmlSerializer(typeof(Game));
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                Game game = (Game)inner.Deserialize(reader);
                Add(game);
            }
        }        

        #endregion

        #region Parent

        private Score _parent;
        [System.Xml.Serialization.XmlIgnore]
        public Score Parent
        {
            get { return _parent; }
        }

        public void SetParent(Score parent)
        {
            _parent = parent;
        }

        #endregion               

        private string _tournamentId;
        public string TournamentId
        {
            get { return _tournamentId; }
            set
            {
                _tournamentId = value;
                NotifyPropertyChanged("TournamentId");
            }
        }

        private string _gameDateId;
        public string GameDateId
        {
            get { return _gameDateId; }
            set
            {
                _gameDateId = value;
                NotifyPropertyChanged("GameDateId");
            }
        }

        private string _pitchId;
        public string PitchId
        {
            get { return _pitchId; }
            set
            {
                _pitchId = value;
                NotifyPropertyChanged("PitchId");
            }
        }

        public void ClearGames()
        {
            TournamentId = null;
            GameDateId = null;
            PitchId = null;
            Clear();
        }

        public void Assign(GameList games)
        {
            TournamentId = games.TournamentId;
            GameDateId = games.GameDateId;
            PitchId = games.PitchId;
        }

        public void SaveGames()
        {
            if (Parent != null)
            {
                Parent.SaveGames();
            }
        }

        public Game CurrentGame
        {
            get
            {
                return Parent != null ? Parent.CurrentGame : null;
            }
        }
        
        public bool HasPool
        {
            get
            {
                foreach (Game game in this)
                {
                    if (!String.IsNullOrWhiteSpace(game.Pool))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public new void Add(Game game)
        {
            base.Add(game);
            game.SetParent(this);
        }

        public void ModifyFollowingTimes(Game game, TimeSpan changeBy, Boolean force)
        {
            Game previousGame = game;
            int gameIndex = game == null ? -1 : IndexOf(game);
            TimeSpan minimumTimeBetweenGames = Score.ParseTimeSpan(Properties.Settings.Default.MinimumTimeBetweenGames);            

            foreach (Game gameItem in this.Skip(gameIndex + 1))
            {
                if (force || changeBy.TotalMilliseconds < 0)
                {
                    // Always adjust the following times (when manually updating game times).
                    gameItem.ModifyTimes(changeBy);
                }
                else
                {
                    // When resuming from pause only adjust the following game time if the time betweeen games is less than the MinimumTimeBetweenGames.
                    if (gameItem.StartTime.HasValue && previousGame.EndTime.HasValue)
                    {
                        TimeSpan timeUntilGameStarts = gameItem.StartTime.Value - previousGame.EndTime.Value;
                        if (timeUntilGameStarts > minimumTimeBetweenGames)
                        {
                            // Don't adjust anythign because the time betweeen games is more than the MinimumTimeBetweenGames.
                            break;
                        }
                        else
                        {
                            // Adjust the followng game time so that MinimumTimeBetweenGames is true.
                            gameItem.ModifyTimes(previousGame.EndTime.Value + minimumTimeBetweenGames - gameItem.StartTime.Value);
                        }
                    }
                }                
                previousGame = gameItem;
            }
        }

        public void Sort()
        {
            List<Game> sortedGames = new List<Game>(this);
            sortedGames.Sort((Game x, Game y) => x.StartTime.Value.CompareTo(y.StartTime.Value));
            Clear();
            foreach (Game game in sortedGames)
            {
                Add(game);
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (Game game in this)
            {
                result.AppendLine(game.ToString());
            }

            return result.ToString();
        }
    }  
}
