using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        private Queue<Game> _uploadQueue = new Queue<Game>();

        private JObject _tournaments = null;
        private JObject _tournament = null;

        private string _tournamentId = null;
        private string _gameDateId = null;
        private string _pitchId = null;

        private JObject _gameDate = null;
        private JObject _pitch = null;

        private volatile bool _queueIsProcessing = false;

        private bool _authenticated = false;
        public bool Authenticated { get { return _authenticated; } }

        private string _googleToken;
        public string GoogleToken { get { return _googleToken; } }

        public Tourney(Score score)
        {
#if DEBUG
            _baseUrl = "http://localhost:8000"; // Testing 
#else
            _baseUrl = Properties.Settings.Default.TourneyUrl;
#endif
            if (Tourney._httpClient == null)
            {
                Tourney._httpClient = new HttpClient();
                Tourney._httpClient.Timeout = TimeSpan.FromSeconds(20);
            }
            _score = score;
        }

        public void LoadGames(Window owner)
        {
            _owner = owner;
            SelectPitchAndAddGames();
        }

        public void ApplyGame(Game game, bool send = true)
        {
            if (!String.IsNullOrWhiteSpace(_score.Games.PitchId))
            {                
                if (!_uploadQueue.Contains(game))
                {
                    game.NeedsSending = true;
                    _uploadQueue.Enqueue(game);
                }                
                if (send)
                {
                    ProcessQueue();
                }
            }
        }

        public void ProcessQueue()
        {
            if (!String.IsNullOrWhiteSpace(_score.Games.PitchId) && !_queueIsProcessing)
            {
                _queueIsProcessing = true;
                ProcessQueueItem(); // This is recursive. 
            }
        }

        public void ProcessQueueItem()
        {
            if (_uploadQueue.Count == 0)
            {
                _queueIsProcessing = false;
            }
            else
            {
                Game game = _uploadQueue.Peek();
                string url = _baseUrl + "/data/tournament/" + _score.Games.TournamentId + "/date/" + _score.Games.GameDateId + "/pitch/" + _score.Games.PitchId + "/game/" + game.Id;
                string gameJson = GetGameJson(game);
                bool gameUploaded = false;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate {
                    gameUploaded = UploadGame(url, gameJson);                    
                };
                worker.RunWorkerCompleted += delegate
                {
                    if (gameUploaded)
                    {
                        if (_uploadQueue.Count > 0)
                        {
                            Game sentGame = _uploadQueue.Dequeue();
                            sentGame.NeedsSending = false;
                        }
                        ProcessQueueItem();
                    }
                    else
                    {
                        _queueIsProcessing = false;
                    }

                };
                worker.RunWorkerAsync();                
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
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, new[] { "profile" }, "profile", CancellationToken.None).Result;
                    var m = credential.GetAccessTokenForRequestAsync().Result;

                    _authenticated = true;
                    _googleToken = credential.Token.IdToken;

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GoogleToken);
                    _httpClient.DefaultRequestHeaders.Add("TOURNEYCLIENT", "Scoreboard");
                }
            }
        }

        public void ApplyToGame(JObject gameObject, Game game)
        {
            game.Id = (string)(gameObject["id"]["value"]);
            game.Pool = (string)gameObject["group"];
            game.Team1 = (string)gameObject["team1"];
            //game.Team1Score = (int)gameObject["team1Score"];
            //newGame.Team1Points = (int)game["team1Points"];
            game.Team2 = (string)gameObject["team2"];
            //game.Team2Score = (int)gameObject["team2Score"];
            //newGame.Team2Points = (int)game["team2Points"];                                   
        }

        private Game CreateFromTourneyGame(string gameTime, JObject game)
        {
            Game newGame = new Game();
            ApplyToGame(game, newGame);

            DateTime startTime = Score.ParseTime(gameTime);
            TimeSpan periodDuration = Score.ParseTimeSpan("10");
            TimeSpan intervalDuration = Score.ParseTimeSpan("1");

            newGame.Periods.AddPeriod("Period 1", startTime, startTime + periodDuration);
            startTime = startTime + periodDuration + intervalDuration;
            newGame.Periods.AddPeriod("Period 2", startTime, startTime + periodDuration);

            string gameStatus = (string)game["status"];
            GamePeriodStatus status = GamePeriodStatus.Ended;
            if (gameStatus == "pending") status = GamePeriodStatus.Pending;
            else if (gameStatus == "active") status = GamePeriodStatus.Active;

            newGame.Periods[0].Status = status;
            newGame.Periods[1].Status = status;

            newGame.CalculateResult();

            return newGame;
        }        

        public void SelectPitchAndAddGames()
        {
            if (!_authenticated) Authenticate();

            ProcessingWindow.ShowProcessing(_owner, "Listing Tournaments...");

            // List Tournaments
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate {
                _tournaments = GetRequestAsJObject("/data/tournaments?admin=1");
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
                            _gameDateId = (string)_gameDate["id"]["value"];
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

            _score.Games.ClearGames();
            _score.Games.TournamentId = _tournamentId;
            _score.Games.GameDateId = _gameDateId;
            _score.Games.PitchId = _pitchId;            
            _score.AddGames(newGames);
        }

        public string GetGameJson(Game game)
        {
            string status = "pending";
            if (game.IsCurrentGame) status = "active";
            if (game.HasEnded || game.Result != GameResult.None) status = "complete";

            JObject data = new JObject();
            if (game.NameChanged)
            {
                data["group"] = game.Pool;
                data["team1"] = game.Team1;
                data["team2"] = game.Team2;
                game.NameChanged = false;
            }
            data["team1Score"] = game.Team1Score;
            data["team2Score"] = game.Team2Score;
            data["status"] = status;

            JArray log = new JArray();
            data["eventLog"] = log;

            foreach(GameEvent gameEvent in game.GameEvents)
            {
                JObject logEvent = new JObject();
                logEvent["Time"] = gameEvent.Time;
                logEvent["EventType"] = gameEvent.EventType;
                logEvent["Team"] = gameEvent.Team;
                logEvent["Player"] = gameEvent.Player;
                logEvent["Notes"] = gameEvent.Notes;
                log.Add(logEvent);
            }

            return data.ToString();
        }

        public bool UploadGame(string url, string gameJson)
        {
            HttpContent content = new StringContent(gameJson, Encoding.UTF8, "application/json");            
            try
            {
                HttpResponseMessage response = _httpClient.PutAsync(new Uri(url), content).Result;
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateGameDetails()
        {
            if (!String.IsNullOrWhiteSpace(_score.Games.PitchId))
            {
                JObject tournament = null;
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    tournament = GetRequestAsJObject("/data/tournament/" + _score.Games.TournamentId);
                };
                worker.RunWorkerCompleted += delegate
                {
                    if (tournament != null)
                    {
                        JObject pitch = GetPitch(tournament, _score.Games.PitchId);
                        if (pitch != null)
                        {
                            JArray games = (JArray)pitch["games"];
                            foreach (JObject gameObject in games)
                            {
                                Game game = _score.Games.GetGameById((string)gameObject["id"]["value"]);
                                if (game != null)
                                {
                                    ApplyToGame(gameObject, game);
                                    if (!game.IsCurrentGame)
                                    {
                                        game.CalculateResult();
                                    }
                                }                                
                            }
                        }
                    }
                };
                worker.RunWorkerAsync();
            }
        }

        public JObject GetPitch(JObject tournament, string pitchId)
        {
            foreach (JObject gameDate in tournament["gameDates"])
            {
                foreach (JObject pitch in gameDate["pitches"])
                {
                    if ((string)(pitch["id"]["value"]) == pitchId)
                    {
                        return pitch;
                    }
                }
            }
            return null;
        }
    }
}
