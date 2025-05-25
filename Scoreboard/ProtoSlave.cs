using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Scoreboard
{
    public class ProtoSlave
    {
        private WebsocketClient _webSocket;
        private string _address;
        private string _port;
        private ConcurrentDictionary<string, string> _valueCache;
        private DateTime _cacheTime;
        private int _siren = 0;

        public ProtoSlave()        
        {
          _address = "192.168.0.1";          
          //_address = "localhost";
          _port = "9090";          

          _valueCache = new ConcurrentDictionary<string, string>();

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
          _cacheTime = DateTime.Now;

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
          else if ((DateTime.Now - _cacheTime).TotalSeconds >= 5)
          {
            _valueCache.Clear();
            _cacheTime = DateTime.Now;
          }
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

        public void SendGame(Game game)
        {                              
          AddEvent("SendGame");
          CheckAndConnectWebSocket();

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

          if (game != null)
          {
            if (game.Periods.CurrentPeriod != null)
            {                
              minutes = game.Periods.CurrentPeriod.TimeRemaining.Minutes;
              seconds = game.Periods.CurrentPeriod.TimeRemaining.Seconds;
              
              periodIndex = game.Periods.IndexOf(game.Periods.CurrentPeriod) + 1;
              periodNameVisible = "true";

              if (game.Periods.CurrentPeriod.Status != GamePeriodStatus.Active)
              {
                timeColour = "#ff8888"; 
              }
            }            

            if (game.Parent != null && game.Parent.Parent != null)
            {
                shotSeconds = game.Parent.Parent.ShotTime;
            }             

            team1Score = $"{game.Team1Score}";
            team2Score = $"{game.Team2Score}";

            if (game.Team1.Length > 0)
            {
              team1Name = game.Team1[0..Math.Min(game.Team1.Length, 10)];
            }
            if (game.Team1.Length > 0)
            {
              team2Name = game.Team2[0..Math.Min(game.Team2.Length, 10)];
            }            
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
        }

        public void SendGameAsync(Game game)
        {
          ThreadPool.QueueUserWorkItem(delegate
          {
              SendGame(game);
          }, null);
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