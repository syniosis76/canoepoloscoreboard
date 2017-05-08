using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scoreboard
{
    public class GamePlayer
    {
        public string Pool { get; set; }
        public string Team { get; set; }
        public string Player { get; set; }

        public GamePlayer(string pool, string team, string player)
        {
            Pool = pool;
            Team = team;
            Player = player;
        }

        public static GamePlayer FindGamePlayer(Dictionary<GamePlayer, int> gamePlayerList, string pool, string team, string player)
        {
            string gamePlayerPool = !String.IsNullOrEmpty(pool) ? pool : "--";

            foreach (GamePlayer gamePlayer in gamePlayerList.Keys)
            {
                if (String.Equals(gamePlayerPool, gamePlayer.Pool) && String.Equals(team, gamePlayer.Team) && String.Equals(player, gamePlayer.Player))
                {
                    return gamePlayer;
                }
            }

            return null;
        }

        public static GamePlayer AddOrGetGamePlayer(Dictionary<GamePlayer, int> gamePlayerList, string pool, string team, string player)
        {
            string gamePlayerPool = !String.IsNullOrEmpty(pool) ? pool : "--";

            GamePlayer gamePlayer = FindGamePlayer(gamePlayerList, gamePlayerPool, team, player);

            if (gamePlayer != null)
            {
                return gamePlayer;
            }
            else
            {
                GamePlayer newGamePlayer = new GamePlayer(gamePlayerPool, team, player);
                gamePlayerList[newGamePlayer] = 0;
                return newGamePlayer;
            }
        }

        public static int AddPoints(Dictionary<GamePlayer, int> pointList, GamePlayer player, int points)
        {
            if (!pointList.ContainsKey(player))
            {
                pointList[player] = 0;
            }
            pointList[player] += points;

            return pointList[player];
        }

        public static int? FindGoals(Dictionary<GamePlayer, int> gamePlayerList, string pool, string team, string player)
        {
            GamePlayer gamePlayer = FindGamePlayer(gamePlayerList, pool, team, player);
            if (gamePlayer != null)
            {
                return gamePlayerList[gamePlayer];
            }
            else
            {
                return null;
            }
        }
    }
}

