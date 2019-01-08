using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Scoreboard
{
    public class Tourney
    {
        private static HttpClient _httpClient = new HttpClient();

        private string _baseUrl = null;
        private Window _owner = null;
        private Score _score = null;

        private JObject _tournaments = null;
        private JObject _tournament = null;

        private string _tournamentId = null;
        private string _pitchId = null;

        private JObject _gameDate = null;
        private JObject _pitch = null;

        private bool _authenticated = false;
        public bool Authenticated { get { return _authenticated; } }

        private string _googleToken;
        public string GoogleToken { get { return _googleToken; } }

        public Tourney(Window owner, string baseUrl, Score score)
        {
            _owner = owner;
            _baseUrl = baseUrl;
            _score = score;
            if (Tourney._httpClient == null)
            {
                Tourney._httpClient = new HttpClient();
            }
        }        

        private string GetRequestAsString(string urlSuffix)
        {
            HttpResponseMessage response = _httpClient.GetAsync(new Uri(_baseUrl + urlSuffix)).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        private JObject GetRequestAsJObject(string urlSuffix)
        {
            string result = GetRequestAsString(urlSuffix);
            if (!String.IsNullOrWhiteSpace(result))
            {
                return JObject.Parse(result);
            }
            return null;
        }

        public void Authenticate()
        {
            if (!_authenticated)
            {
                UserCredential credential;
                using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, new[] { "profile" }, "user", CancellationToken.None).Result;

                    _authenticated = true;
                    _googleToken = credential.Token.IdToken;
                }
            }
        }

        public void SelectAndAddGames()
        {
            if (!_authenticated) Authenticate();

            ProcessingWindow.ShowProcessing(_owner, "Listing Tournaments...");

            // List Tournaments
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate {
                _tournaments = GetRequestAsJObject("/data/tournaments");
            };
            worker.RunWorkerCompleted += delegate
            {
                ProcessingWindow.HideProcessing();
                if (_tournaments != null)
                {
                    SelectTournament();
                }
                else
                {
                    MessageBox.Show("Unable to list Tournaments.");
                }
            };
            worker.RunWorkerAsync();
        }

        private void SelectTournament()
        {                                      
            SelectListWindow selectListWindow = new SelectListWindow();
            selectListWindow.Owner = _owner;
            selectListWindow.Title = "Select Tournament";

            foreach (JObject tournament in _tournaments["tournaments"])
            {
                selectListWindow.Items.Add(new SelectItem((string)(tournament["id"]["value"]), (string)tournament["name"]));
            }                

            if (selectListWindow.ShowDialog() == true)
            {
                _tournamentId = selectListWindow.SelectedId;

                ProcessingWindow.ShowProcessing(_owner, "Listing Games...");

                // List Pitches
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate {
                    _tournament = GetRequestAsJObject("/data/tournament/" + _tournamentId);
                };
                worker.RunWorkerCompleted += delegate
                {
                    ProcessingWindow.HideProcessing();
                    if (_tournament != null)
                    {
                        SelectPitch();
                    }
                    else
                    {
                        MessageBox.Show("Unable to list Games.");
                    }
                };
                worker.RunWorkerAsync();
            }            
        }

        public void SelectPitch()
        {
            SelectListWindow selectListWindow = new SelectListWindow();
            selectListWindow.Owner = _owner;
            selectListWindow.Title = "Select Pitch";

            foreach (JObject gameDate in _tournament["gameDates"])
            {
                string gameDateName = ((DateTime)(gameDate["date"]["value"])).ToString("ddd MMM dd yyyy");
                foreach (JObject pitch in gameDate["pitches"])
                {
                    selectListWindow.Items.Add(new SelectItem((string)(pitch["id"]["value"]), gameDateName + " " + (string)pitch["name"]));
                }
            }

            if (selectListWindow.ShowDialog() == true)
            {
                _pitchId = selectListWindow.SelectedId;

                foreach (JObject gameDate in _tournament["gameDates"])
                {
                    foreach (JObject pitch in gameDate["pitches"])
                    {
                        if ((string)(pitch["id"]["value"]) == _pitchId)
                        {
                            _gameDate = gameDate;
                            _pitch = pitch;
                            break;
                        }
                    }
                }

                if (_pitch != null)
                {
                    AddGames();
                }
            }            
        }

        private void AddGames()
        {
            GameList newGames = new GameList();

            JArray gameTimes = (JArray)_gameDate["gameTimes"];
            JArray games = (JArray)_pitch["games"];
            for (int gameIndex = 0; gameIndex < games.Count; gameIndex++)
            {
                string gameTime = (string)gameTimes[gameIndex];
                JObject game = (JObject)games[gameIndex];
                newGames.Add(CreateFromTourneyGame(gameTime, game));
            }

            _score.Games.Clear();
            _score.AddGames(newGames);
        }

        private Game CreateFromTourneyGame(string gameTime, JObject game)
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

            string gameStatus = (string)game["status"];
            GamePeriodStatus status = GamePeriodStatus.Ended;
            if (gameStatus == "pending") status = GamePeriodStatus.Pending;
            else if (gameStatus == "active") status = GamePeriodStatus.Active;

            newGame.Periods[0].Status = status;
            newGame.Periods[1].Status = status;

            newGame.CalculateResult();

            return newGame;
        }        
    }
}
