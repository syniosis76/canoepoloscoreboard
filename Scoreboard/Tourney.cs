﻿using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        private readonly string _baseUrl = null;
        private Window _owner = null;
        private readonly Score _score = null;

        private readonly Queue<Game> _uploadQueue = new Queue<Game>();

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

        private List<string> _events = new List<string>();

        public List<string> Events { get { return _events;}}
        private string _googleToken;
        public string GoogleToken { get { return _googleToken; } }

        public Tourney(Score score)
        {
            //_baseUrl = "http://localhost:8000"; // Testing 
            _baseUrl = Properties.Settings.Default.TourneyUrl;

            if (Tourney._httpClient == null)
            {
                Tourney._httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(20)
                };
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

        public void ClearQueue()
        {
            _uploadQueue.Clear();
        }

        public void ProcessQueue()
        {
            if (!String.IsNullOrWhiteSpace(_score.Games.PitchId) && !_queueIsProcessing)
            {
                if (_uploadQueue.Count > 0)
                {
                    AddEvent($"Uploading {_uploadQueue.Count} games.");
                }
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
                AddEvent($"Uploading Game {game.Team1} vs {game.Team2}");
                string gameJson = GetGameJson(game);
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                string message = "";

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate {                    
                    statusCode = UploadGame(url, gameJson, out message);                    
                };
                worker.RunWorkerCompleted += delegate
                {
                    if (statusCode == HttpStatusCode.OK)
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
                        AddEvent($"Error: {message}");
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

        public void RefreshAuthentication()
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.GoogleJwt))
            {
                var googleJwt = Properties.Settings.Default.GoogleJwt;

                if (ValidateJwt(googleJwt))                
                {    
                    _googleToken = googleJwt;
                    _authenticated = true;

                    SetAuthenticationHeaders();
                }                
            }
        }

        public void Authenticate()
        {
            // Check if previously logged in.
            if (!_authenticated)
            {
                RefreshAuthentication();
            }

            // If still not logged in Log In.
            if (!_authenticated)
            {                                 
                using FileStream stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read);
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var scopes = new[] { "profile", "email" };
                var user = "profile2";
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, user, CancellationToken.None).Result;
                var googleJwt = credential.Token.IdToken;                                

                if (ValidateJwt(googleJwt))
                {
                    // Immediately Revoke Token but keep JWT.
                    bool result = credential.RevokeTokenAsync(CancellationToken.None).Result;

                    _authenticated = true;
                    _googleToken = googleJwt;
                    Properties.Settings.Default.GoogleJwt = _googleToken;                    
                    Properties.Settings.Default.Save();
                    SetAuthenticationHeaders();  
                }
            }
        }

        public void LogOut()
        {
            _authenticated = false;
            _googleToken = null;
            Properties.Settings.Default.GoogleJwt = "";
            Properties.Settings.Default.Save();

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        internal static string Base64UrlToString(string base64Url) =>
            Encoding.UTF8.GetString(Base64UrlDecode(base64Url));

        internal static byte[] Base64UrlDecode(string base64Url)
        {
            var base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }


        public bool ValidateJwt(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length != 3)
                {
                    return false;
                }
                var encodedPayload = parts[1];
                var payload = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<Google.Apis.Auth.GoogleJsonWebSignature.Payload>(Base64UrlToString(encodedPayload));

                if (!String.IsNullOrEmpty(payload.Email))
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString()); 
            }

            return false;
        }

        public void SetAuthenticationHeaders()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GoogleToken);
            _httpClient.DefaultRequestHeaders.Remove("TOURNEYCLIENT");
            _httpClient.DefaultRequestHeaders.Add("TOURNEYCLIENT", "Scoreboard");
        } 

        public static void ApplyToGame(JObject gameObject, Game game)
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

        private static Game CreateFromTourneyGame(string gameTime, JObject game)
        {
            Game newGame = new Game();
            ApplyToGame(game, newGame);

            DateTime startTime = Score.ParseTime(gameTime);

            if (!int.TryParse(Properties.Settings.Default.NumberOfPeriods, out int numberOfPeriods))
            {
                numberOfPeriods = 2;
            }
            TimeSpan periodDuration = Score.ParseTimeSpan(Properties.Settings.Default.PeriodDuration);
            TimeSpan intervalDuration = Score.ParseTimeSpan(Properties.Settings.Default.IntervalDuration);

            for (int period = 1; period <= numberOfPeriods; period++)
            {
                newGame.Periods.AddPeriod("Period " + period.ToString(), startTime, startTime + periodDuration);
                startTime = startTime + periodDuration + intervalDuration;
            }

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
            // Authenticate            
            BackgroundWorker authenticateWorker = new BackgroundWorker();
            authenticateWorker.DoWork += delegate {
                if (!_authenticated) Authenticate();                
            };
            authenticateWorker.RunWorkerCompleted += delegate
            {
                if (_authenticated)
                {
                    // List Tournaments
                    ProcessingWindow.ShowProcessing(_owner, "Listing Tournaments...");

                    BackgroundWorker listWorker = new BackgroundWorker();
                    listWorker.DoWork += delegate {
                        _tournaments = GetRequestAsJObject("/data/tournaments?admin=1");
                    };
                    listWorker.RunWorkerCompleted += delegate
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
                    listWorker.RunWorkerAsync();
                }
                ProcessingWindow.HideProcessing();
            };
            authenticateWorker.RunWorkerAsync();                        
        }

        private void SelectTournament()
        {
            SelectListWindow selectListWindow = new SelectListWindow
            {
                Owner = _owner,
                Title = "Select Tournament"
            };

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
            SelectListWindow selectListWindow = new SelectListWindow
            {
                Owner = _owner,
                Title = "Select Pitch"
            };

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

            JArray gameTimes;
            
            if (_pitch.ContainsKey("gameTimes") && _pitch["gameTimes"].HasValues)
            {
                gameTimes = (JArray)_pitch["gameTimes"];
            }
            else
            {
                gameTimes = (JArray)_gameDate["gameTimes"];
            }

            JArray games = (JArray)_pitch["games"];

            int count = Math.Min(games.Count, gameTimes.Count);
            for (int gameIndex = 0; gameIndex < count; gameIndex++)
            {
                string gameTime = (string)gameTimes[gameIndex];
                JObject game = (JObject)games[gameIndex];

                newGames.Add(CreateFromTourneyGame(gameTime, game));
            }

            ClearQueue();
            _score.Games.ClearGames();
            _score.Games.TournamentId = _tournamentId;
            _score.Games.GameDateId = _gameDateId;
            _score.Games.PitchId = _pitchId;
            _score.AddGames(newGames);
        }

        public static string GetGameJson(Game game)
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
                if (!gameEvent.EventType.Equals("Paused")
                        & !gameEvent.EventType.Equals("Resumed")
                        & !gameEvent.EventType.Contains("Shot"))
                {
                    JObject logEvent = new JObject
                    {
                        ["Time"] = gameEvent.Time,
                        ["EventType"] = gameEvent.EventType,
                        ["Team"] = gameEvent.Team,
                        ["Player"] = gameEvent.Player,
                        ["Notes"] = gameEvent.Notes
                    };
                    log.Add(logEvent);
                }
            }

            return data.ToString();
        }

        public static HttpStatusCode UploadGame(string url, string gameJson, out string message)
        {
            HttpContent content = new StringContent(gameJson, Encoding.UTF8, "application/json");            
            try
            {
                HttpResponseMessage response = _httpClient.PutAsync(new Uri(url), content).Result;
                message = response.Content.ReadAsStringAsync().Result;
                if (String.IsNullOrWhiteSpace(message))
                {
                    message = response.ReasonPhrase;
                }
                else if (message.StartsWith("{\"message\"=\""))
                {
                    message = message.Substring(12, message.Length - 14);
                }
                
                return response.StatusCode;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return HttpStatusCode.InternalServerError;
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

        public static JObject GetPitch(JObject tournament, string pitchId)
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

        public void AddEvent(string eventText)
        {            
            _events.Add(DateTime.Now.ToString("HH:mm:ss") + ": " + eventText);
            Console.WriteLine(eventText);

            // Keep at most 50 events
            if (_events.Count > 50)
            {
                _events.RemoveAt(0);
            }
        }
    }
}
