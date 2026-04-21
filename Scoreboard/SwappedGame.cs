using System;
using System.ComponentModel;

namespace Scoreboard
{
    public class SwappedGame : INotifyPropertyChanged
    {
        private readonly Game _game;
        private readonly bool _swapped;
        private readonly string[] _teamProperties = { "Team1", "Team2", "Team1Score", "Team2Score", "Team1Color", "Team2Color", "Team1Flag", "Team2Flag", "Result", "ResultDescription" };

        public SwappedGame(Game game, bool swapped)
        {
            _game = game;
            _swapped = swapped;

            if (_game != null)
            {
                _game.PropertyChanged += OnGamePropertyChanged;
            }
        }

        private void OnGamePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName)) return;

            if (!_swapped)
            {
                NotifyPropertyChanged(e.PropertyName);
                return;
            }

            string mappedProperty = e.PropertyName switch
            {
                "Team1" => "Team2",
                "Team2" => "Team1",
                "Team1Score" => "Team2Score",
                "Team2Score" => "Team1Score",
                "Team1Color" => "Team2Color",
                "Team2Color" => "Team1Color",
                "Team1Flag" => "Team2Flag",
                "Team2Flag" => "Team1Flag",
                "Result" => "Result",
                "ResultDescription" => "ResultDescription",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(mappedProperty))
            {
                NotifyPropertyChanged(mappedProperty);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        public Game Game => _game;

        public bool Swapped => _swapped;

        public string Id => _game?.Id;
        public string Pool => _game?.Pool;

        public string Team1 => _swapped ? _game?.Team2 : _game?.Team1;
        public string Team2 => _swapped ? _game?.Team1 : _game?.Team2;

        public string Team1Original => _swapped ? _game?.Team2Original : _game?.Team1Original;
        public string Team2Original => _swapped ? _game?.Team1Original : _game?.Team2Original;

        public string Team1Flag => _swapped ? _game?.Team2Flag : _game?.Team1Flag;
        public string Team2Flag => _swapped ? _game?.Team1Flag : _game?.Team2Flag;

        public string Team1FlagImage
        {
            get
            {
                string flag = Team1Flag;
                if (!String.IsNullOrWhiteSpace(flag))
                {
                    return "/Scoreboard;component/flags/" + flag + ".png";
                }
                return String.Empty;
            }
        }

        public string Team2FlagImage
        {
            get
            {
                string flag = Team2Flag;
                if (!String.IsNullOrWhiteSpace(flag))
                {
                    return "/Scoreboard;component/flags/" + flag + ".png";
                }
                return String.Empty;
            }
        }

        public string Team1Color => _swapped ? _game?.Team2Color : _game?.Team1Color;
        public string Team2Color => _swapped ? _game?.Team1Color : _game?.Team2Color;

        public int Team1Score => _swapped ? _game?.Team2Score ?? 0 : _game?.Team1Score ?? 0;
        public int Team2Score => _swapped ? _game?.Team1Score ?? 0 : _game?.Team2Score ?? 0;

        public GameResult Result
        {
            get
            {
                if (_game == null) return GameResult.None;
                if (!_swapped) return _game.Result;

                return _game.Result switch
                {
                    GameResult.Team1 => GameResult.Team2,
                    GameResult.Team2 => GameResult.Team1,
                    _ => _game.Result
                };
            }
        }

        public int Team1Points => _swapped ? _game?.Team2Points ?? 0 : _game?.Team1Points ?? 0;
        public int Team2Points => _swapped ? _game?.Team1Points ?? 0 : _game?.Team2Points ?? 0;

        public string ResultDescription
        {
            get
            {
                if (_game == null) return "None";
                string t1 = Team1;
                string t2 = Team2;

                if (Result == GameResult.Draw) return "Draw";
                else if (Result == GameResult.Team1) return t1 + " Won";
                else if (Result == GameResult.Team2) return t2 + " Won";
                else return "None";
            }
        }

        public GamePeriodList Periods => _game?.Periods;

        public bool HasStarted => _game?.HasStarted ?? false;
        public bool HasEnded => _game?.HasEnded ?? false;

        public DateTime? StartTime => _game?.StartTime;
        public DateTime? EndTime => _game?.EndTime;

        public string CurrentPeriodDescription => _game?.CurrentPeriodDescription;
        public string CurrentPeriodTimeDescription => _game?.CurrentPeriodTimeDescription;

        public string ToJson()
        {
            if (_game == null) return "{}";

            var sb = new System.Text.StringBuilder();
            sb.Append('{');
            sb.Append("\"startTime\": \"" + (StartTime.HasValue ? StartTime.Value.ToString("HH:mm") : "--:--") + "\"");
            sb.Append(", \"team1\": \"" + Team1 + "\"");
            string t1Flag = _swapped ? _game?.Team2Flag : _game?.Team1Flag;
            if (!String.IsNullOrEmpty(t1Flag))
            {
                sb.Append(", \"team1Flag\": \"" + t1Flag + "\"");
            }
            sb.Append(", \"team1Score\": " + Team1Score);
            sb.Append(", \"team2\": \"" + Team2 + "\"");
            string t2Flag = _swapped ? _game?.Team1Flag : _game?.Team2Flag;
            if (!String.IsNullOrEmpty(t2Flag))
            {
                sb.Append(", \"team2Flag\": \"" + t2Flag + "\"");
            }
            sb.Append(", \"team2Score\": " + Team2Score);
            if (!String.IsNullOrEmpty(Pool))
            {
                sb.Append(", \"pool\": \"" + Pool + "\"");
            }
            else
            {
                sb.Append(", \"pool\": \"\"");
            }
            if (Periods?.CurrentPeriod != null)
            {
                sb.Append(", \"period\": \"" + Periods.CurrentPeriod.Name + "\"");
                sb.Append(", \"periodIsActive\": \"" + (Periods.CurrentPeriod.Status == GamePeriodStatus.Active ? "1" : "0") + "\"");
                sb.Append(", \"timeRemaining\": \"" + GameTimeConverter.ToString(Periods.CurrentPeriod.TimeRemaining) + "\"");
            }
            else
            {
                sb.Append(", \"period\": \"None\"");
                sb.Append(", \"periodIsActive\": false");
                sb.Append(", \"timeRemaining\": \"0:00\"");
            }
            if (_game.Parent != null && _game.Parent.Parent != null)
            {
                sb.Append(", \"shotClockTime\": \"" + _game.Parent.Parent.ShotTime.ToString() + "\"");
                sb.Append(", \"shotClockDisplayTime\": \"" + _game.Parent.Parent.ShotDisplayTime.ToString() + "\"");
            }
            else
            {
                sb.Append(", \"shotClockTime\": \"0\"");
                sb.Append(", \"shotClockDisplayTime\": \"0\"");
            }

            if (Result != GameResult.None)
            {
                sb.Append(", \"hasCompleted\": true");
                sb.Append(", \"result\": \"" + ResultDescription + "\"");
                sb.Append(", \"team1Points\": " + Team1Points);
                sb.Append(", \"team2Points\": " + Team2Points);
            }
            else
            {
                sb.Append(", \"hasCompleted\": false");
            }

            sb.Append('}');
            return sb.ToString();
        }
    }
}