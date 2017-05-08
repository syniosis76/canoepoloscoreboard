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

namespace Scoreboard
{
    class ScoreboardServer
    {
        Score _score;
        WebServer _webServer;

        string _scoreboardCss;
        string _scoreboardJs;
        string _homePage;
        string _shotClockPage;
        string _scoreboardPage;
        string _controllerPage;
        string _resultsPage;
        string _resultsCss;
        string _statisticsPage;

        // netsh http add urlacl url="http://+:8080/" user=everyone
        // netsh advfirewall firewall add rule name="Scoreboard" dir=in action=allow protocol=TCP localport=8080

        public ScoreboardServer(Score score)
        {
            _score = score;
                      
            _webServer = new WebServer(score.ServerOptions.Port);
            _webServer.AddMethod(String.Empty, HomeMethod);
            _webServer.AddMethod("scoreboard.css", ScoreboardCssMethod);
            _webServer.AddMethod("scoreboard.js", ScoreboardJsMethod);
            _webServer.AddMethod("about", AboutMethod);
            _webServer.AddMethod("game", GameMethod);
            _webServer.AddMethod("results", ResultsMethod);
            _webServer.AddMethod("results-page", ResultsPageMethod);
            _webServer.AddMethod("results.css", ResultsCssMethod);
            _webServer.AddMethod("statistics", StatisticsPageMethod);
            _webServer.AddMethod("scoreboard", ScoreboardMethod);
            _webServer.AddMethod("controller", ControllerMethod);
            _webServer.AddMethod("shot-clock", ShotClockMethod);
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

        public string HomeMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_homePage))
            {
                _homePage = File.ReadAllText("pages\\home.html");
            }

            return _homePage;
        }

        public string ScoreboardCssMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_scoreboardCss))
            {
                _scoreboardCss = File.ReadAllText("pages\\scoreboard.css");
            }
             
            return _scoreboardCss;
        }

        public string ScoreboardJsMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_scoreboardJs))
            {
                _scoreboardJs = File.ReadAllText("pages\\scoreboard.js");
            }

            return _scoreboardJs;
        }

        public string AboutMethod(HttpListenerRequest request)
        {
            return HtmlResponse("Scoreboard");
        }

        public string GameMethod(HttpListenerRequest request)
        {
            if (_score.CurrentGame == null)
            {
                return "null";
            }
            else
            {
                return _score.CurrentGame.ToJson();
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

        public string ResultsPageMethod(HttpListenerRequest request)
        {
            if(String.IsNullOrEmpty(_resultsPage))
            {
                _resultsPage = File.ReadAllText("pages\\results.html");
            }

            return _resultsPage;
        }

        public string ResultsCssMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_resultsCss))
            {
                _resultsCss = File.ReadAllText("pages\\results.css");
            }

            return _resultsCss;
        }

        public string StatisticsPageMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_statisticsPage))
            {
                _statisticsPage = File.ReadAllText("pages\\statistics.html");
            }

            return _statisticsPage;
        }

        public string ShotClockMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_shotClockPage))
            {
                _shotClockPage = File.ReadAllText("pages\\shot-clock.html");
            }

            return _shotClockPage;
        }

        public string ShotClockTimeMethod(HttpListenerRequest request)
        {
            return "{\"shot-clock-time\": " + _score.ShotTime.ToString() + "}";
        }

        public string ScoreboardMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_scoreboardPage))
            {
                _scoreboardPage = File.ReadAllText("pages\\scoreboard.html");
            }

            return _scoreboardPage;
        }

        public string ControllerMethod(HttpListenerRequest request)
        {
            if (String.IsNullOrEmpty(_controllerPage))
            {
                _controllerPage = File.ReadAllText("pages\\controller.html");
            }

            return _controllerPage;
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
            Application.Current.Dispatcher.Invoke(new Action(() => { _score.Games.Clear(); }));
            return "OK";
        }

    }
}