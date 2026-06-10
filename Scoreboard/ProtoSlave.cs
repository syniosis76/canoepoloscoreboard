using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using System.Net.Http;

namespace Scoreboard
{
    public class ProtoSlave
    {
        private WebsocketClient _webSocket;
        private HttpClient _httpClient;
        private string _address;
        private string _port;
        private ConcurrentDictionary<string, string> _valueCache;        
        private int _siren = 0;

        public ProtoSlave()        
        {
          //_address = "192.168.0.1";          
          _address = "localhost";
          _port = "9090";          

          _valueCache = new ConcurrentDictionary<string, string>();

          _httpClient = new HttpClient();

          InitialiseWebSocket();
        }

        public void Dispose()
        {
          if (_webSocket != null) 
          {            
            _webSocket.Dispose();
          }
        }

        public void InitialiseWebSocket()
        {
          Uri serverUri = new Uri("ws://" + _address + ":" + _port);

          _webSocket = new WebsocketClient(serverUri);

          _webSocket.ReconnectTimeout = TimeSpan.FromSeconds(5);
          _webSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);
          _webSocket.ReconnectionHappened.Subscribe(info => AddEvent($"Proto: Reconnection: {info.Type}"));
          _webSocket.DisconnectionHappened.Subscribe(info => AddEvent($"Proto: Disconnection: {info.Type}"));

          _webSocket.MessageReceived.Subscribe(msg =>
          {            
            string message = "NULL";
            if (msg.Binary != null)
            {
              message = System.Text.Encoding.UTF8.GetString(msg.Binary, 0, msg.Binary.Length);            
            }
            else if (msg.Text != null)
            {
              message = msg.Text;
            }
            AddEvent($"Proto: Message received: {message}");
          });
        }

        public void ConnectWebSocket()
        {
          try
          {
            AddEvent("Proto: Starting");
            if (_webSocket.IsStarted)
            {
              try
              {
                _webSocket.StopOrFail(WebSocketCloseStatus.NormalClosure, "Close for reconnect.");
              }
              catch (Exception exception)
              {
                AddEvent($"Proto: Failed to stop: {exception.Message}");
              }
            }
            _webSocket.StartOrFail();            
            AddEvent("Proto: Started");
          }
          catch (Exception exception)
          {
            AddEvent($"Proto: Failed to start: {exception.Message}");
          }
        }

        public void CheckAndConnectWebSocket()
        {
          if (!_webSocket.IsRunning)
          {
            ConnectWebSocket();
          }
        }

        public void ResetUpdates()
        {
          _valueCache.Clear();
        }

        public bool SendCommand(string command)
        {          
          if (_webSocket.IsRunning)
          {
            AddEvent($"Proto: Command: {command}");

            //Task.Run(() => _webSocket.Send(command));            
            //_webSocket.Send(command);
            _webSocket.SendInstant(command);

            return true;           
          }

          return false;
        }     

        public void SendUpdate(string prefix, string value, string suffix)
        {
          string name = prefix + "/" + suffix;
          string cachedValue = _valueCache.GetValueOrDefault(name);

          if (cachedValue != value)
          {            
            if (SendCommand(prefix + value + suffix))
            {            
              _valueCache[name] = value;
            }
          }
        }        

        public void SendHttpCommand(string command)
        {                    
          string url = $"https://{_address}{command}";
          Console.WriteLine(url);

          try
          {
              Task<HttpResponseMessage> getTask = _httpClient.GetAsync(url);

              getTask.ContinueWith(task =>
              {
                  if (task.IsFaulted)
                  {
                      Console.WriteLine($"Error in background request: {task.Exception?.InnerException?.Message}");
                  }
                  else
                  {
                      using var response = task.Result;
                      response.EnsureSuccessStatusCode(); // Throws if not a success code.
                      Console.WriteLine($"Background request to {url} completed with status: {response.StatusCode}");
                  }
              });
              
              Console.WriteLine("Main thread continues immediately after starting the GET request.");
          }
          catch (Exception ex)
          {
              Console.WriteLine($"Synchronous error starting the request: {ex.Message}");
          }
        }

        public void SendHttpUpdate(string prefix, string value)
        {
          string cachedValue = _valueCache.GetValueOrDefault(prefix);

          if (cachedValue != value)
          {            
            SendHttpCommand(prefix + value);            
            _valueCache[prefix] = value;            
          }
        }        

        public void SendGameAsync(IGameDisplay game, Boolean sendAll)
        {
          ThreadPool.QueueUserWorkItem(delegate
          {
              if (game is SwappedGame swapped)
              {
                  SendGame(swapped, sendAll);
              }
              else if (game is Game g)
              {
                  SendGame(g, sendAll);
              }
          }, null);
        }

        public void SendGame(IGameDisplay game, Boolean sendAll)
        {
          if (sendAll)
          {
            ResetUpdates();
            AddEvent("SendGame All");            
          }
          else
          {
            AddEvent("SendGame");
          }                    

          CheckAndConnectWebSocket();            

          if (game == null)
          {
              SendEmptyGame();
              return;
          }

          int minutes = 0;
          int seconds = 0;
          int shotSeconds = 0;
          string timeColour = "#ffffff";
          string shotColour = "#ff8888";
          string team1Score = "-";
          string team2Score = "-";
          string scoreColour = "#88ff88";
          string team1Name = "";
          string team2Name = "";
          int periodIndex = 0;
          string periodNameVisible = "false";

          if (game.Periods.CurrentPeriod != null)
          {
              IGameDisplay timeGame = game;
              if (game.Parent != null && game.Parent.Parent != null)
              {
                timeGame = game.Parent.Parent.CurrentGame;
              }
              TimeSpan timeRemaining = timeGame.Periods.CurrentPeriod.TimeRemaining;
              minutes = timeRemaining.Minutes;
              seconds = timeRemaining.Seconds;

              periodIndex = game.Periods.IndexOf(game.Periods.CurrentPeriod) + 1;
              periodNameVisible = "true";

              if (game.Periods.CurrentPeriod.Status != GamePeriodStatus.Active)
              {
                  timeColour = "#ff8888";
              }
          }

          if (game.Parent != null && game.Parent.Parent != null)
          {
              shotSeconds = game.Parent.Parent.ShotDisplayTime;
          }

          team1Score = $"{game.Team1Score}";
          team2Score = $"{game.Team2Score}";

          if (game.Team1 != null && game.Team1.Length > 0)
          {
              team1Name = game.Team1[0..Math.Min(game.Team1.Length, 10)];
          }
          if (game.Team2 != null && game.Team2.Length > 0)
          {
              team2Name = game.Team2[0..Math.Min(game.Team2.Length, 10)];
          }

          SendUpdate($"SLAVE,sendText,Seconds,", $"{seconds:D2}", $",CS");
          if (_siren == 1)
          {
              _siren = 2;
              SendCommand("SLAVE,Siren,true,true,CS");
          }
          else if (_siren == 2)
          {
              _siren = 0;
              SendCommand("SLAVE,Siren,false,false,CS");
          }
          SendUpdate($"SLAVE,sendText,intShotclock,", $"{shotSeconds:D2}", $",CS");
          SendUpdate($"SLAVE,sendText,Shotclock,", $"{shotSeconds:D2}", $",CS");
          SendUpdate($"SLAVE,sendText,Minutes,", $"{minutes:D2}", $",CS");

          SendUpdate($"SLAVE,configText,intShotclock,", $"{shotColour}", $",CS");
          SendUpdate($"SLAVE,configText,Shotclock,", $"{shotColour}", $",CS");

          SendUpdate($"SLAVE,configText,Minutes,", $"{timeColour}", $",CS");
          SendUpdate($"SLAVE,configText,ColonS,", $"{timeColour}", $",CS");
          SendUpdate($"SLAVE,configText,Seconds,", $"{timeColour}", $",CS");

          SendUpdate($"SLAVE,sendText,ScoreA,", $"{team1Score}", $",CS");
          SendUpdate($"SLAVE,sendText,ScoreB,", $"{team2Score}", $",CS");
          SendUpdate($"SLAVE,configText,ScoreA,", $"{scoreColour}", $",CS");
          SendUpdate($"SLAVE,configText,ScoreB,", $"{scoreColour}", $",CS");

          SendUpdate($"SLAVE,sendText,NameTeamA,", $"{team1Name}", $",CS");
          SendUpdate($"SLAVE,sendText,NameTeamB,", $"{team2Name}", $",CS");

          SendUpdate($"SLAVE,setText,PeriodName", $",{periodNameVisible}", $",CS");
          SendUpdate($"SLAVE,sendText,PeriodName", $",P{periodIndex}", $",CS");

          SendHttpUpdate("/Scripts/setBrightness.php?Brightness=", "99");            
        }

        private void SendEmptyGame()
        {
            SendUpdate($"SLAVE,sendText,Seconds,", "00", $",CS");
            SendUpdate($"SLAVE,sendText,intShotclock,", "00", $",CS");
            SendUpdate($"SLAVE,sendText,Shotclock,", "00", $",CS");
            SendUpdate($"SLAVE,sendText,Minutes,", "00", $",CS");
            
            SendUpdate($"SLAVE,sendText,ScoreA,", "-", $",CS");
            SendUpdate($"SLAVE,sendText,ScoreB,", "-", $",CS");
            SendUpdate($"SLAVE,sendText,NameTeamA,", "", $",CS");
            SendUpdate($"SLAVE,sendText,NameTeamB,", "", $",CS");
            SendUpdate($"SLAVE,setText,PeriodName", ",false", $",CS");

            SendHttpUpdate("/Scripts/setBrightness.php?Brightness=", "99");
        }

        public void PlaySiren()
        {
          _siren = 1;
        }

        public void AddEvent(string eventText)
        {            
            string timeAndEventText = DateTime.Now.ToString("HH:mm:ss.fff") + ": " + eventText;
            //_events.Add();
            Console.WriteLine(timeAndEventText);

            // Keep at most 50 events
            /*if (_events.Count > 50)
            {
                _events.RemoveAt(0);
            }*/
        }
    }
}