using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Windows;

namespace Scoreboard
{
    public class TourneyId
    {
        public string Tournament { get; set; }
        public string GameDate { get; set; }
        public string Pitch { get; set; }
        public string Game { get; set; }
    }


    public class Tourney
    {
        private static HttpClient _httpClient = new HttpClient();

        private string _baseUrl;
        private Window _owner;

        public static bool SelectAndAddGames(Window owner, Score score)
        {
            string tourneyUrl = Properties.Settings.Default.TourneyUrl;
            Tourney tourney = new Tourney(owner, tourneyUrl);

            string tournamentId = tourney.SelectTournament();
            if (!String.IsNullOrWhiteSpace(tournamentId))
            {
                (JObject gameDate, JObject pitch) = tourney.SelectPitch(tournamentId);
                if (gameDate != null && pitch != null)
                {
                    score.TournamentId = tournamentId;
                    score.GameDateId = (string)(gameDate["id"]["value"]);
                    score.PitchId = (string)(pitch["id"]["value"]);

                    GameList newGames = new GameList();

                    JArray gameTimes = (JArray)gameDate["gameTimes"];
                    JArray games = (JArray)pitch["games"];
                    for (int gameIndex = 0; gameIndex < games.Count; gameIndex++)
                    {
                        string gameTime = (string)gameTimes[gameIndex];
                        JObject game = (JObject)games[gameIndex];
                        newGames.Add(tourney.CreateFromTourneyGame(gameTime, game));
                    }

                    score.Games.Clear();
                    score.AddGames(newGames);                    
                }
            }

            return false;
        }

        public Tourney(Window owner, string baseUrl)
        {
            _owner = owner;
            _baseUrl = baseUrl;
            if (Tourney._httpClient == null)
            {
                Tourney._httpClient = new HttpClient();
            }
        }

        public string SelectTournament()
        {           
            try
            {
                ProcessingWindow.ShowProcessing(_owner, "Listing Tournaments...");

                JObject tournaments = GetRequestAsJObject("/data/tournaments");
                if (tournaments != null)
                {
                    SelectListWindow selectListWindow = new SelectListWindow();
                    selectListWindow.Owner = _owner;
                    selectListWindow.Title = "Select Tournament";

                    foreach (JObject tournament in tournaments["tournaments"])
                    {
                        selectListWindow.Items.Add(new SelectItem((string)(tournament["id"]["value"]), (string)tournament["name"]));
                    }

                    ProcessingWindow.HideProcessing();

                    if (selectListWindow.ShowDialog() == true)
                    {
                        return selectListWindow.SelectedId;
                    }
                }
                else
                {
                    ProcessingWindow.HideProcessing();
                    MessageBox.Show("Unable to list Tournaments.");
                }
            }
            finally
            {
                ProcessingWindow.HideProcessing();
            }               

            return null;
        }

        public (JObject gameDate, JObject pitch) SelectPitch(string id)
        {
            try
            {
                ProcessingWindow.ShowProcessing(_owner, "Listing Games...");

                JObject tournament = GetRequestAsJObject("/data/tournament/" + id);
                if (tournament != null)
                {
                    SelectListWindow selectListWindow = new SelectListWindow();
                    selectListWindow.Owner = _owner;
                    selectListWindow.Title = "Select Pitch";

                    foreach (JObject gameDate in tournament["gameDates"])
                    {
                        string gameDateName = ((DateTime)(gameDate["date"]["value"])).ToString("ddd MMM dd yyyy");
                        foreach (JObject pitch in gameDate["pitches"])
                        {
                            selectListWindow.Items.Add(new SelectItem((string)(pitch["id"]["value"]), gameDateName + " " + (string)pitch["name"]));
                        }
                    }

                    ProcessingWindow.HideProcessing();

                    if (selectListWindow.ShowDialog() == true)
                    {
                        string pitchId = selectListWindow.SelectedId;

                        foreach (JObject gameDate in tournament["gameDates"])
                        {
                            string gameDateName = ((DateTime)(gameDate["date"]["value"])).ToString("ddd MMM dd yyyy");
                            foreach (JObject pitch in gameDate["pitches"])
                            {
                                if ((string)(pitch["id"]["value"]) == pitchId)
                                {
                                    return (gameDate, pitch);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ProcessingWindow.HideProcessing();
                    MessageBox.Show("Unable to load Tournament details.");
                }
            }
            finally
            {
                ProcessingWindow.HideProcessing();
            }

            return (null, null);
        }

        public Game CreateFromTourneyGame(string gameTime, JObject game)
        {
            Game newGame = new Game();
            newGame.Id = (string)(game["id"]["value"]);
            newGame.Pool = (string)game["group"];
            newGame.Team1 = (string)game["team1"];
            newGame.Team1Score = (int)game["team1Score"];
            //newGame.Team1Points = (int)game["team1Points"];
            newGame.Team2 = (string)game["team2"];
            newGame.Team2Score = (int)game["team2Score"];
            //newGame.Team2Points = (int)game["team2Points"];            
   
            DateTime startTime = Score.ParseTime(gameTime);
            TimeSpan periodDuration = Score.ParseTimeSpan("10");
            TimeSpan intervalDuration = Score.ParseTimeSpan("1");

            newGame.Periods.AddPeriod("Period 1", startTime, startTime + periodDuration);
            startTime = startTime + periodDuration + intervalDuration;
            newGame.Periods.AddPeriod("Period 1", startTime, startTime + periodDuration);

            GamePeriodStatus status = GamePeriodStatus.Pending;
            if ((string)game["status"] == "active") status = GamePeriodStatus.Active;
            else if ((string)game["status"] == "complete") status = GamePeriodStatus.Ended;

            newGame.Periods[0].Status = status;
            newGame.Periods[1].Status = status;

            newGame.CalculateResult();

            return newGame;
        }

        public string GetRequestAsString(string urlSuffix)
        {
            HttpResponseMessage response = _httpClient.GetAsync(new Uri(_baseUrl + urlSuffix)).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        public JObject GetRequestAsJObject(string urlSuffix)
        {
            string result = GetRequestAsString(urlSuffix);
            if (!String.IsNullOrWhiteSpace(result))
            {
                return JObject.Parse(result);
            }
            return null;
        }
    }
}
