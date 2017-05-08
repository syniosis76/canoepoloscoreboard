using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scoreboard
{
    public class GameTeam
    {
        public string Pool { get; set; }
        public string Team { get; set; }

        public GameTeam(string pool, string team)
        {
            Pool = pool;
            Team = team;
        }

        public static GameTeam FindGameTeam(Dictionary<GameTeam, int> gameTeamList, string pool, string team)
        {
            string gameTeamPool = !String.IsNullOrEmpty(pool) ? pool : "--";

            foreach (GameTeam gameTeam in gameTeamList.Keys)
            {
                if (String.Equals(gameTeamPool, gameTeam.Pool) && String.Equals(team, gameTeam.Team))
                {
                    return gameTeam;
                }
            }

            return null;
        }

        public static GameTeam AddOrGetGameTeam(Dictionary<GameTeam, int> gameTeamList, string pool, string team)
        {
            string gameTeamPool = !String.IsNullOrEmpty(pool) ? pool : "--";

            GameTeam gameTeam = FindGameTeam(gameTeamList, gameTeamPool, team);

            if (gameTeam != null)
            {
                return gameTeam;
            }
            else
            {
                GameTeam newGameTeam = new GameTeam(gameTeamPool, team);
                gameTeamList[newGameTeam] = 0;
                return newGameTeam;
            }
        }

        public static int AddPoints(Dictionary<GameTeam, int> pointList, GameTeam team, int points)
        {
            if (!pointList.ContainsKey(team))
            {
                pointList[team] = 0;
            }
            pointList[team] += points;

            return pointList[team];
        }

        public static int? FindPoints(Dictionary<GameTeam, int> gameTeamList, string pool, string team)
        {
            GameTeam gameTeam = FindGameTeam(gameTeamList, pool, team);
            if (gameTeam != null)
            {
                return gameTeamList[gameTeam];
            }
            else
            {
                return null;
            }
        }
    }
}
