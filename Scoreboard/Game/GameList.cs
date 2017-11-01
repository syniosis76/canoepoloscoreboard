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
