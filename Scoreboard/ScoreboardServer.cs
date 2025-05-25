using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using Utilities;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Enumeration;
using System.Security.Policy;
using System.Threading;

namespace Scoreboard
{
    public class ScoreboardServer
    {
        Score _score;
        WebServer _webServer;        

        Dictionary <string, string> _contentCache;

        // netsh http add urlacl url="http://+:8080/" user=everyone
        // netsh advfirewall firewall add rule name="Scoreboard" dir=in action=allow protocol=TCP localport=8080

        public ScoreboardServer(Score score)
        {
            _score = score;
            _contentCache = new Dictionary<string, string>();            

            _webServer = new WebServer(score.ServerOptions.Port);

            _webServer.AddMethod(String.Empty, delegate (HttpListenerRequest request) { return CachedFileContent("index.html"); });            
            _webServer.AddMethod("about", AboutMethod);
            _webServer.AddMethod("game", GameMethod);
            _webServer.AddMethod("game-info", GameInfoMethod);
            _webServer.AddMethod("results", ResultsMethod);
            _webServer.AddMethod("shot-clock-time", ShotClockTimeMethod);            

            _webServer.AddMethod("executeTeam1ScoreUp", ExecuteTeam1ScoreUp);
            _webServer.AddMethod("executeTeam1ScoreDown", ExecuteTeam1ScoreDown);
            _webServer.AddMethod("executeTeam2ScoreUp", ExecuteTeam2ScoreUp);
            _webServer.AddMethod("executeTeam2ScoreDown", ExecuteTeam2ScoreDown);
            _webServer.AddMethod("executePlayPause", ExecutePlayPause);
            _webServer.AddMethod("executeShotClockReset", ExecuteShotClockReset);
            _webServer.AddMethod("executeShotClockPlayPause", ExecuteShotClockPlayPause);

            _webServer.AddMethod("replace-team-names", ExecuteReplaceTeamNames);
            _webServer.AddMethod("add-game", ExecuteAddGame);
            _webServer.AddMethod("clear-games", ExecuteClearGames);

            _webServer.DefaultMethod = FileContentMethod;               

            _webServer.Run();     
        }

        ~ScoreboardServer()
        {
            Stop();
        }

        public void Stop()
        {            
            _webServer.Stop();
        }

        public static string HtmlResponse(string content)
        {
            return "<html>\n"
                + "<head>\n"
                + "    <title>Scoreboard Server</title>\n"
                + "    <link rel=\"stylesheet\" type=\"text/css\" href=\"scoreboard.css\">\n"
                + "</head>\n"                
                + "<body>\n"
                + content + "\n"
                + "</body>\n"
                + "</html>";
        }

        public string AboutMethod(HttpListenerRequest request)
        {
            return HtmlResponse("Scoreboard");
        }

        public string GameMethod(HttpListenerRequest request)
        {
            if (_score.CurrentOrEndedGame == null)
            {
                return "null";
            }
            else
            {
                return _score.CurrentOrEndedGame.ToJson();
            }
        }
        public string GameInfoMethod(HttpListenerRequest request)
        {
            if (_score.CurrentOrEndedGame == null)
            {
                return "[]";
            }
            else
            {
                return "[" + _score.CurrentOrEndedGame.ToJson() + "]";
            }
        }

        public string ResultsMethod(HttpListenerRequest request)
        {
            StringBuilder result = new StringBuilder();
            result.Append("[ ");
            bool firstGame = true;

            foreach (Game game in _score.Games)
            {                
                if (firstGame)
                {
                    firstGame = false;
                }
                else
                {
                    result.Append(", ");
                }
                result.Append(game.ToJson());
            }
            result.Append(" ]");

            return result.ToString();
        }

        public string ShotClockTimeMethod(HttpListenerRequest request)
        {
            return "{\"shot-clock-time\": " + _score.ShotTime.ToString() + "}";
        }

        public string ExecuteTeam1ScoreUp(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.Team1Goal(String.Empty); }));    
            return "OK";
        }

        public string ExecuteTeam1ScoreDown(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.Team1NoGoal(); }));    
            return "OK";
        }

        public string ExecuteTeam2ScoreUp(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.Team2Goal(String.Empty); }));    
            return "OK";
        }

        public string ExecuteTeam2ScoreDown(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.Team2NoGoal(); }));    
            return "OK";
        }

        public string ExecutePlayPause(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.PauseResume(); }));                            
            
            return "OK";
        }

        public string ExecuteShotClockReset(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.ResetShotTime(); }));
            return "OK";
        }

        public string ExecuteShotClockPlayPause(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.PauseResumeShotTime(); }));
            return "OK";
        }

        public string ExecuteReplaceTeamNames(HttpListenerRequest request)
        {             
            using (StreamReader streamReader = new StreamReader(request.InputStream))
            {
                string postData = streamReader.ReadToEnd();
                if (postData.Length > 0)
                {
                    Dictionary<string, string> teamNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(postData);
                    Application.Current.Dispatcher.Invoke(new Action(() => { _score.ReplaceTeamNames(teamNames); }));
                }
            }

            return "{ \"Response\": \"OK\" }";
        }

        public string ExecuteAddGame(HttpListenerRequest request)
        {
            using (StreamReader streamReader = new StreamReader(request.InputStream))
            {
                string postData = streamReader.ReadToEnd();
                dynamic game = JObject.Parse(postData);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {                    
                    Game newGame = new Game(game.pool == null ? string.Empty : game.pool.Value, game.team1.Value, game.team2.Value, null);                    

                    foreach (dynamic period in game.periods)
                    {
                        DateTime startTime = Score.ParseTime(period.startTime.Value);
                        DateTime endTime = Score.ParseTime(period.endTime.Value);
                        newGame.Periods.AddPeriod(period.name.Value, startTime, endTime);
                    }

                    _score.Games.Add(newGame);
                }));
            }

            return "OK";
        }

        public string ExecuteClearGames(HttpListenerRequest request)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.Games.ClearGames(); }));
            return "OK";
        }

        public string FileContentMethod(HttpListenerRequest request)
        {
            string url = request.RawUrl.TrimStart('/');

            if (url == "favicon.ico") 
            {
                return "Invalid Request";
            }
            else
            {
                return CachedFileContent(url);
            }
        }

        public string CachedFileContent(string url)
        {
            string content;

            if (_contentCache.ContainsKey(url))
            {
                content = _contentCache[url]; // Read Content from Cache
            }
            else
            {
                string fileName = url;
                if (!fileName.Contains('.'))
                {
                    fileName += ".html";
                }
                        
                if (fileName.EndsWith("/"))
                {
                    fileName = fileName[..^1];
                }

                Console.WriteLine($"Reading File: {fileName}");  
                if (WebServer.IsBinaryFile(url))
                {
                    byte[] bytes = File.ReadAllBytes("pages\\" + fileName);
                    content = Convert.ToBase64String(bytes);
                }              
                else
                {
                    content = File.ReadAllText("pages\\" + fileName);
                }
                _contentCache.Add(url, content);
            }

            return content;
        }

        public void SendWebSocketMessage(string message)
        {
            _webServer.SendWebSocketMessage(message);
        }

        public void SendGame(Game game)
        {
            if (game != null)
            {
                
                string gameJson = game.ToJson();
                SendWebSocketMessage(gameJson);                
            }            
        }

        public void SendGameAsync(Game game)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SendGame(game);
            }, null);
        }
    }
}