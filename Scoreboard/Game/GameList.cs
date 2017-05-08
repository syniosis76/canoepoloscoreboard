using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public class GameList : BindingList<Game>
    {
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

        public void ModifyFollowingTimes(Game game, TimeSpan changeBy)
        {
            int gameIndex = game == null ? -1 : IndexOf(game);
            foreach (Game gameItem in this.Skip(gameIndex + 1))
            {
                gameItem.ModifyTimes(changeBy);
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
