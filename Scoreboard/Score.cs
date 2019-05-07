using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Media;
using System.Windows.Threading;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using Utilities;
using System.Windows.Input;

namespace Scoreboard
{
    public class Score: INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion
        
        private GameList _games;
        public GameList Games
        {
            get { return _games; }
        }

        private Visibility _poolIsVisible;
        public Visibility PoolIsVisible
        {
            get
            {
                return _poolIsVisible;
            }
            set
            {
                if (_poolIsVisible != value)
                {
                    _poolIsVisible = value;
                }         

                NotifyPropertyChanged("PoolIsVisible");
            }
        }

        private int _currentGameIndex = 0;
        public int CurrentGameIndex
        {
            get
            {
                return _currentGameIndex;
            }
            set
            {
                _currentGameIndex = value;
                if (_currentGameIndex >= 0 && _currentGameIndex < Games.Count)
                {
                    CurrentGame = Games[_currentGameIndex];
                }
                else if (Games.Count > 0)
                {
                    CurrentGame = Games.Last();
                }
                else
                {
                    CurrentGame = null;
                }
                NotifyPropertyChanged("CurrentGameIndex");
            }
        }

        private Game _currentGame = null;
        public Game CurrentGame
        {
            get
            {
                return _currentGame;             
            }
            set
            {
                if (_currentGame != value)
                {                    
                    _currentGame = value;              
                    NotifyPropertyChanged("CurrentGame");                    
                    foreach (Game game in Games)
                    {
                        game.NotifyPropertyChanged("IsCurrentGame");
                    }
                    SetupCurrentOrEndedGame();
                }
            }
        }

        private Game _currentOrEndedGame = null;
        public Game CurrentOrEndedGame
        {
            get
            {
                return _currentOrEndedGame;
            }
            set
            {
                if (_currentOrEndedGame != value)
                {
                    _currentOrEndedGame = value;
                    SelectedGame = CurrentOrEndedGame;
                    NotifyPropertyChanged("CurrentOrEndedGame");
                }
            }
        }

        private DateTime? _gameEndDelayTime = null;

        protected void SetupCurrentOrEndedGame()
        {
            if (_gameEndDelayTime == null || DateTime.Now >= _gameEndDelayTime.Value)
            {
                _gameEndDelayTime = null;
                CurrentOrEndedGame = CurrentGame;                
            }
        }

        private Game GetNextGame()
        {
            int nextGameIndex = CurrentGameIndex + 1;

            if (nextGameIndex < Games.Count)
            {
                return Games[nextGameIndex];
            }

            return null;
        }

        private String _gameCaption;
        public string GameCaption
        {
            get
            {
                return _gameCaption;
            }
            set
            {
                _gameCaption = value;
                NotifyPropertyChanged("GameCaption");
            }
        }                       

        private String _gameTimeColor;
        public String GameTimeColor
        {
            get
            {
                return _gameTimeColor;
            }
            set
            {
                _gameTimeColor = value;
                NotifyPropertyChanged("GameTimeColor");
            }
        }

        private bool _recordGoalScorers = false;
        public bool RecordGoalScorers
        {
            get
            {
                return _recordGoalScorers;
            }
            set
            {
                if (!_recordGoalScorers.Equals(value))
                {
                    _recordGoalScorers = value;
                    if (RecordGoalScorers != Properties.Settings.Default.RecordGoalScorers)
                    {
                        Properties.Settings.Default.RecordGoalScorers = RecordGoalScorers;
                        Properties.Settings.Default.Save();
                    }
                    NotifyPropertyChanged("RecordGoalScorers");
                }
            }
        }

        private bool _lockResults = false;
        public bool LockResults
        {
            get
            {
                return _lockResults;
            }
            set
            {
                if (!_lockResults.Equals(value))
                {
                    if (WindowsAuthentication.Authenticate("Authenticate to " + (value ? "Lock" : "Unlock") + " Results", true))
                    {
                        _lockResults = value;
                        if (LockResults != Properties.Settings.Default.LockResults)
                        {
                            Properties.Settings.Default.LockResults = LockResults;
                            Properties.Settings.Default.Save();
                        }
                        NotifyPropertyChanged("LockResults");
                    }
                }
            }
        }

        private bool _startPaused = false;
        public bool StartPaused
        {
            get
            {
                return _startPaused;
            }
            set
            {
                if (!_startPaused.Equals(value))
                {
                    _startPaused = value;
                    if (StartPaused != Properties.Settings.Default.StartPaused)
                    {
                        Properties.Settings.Default.StartPaused = StartPaused;
                        Properties.Settings.Default.Save();
                    }
                    NotifyPropertyChanged("StartPaused");
                }
            }
        }

        private bool _decrementShotTime = false;
        private int _shotTime = 0;
        public int ShotTime
        {
            get
            {
                return _shotTime;
            }
            set
            {
                int shotTime = Math.Max(0, value);
                if (_shotTime != shotTime)
                {
                    _shotTime = shotTime;
                    NotifyPropertyChanged("ShotTime");
                }
            }
        }
        
        private Visibility _shotTimeVisible = Visibility.Collapsed;
        public Visibility ShotTimeVisible
        {
            get
            {
                return _shotTimeVisible;
            }
            set
            {
                if (_shotTimeVisible != value)
                {
                    _shotTimeVisible = value;
                    NotifyPropertyChanged("ShotTimeVisible");
                }
            }
        }

        private Tourney _tourney;
        public Tourney Tourney { get { return _tourney; } }

        public void ResetShotTime()
        {
            ShotTimeVisible = Visibility.Visible;
            _decrementShotTime = false;
            ShotTime = 60;
            if (CurrentGame != null)
            {
                CurrentGame.LogEvent("Reset Shot Clock");
            }
        }

        public void ResetResumeShotTime()
        {
            ResetShotTime();
            PauseResumeShotTime();
        }

        public void StopShotTime()
        {
            _decrementShotTime = false;
            ShotTime = 0;
        }

        public void PauseResumeShotTime()
        {
            ShotTimeVisible = Visibility.Visible;
            if (CurrentGame != null && CurrentGame.Periods.CurrentPeriod != null && CurrentGame.Periods.CurrentPeriod.Status == GamePeriodStatus.Active)
            {
                _decrementShotTime = !_decrementShotTime;
                if (_decrementShotTime)
                {
                    if (ShotTime <= 0)
                    {
                        ShotTime = 60;
                    }
                    CurrentGame.LogEvent("Resume Shot Clock");
                }
                else
                {
                    CurrentGame.LogEvent("Pause Shot Clock"); 
                }
            }
        }

        public void IncrementShotTime()
        {
            if (ShotTime < 60)
            {
                ShotTime = ShotTime + 1;
            }
        }

        public void DecrementShotTime()
        {
            if (ShotTime > 0)
            {
                ShotTime = ShotTime - 1;
            }
        }

        public void ReplaceTeamNames(Dictionary<string, string> teamNames)
        {
            foreach (KeyValuePair<string, string> teamName in teamNames)
            {
                ReplaceTeamName(teamName.Key, teamName.Value);
            }
        }

        public void ReplaceTeamName(string oldName, string newName)
        {
            foreach (Game game in Games)
            {
                if (game.Team1Original.Equals(oldName))
                {
                    game.Team1 = newName;
                }
                if (game.Team2Original.Equals(oldName))
                {
                    game.Team2 = newName;
                }
            }
        }

        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get
            {
                return _currentTime;
            }
            set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    NotifyPropertyChanged("CurrentTime");
                }
            }
        }

        public int TimeToLastTick
        {
            get
            {
                if (CurrentTime == null)
                {
                    return 0;
                }
                else
                {
                    return (int)(DateTime.Now - CurrentTime).TotalMilliseconds;
                }
            }
        }

        private System.Windows.Threading.DispatcherTimer gameTimer = new System.Windows.Threading.DispatcherTimer();
        protected System.Windows.Threading.DispatcherTimer GameTimer
        {
            get { return gameTimer; }
        }

        private SoundPlayer _beep;
        private SoundPlayer _beepEnd;
        private SoundPlayer _shotClockBell;
        private SoundPlayer _shotClockEnd;

        private bool _paused = false;
        public bool Paused
        {
            get { return _paused; }
            set
            {
                if (_paused != value)
                {
                    _paused = value;
                    NotifyPropertyChanged("Paused");
                }
            }
        }        

        private BindingList<Card> _team1Cards = new BindingList<Card>();
        public BindingList<Card> Team1Cards { get { return _team1Cards; } }

        private BindingList<Card> _team2Cards = new BindingList<Card>();
        public BindingList<Card> Team2Cards { get { return _team2Cards; } }

        public static string AppDataFolder
        {            
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, @"verner software\ScoreBoard");
            } 
        }

        protected string CurrentGamesFileName
        {
            get
            {
                return System.IO.Path.Combine(AppDataFolder, "currentgames.xml");
            }
        }        

        #region Delegates

        public OnSelectOptionsDelegate OnAddGames;
        public OnSelectOptionsDelegate OnAddExtraPeriod;
        public OnSelectOptionsDelegate OnGameCompleted;

        #endregion


        public Score()
        {
            _games = new GameList();
            _serverOptions = new SimpleWebServerOptions();
            _tourney = new Tourney(this);
            Games.SetParent(this);
            LoadBeep();
            RecordGoalScorers = Properties.Settings.Default.RecordGoalScorers;
            _lockResults = Properties.Settings.Default.LockResults;
            StartPaused = Properties.Settings.Default.StartPaused;
            ServerOptions.Port = Properties.Settings.Default.ServerPort;
            ServerOptions.Active = Properties.Settings.Default.ServerActive;
            StartStopServer();
        }

        public void Initialise()
        {            
            StartTimer();
            LoadGames();
        }

        public void StartTimer()
        {
            GameTimer.Interval = new TimeSpan(0, 0, 1);
            GameTimer.Tick += new EventHandler(GameTimer_Tick);
            GameTimer.IsEnabled = true;            
        }

        protected DateTime ReadTime(string time, DateTime current)
        {
            if (time.Length > 0)
            {
                if (time[0] == '+')
                {
                    return current + ReadTimeSpan(time.Substring(1));
                }
                else
                {
                    return DateTime.Parse(time);
                }
            }
            else
            {
                throw new FormatException("The time \"" + time + "\" is not valid.");
            }
        }

        protected TimeSpan ReadTimeSpan(string time)
        {
            string[] parts = time.Split(':');
            if (parts.Length > 0)
            {
                int hours = 0;
                int minutes = 0;
                int seconds = 0;

                if (parts.Length == 1)
                {
                    minutes = int.Parse(parts[0]);
                }
                else if (parts.Length == 2)
                {
                    minutes = int.Parse(parts[0]);
                    seconds = int.Parse(parts[1]);
                }
                else // if (parts.Length == 3)
                {
                    hours = int.Parse(parts[0]);
                    minutes = int.Parse(parts[1]);
                    seconds = int.Parse(parts[2]);
                }

                return new TimeSpan(hours, minutes, seconds);
            }
            else
            {
                throw new FormatException("The time \"" + time + "\" is not valid.");
            }
        }

        private Visibility _editVisible = Visibility.Visible;
        public Visibility EditVisible
        {
            get { return _editVisible; }
            set
            {
                if (!_editVisible.Equals(value))
                {
                    _editVisible = value;
                    NotifyPropertyChanged("EditVisible");
                }
            }
        }

        public void LoadGames()
        {
            if (File.Exists(CurrentGamesFileName))
            {
                LoadGamesFromFile(CurrentGamesFileName, false, true);
            }            
        }

        public void InitialiseGames()
        {
            Games.ClearGames();
            _currentGameIndex = 0;
        }

        public void StartGames()
        {           
            PoolIsVisible = Games.HasPool ? Visibility.Visible : Visibility.Hidden;

            SelectedGame = Games.FirstOrDefault();
            SelectCurrentGame(0);
            if (CurrentGame != null)
            {
                if (CurrentGame.EndTime > DateTime.Now)
                {
                    StartGame();
                }
                CurrentOrEndedGame = CurrentGame;
            }
        }
        
        public void RestartGames()
        {
            Games.Sort();

            foreach (Game game in Games)
            {
                game.ResetGame();
            }
            StartGames();
        }

        public void LoadGamesFromFile(string fileName, bool clearStatus, bool clearExistingGames)
        {
            if (clearExistingGames)
            {
                InitialiseGames();
            }

            string xmlString = File.ReadAllText(fileName).Replace("ArrayOfGame", "GameList");

            XmlSerializer deserializer = new XmlSerializer(typeof(GameList));
            using (TextReader reader = new StringReader(xmlString))
            {
                GameList games = (GameList)deserializer.Deserialize(reader);

                if (games.Count > 0 && games[0].StartTime != null)
                {
                    DateTime startTime = games[0].StartTime.Value;
                    TimeSpan adjustment = DateTime.Today - startTime.Date;
                    foreach (Game game in games)
                    {
                        game.Periods.AdjustTimes(adjustment);
                    }
                }

                foreach (Game game in games)
                {
                    if (clearStatus)
                    {
                        game.ClearStatus();
                    }
                    game.Loaded();
                    Games.Add(game);
                }

                Games.Assign(games);

                // Send games to Tourney if needed.
                foreach (Game game in Games)
                {
                    if (game.NeedsSending)
                    {
                        Tourney.ApplyGame(game, false);
                    }
                }
                Tourney.ProcessQueue();                
            }

            StartGames();
        }

        public void SaveGames()
        {
            SaveGamesToFile(CurrentGamesFileName);
        }

        public void SaveGamesToFile(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(path);

            XmlSerializer serializer = new XmlSerializer(typeof(GameList));
            TextWriter textWriter = new StreamWriter(fileName);
            serializer.Serialize(textWriter, Games);
            textWriter.Close();
        }

        #region Commands

        RelayCommand _addGamesCommand;
        public ICommand AddGamesCommand
        {
            get
            {
                if (_addGamesCommand == null)
                {
                    _addGamesCommand = new RelayCommand(param => this.AddNewGames(), null);
                }
                return _addGamesCommand;
            }
        }

        RelayCommand _removeAllGamesCommand;
        public ICommand RemoveAllGamesCommand
        {
            get
            {
                if (_removeAllGamesCommand == null)
                {
                    _removeAllGamesCommand = new RelayCommand(param => this.RemoveAllGames(), null);
                }
                return _removeAllGamesCommand;
            }
        }

        RelayCommand _enableDisableEditCommand;
        public ICommand EnableDisableEditCommand
        {
            get
            {
                if (_enableDisableEditCommand == null)
                {
                    _enableDisableEditCommand = new RelayCommand(param => this.EnableDisableEdit(), null);
                }
                return _enableDisableEditCommand;
            }
        }

        RelayCommand _addPeriodCommand;
        public ICommand AddPeriodCommand
        {
            get
            {
                if (_addPeriodCommand == null)
                {
                    _addPeriodCommand = new RelayCommand(param => this.AddPeriod((Game)param), null);
                }
                return _addPeriodCommand;
            }
        }

        #endregion

        public void AddNewGames()
        {
            if (OnAddGames != null)
            {
                GameList newGames = new GameList();
                if (OnAddGames(this, newGames))
                {
                    AddGames(newGames);
                }
            }
        }

        public void AddGames(GameList newGames)
        {
            foreach (Game newGame in newGames)
            {
                Games.Add(newGame);
            }
               
            PoolIsVisible = Games.HasPool ? Visibility.Visible : Visibility.Hidden;
            SaveGames();
            RestartGames();
        }

        public bool AddPeriod(Game game)
        {
            if (game != null && OnAddExtraPeriod != null)
            {            
                if (OnAddExtraPeriod(game, null))
                {
                    game.CalculateResult();
                    if (CurrentGame != game)
                    {
                        CurrentGame = game;
                    }
                    game.Periods.CurrentIndex = game.Periods.Count - 1;
                    Pause();
                    SelectedPeriod = CurrentGame.Periods.CurrentPeriod;
                    EditVisible = Visibility.Visible;
                    return true;
                }
                else
                {
                    return false;
                }                    
            }
            return false;
        }

        public void ShowGameCompleted(Game currentGame, Game nextGame)
        {
            if (currentGame != null && OnGameCompleted != null)
            {
                if (currentGame.HasStarted && !currentGame.HasCompleted)
                {
                    currentGame.HasCompleted = true;
                    OnGameCompleted(currentGame, nextGame);
                }
            }
        }

        public void AddExtraPeriod(Game game, int durationSeconds)
        {
            game.HasCompleted = false; // Just in case we need another extra period.

            CurrentGame = game;

            DateTime startTime = DateTime.Now;
            TimeSpan extraPeriodDuration = TimeSpan.FromSeconds(durationSeconds);

            GamePeriod gamePeriod = new GamePeriod();
            gamePeriod.EndTime = startTime + extraPeriodDuration;
            gamePeriod.IsExtraPeriod = true;
            gamePeriod.Name = "Extra Period";
            gamePeriod.StartTime = startTime;
            game.Periods.Add(gamePeriod);

            gamePeriod.ModifyFollowingTimes(gamePeriod.EndTime - DateTime.Now, false);

            game.CalculateResult();
            if (CurrentGame != game)
            {
                CurrentGame = game;
            }
            game.Periods.CurrentIndex = game.Periods.Count - 1;
            Pause();
            SelectedPeriod = CurrentGame.Periods.CurrentPeriod;
            EditVisible = Visibility.Visible;            
        }

        public void StartGameInSeconds(Game game, int startInSeconds)
        {            
            CurrentGame = game;
            CurrentGame.StartFirstPeriodInSeconds(startInSeconds);
        }

        public void RemoveGame(Game game)
        {
            // Todo - Do this in View            
            if (MessageBox.Show("Are you sure you want to remove this game?", "Remove Game?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            int selectedIndex = Math.Max(0, Games.IndexOf(game) - 1);

            if (CurrentGame == game)
            {                
                NextGame(false);
            }
            Games.Remove(game);

            // Todo - Display selected game
            if (selectedIndex < Games.Count)
            {
                SelectedGame = Games[selectedIndex];
            }
            else
            {
                SelectedGame = Games.FirstOrDefault();
            }

            SaveGames();
        }

        public void RemoveAllGames()
        {
            // Todo - Do this in View            
            if (MessageBox.Show("Are you sure you want to remove All Games?", "Remove All Games?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            Games.ClearGames();
            StartGames();

            SaveGames();
        }

        private Game _selectedGame;
        public Game SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                if (_selectedGame != value)
                {
                    _selectedGame = value;                    
                    NotifyPropertyChanged("SelectedGame");
                    if (SelectedGame != null)
                    {
                        SelectedPeriod = SelectedGame.Periods.CurrentPeriod == null ? SelectedGame.Periods.FirstPeriod : SelectedGame.Periods.CurrentPeriod;
                    }
                }
            }
        }

        private GamePeriod _selectedPeriod;
        public GamePeriod SelectedPeriod
        {
            get { return _selectedPeriod; }
            set
            {
                if (_selectedPeriod != value)
                {
                    _selectedPeriod = value;
                    NotifyPropertyChanged("SelectedPeriod");
                }
            }
        }

        public void EnableDisableEdit()
        {
            if (EditVisible == Visibility.Hidden || EditVisible == Visibility.Collapsed)
            {
                EditVisible = Visibility.Visible;
            }
            else
            {
                EditVisible = Visibility.Collapsed;
            }
        }

        protected void StartGame()
        {
            Team1Cards.Clear();
            Team2Cards.Clear();

            if (CurrentGame != null)
            {                
                CurrentGame.Periods.SetFirstPeriod();
            }
            
            Paused = false;        
         
            UpdateDisplay();            
        }

        protected void NextPosition()
        {
            StopShotTime();

            if (CurrentGame != null)
            {
                CurrentGame.Periods.NextPeriod();
                SelectedPeriod = CurrentGame.Periods.CurrentPeriod;
                // Do not log the start of this period now, as there is an interval delay.
            }            
          
            UpdateDisplay(); 
        }

        public void NextGame(bool delayCurrentEnd)
        {
            DateTime now = DateTime.Now;

            StopShotTime();

            Game currentGame = CurrentGame;            

            int currentGameIndex = Games.IndexOf(CurrentGame);
            int nextGameIndex = currentGameIndex + 1;
            Game nextGame = nextGameIndex < Games.Count ? Games[nextGameIndex] : null;

            if (delayCurrentEnd)
            {
                TimeSpan nextGameDelay = new TimeSpan(0, 0, 30);

                if (nextGame != null && nextGame.StartTime != null)
                {
                    TimeSpan nextGameStartDuration = nextGame.StartTime.Value - now;
                    if (nextGameStartDuration.TotalSeconds < 60)
                    {
                        nextGameDelay = new TimeSpan(0, 0, (int)Math.Round(nextGameStartDuration.TotalSeconds / 3));
                    }
                }
                _gameEndDelayTime = now + nextGameDelay;
            }
            else
            {
                _gameEndDelayTime = null;
            }

            SelectCurrentGame(nextGameIndex);
            if (CurrentGame != null && !CurrentGame.HasEnded)
            {
                StartGame();
            }
            UpdateDisplay();

            if (currentGame != null)
            {
                Tourney.ApplyGame(currentGame);
            }
        }

        protected void SelectCurrentGame(int startIndex)
        {
            DateTime now = DateTime.Now;

            int gameIndex = startIndex;
            while (gameIndex < Games.Count)
            {
                Game game = Games[gameIndex];
                if (now < game.Periods.EndTime)
                {                    
                    break;
                }
                gameIndex++;                
            }
            CurrentGameIndex = gameIndex;
        }
     
        protected void UpdateTime()
        {
            DateTime now = DateTime.Now;            
            CurrentTime = now;

            SetupCurrentOrEndedGame();

            if (!Paused)
            {                                
                if (CurrentGame != null && CurrentGame.Periods.CurrentPeriod != null)
                {
                    GamePeriodStatus status = CurrentGame.Periods.CurrentPeriod.Status;
                    CurrentGame.Periods.CurrentPeriod.UpdatePeriod();

                    TimeSpan remaining = CurrentGame.Periods.CurrentPeriod.TimeRemaining;

                    int oldShotTime = ShotTime;
                    if (_decrementShotTime)
                    {
                        ShotTime--;
                    }

                    if (CurrentGame.Periods.CurrentPeriod != null && CurrentGame.Periods.CurrentPeriod.Status == GamePeriodStatus.Active)
                    {
                        for (int i = Team1Cards.Count - 1; i >= 0; i--)
                        {
                            Card card = Team1Cards[i];
                            if (card.Time > 0)
                            {
                                card.Time--;
                            }
                            else 
                            {
                                Team1Cards.Remove(card);
                            }
                        }

                        for (int i = Team2Cards.Count - 1; i >= 0; i--)
                        {
                            Card card = Team2Cards[i];
                            if (card.Time > 0)
                            {
                                card.Time--;
                            }
                            else
                            {
                                Team2Cards.Remove(card);
                            }
                        }
                    }

                    DateTime currentTime = DateTime.Now;

                    if (remaining.TotalMilliseconds <= 0)
                    {
                        if (CurrentGame.HasEnded)
                        {
                            CurrentGame.CalculateResult();
                            ShowGameCompleted(CurrentGame, GetNextGame());
                            NextGame(true);
                        }
                        else
                        {
                            NextPosition();
                        }
                    }
                    else
                    {
                        if (remaining.Minutes == 0 && remaining.Seconds >= 0 && remaining.Seconds <= 4)
                        {
                            Beep(remaining.Seconds == 0);
                        }
                        else if (oldShotTime != ShotTime)
                        {
                            if (oldShotTime == 21 && ShotTime == 20)
                            {
                                _shotClockBell.Play();
                                _shotClockBell.Play();
                            }
                            else if (oldShotTime == 1 && ShotTime == 0)
                            {
                                _decrementShotTime = false;
                                _shotClockEnd.Play();
                                if (CurrentGame != null)
                                {
                                    CurrentGame.LogEvent("Shot Elapsed");
                                }
                            }
                        }
                    }

                    GameCompletedWindow.StaticTick();
                }
            }
            
            // Try to send the queue every 20 seconds.
            if (CurrentTime.Second % 20 == 0)
            {
                Tourney.ProcessQueue();
            }
        }

        protected void LoadBeep()
        {
            _beep = new SoundPlayer(@"sounds\beep.wav");
            _beep.LoadAsync();
            _beepEnd = new SoundPlayer(@"sounds\beep_end.wav");
            _beepEnd.LoadAsync();
            _shotClockBell = new SoundPlayer(@"sounds\shot_clock_bell.wav");
            _shotClockBell.LoadAsync();
            _shotClockEnd = new SoundPlayer(@"sounds\shot_clock_end.wav");
            _shotClockEnd.LoadAsync();
        }

        protected void Beep(bool longBeep)
        {
            if (!Paused && CurrentGame != null && !CurrentGame.HasEnded)
            {
                if (longBeep)
                {                    
                    _beepEnd.Play();
                }
                else
                {                                        
                    _beep.Play();
                }
            }
        }

        protected void UpdateDisplay()
        {
            GameCaption = GetGameCaption();
        }

        protected string GetGameCaption()
        {
            // Gets the caption which usually says what the next games is.
            int gameIndex = CurrentGameIndex;
            // If the current game is in progress then the caption should be about the next game.
            if (CurrentGame != null && CurrentGame.Periods.CurrentPeriod != null
                    && (CurrentGame.Periods.CurrentPeriod != CurrentGame.Periods.First() || CurrentGame.Periods.CurrentPeriod.Status != GamePeriodStatus.Pending))
            {
                gameIndex++;
            }
            if (gameIndex < Games.Count)
            {
                Game game = Games[gameIndex];
                return "Next: " + game.Team1 + " vs " + game.Team2 + " at " + game.Periods.StartTime.ToString("hh:mm");
            }
            else
            {
                return "";
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {            
            UpdateTime();
        }

        public void Team1NoGoal()
        {            
            if (CurrentGame != null)
            {
                CurrentOrEndedGame.Team1Score--;
                CurrentOrEndedGame.LogEvent("No Goal", CurrentOrEndedGame.Team1, null, CurrentOrEndedGame.Team1Score.ToString() + " to " + CurrentOrEndedGame.Team2Score.ToString());
                SaveGames();
            }
        }

        public void Team1Goal(string player)
        {
            if (CurrentOrEndedGame != null)
            {
                CurrentOrEndedGame.Team1Score++;                
                CurrentOrEndedGame.LogEvent("Goal", CurrentOrEndedGame.Team1, player, CurrentOrEndedGame.Team1Score.ToString() + " to " + CurrentOrEndedGame.Team2Score.ToString());                
                SaveGames();
            }
        }

        public void SelectCard(Window owner, string team)
        {
            string card;
            string player;
            string infringement;
            string penaltyDuration;
            string selectedTeam;
            if (Cards.SelectCard(owner, CurrentGame, team, out card, out player, out infringement, out penaltyDuration, out selectedTeam) && CurrentOrEndedGame != null) 
            {
                GameEvent gameEvent = CurrentOrEndedGame.LogEvent(card + " Card", selectedTeam, player, infringement);
                if (card.Equals("Yellow"))
                {
                    int penaltyDurationSeconds = 120;
                    TimeSpan penaltyDurationTimeSpan = ParseTimeSpan(penaltyDuration);
                    if (penaltyDurationTimeSpan.TotalSeconds > 0)
                    {
                        penaltyDurationSeconds = (int)penaltyDurationTimeSpan.TotalSeconds;
                    }

                    if (selectedTeam == CurrentOrEndedGame.Team1)
                    {
                        Team1Cards.Add(new Card(penaltyDurationSeconds, gameEvent));
                    }
                    else
                    {
                        Team2Cards.Add(new Card(penaltyDurationSeconds, gameEvent));
                    }
                }
            }
        }

        public static DateTime ParseTime(string time)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (!String.IsNullOrEmpty(time))
            {
                string[] parts = time.Split(':');

                if (parts.Length > 0)
                {
                    hours = int.Parse(parts[0]);
                }

                if (parts.Length > 1)
                {
                    minutes = int.Parse(parts[1]);
                }

                if (parts.Length > 2)
                {
                    seconds = int.Parse(parts[2]);
                }
            }

            return DateTime.Today + new TimeSpan(hours, minutes, seconds);
        }

        public static TimeSpan ParseTimeSpan(string timeSpan)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (!String.IsNullOrEmpty(timeSpan))
            {
                string[] parts = timeSpan.Split(':');

                if (parts.Length == 1)
                {
                    minutes = int.Parse(parts[0]);
                }
                else if (parts.Length == 2)
                {
                    minutes = int.Parse(parts[0]);
                    seconds = int.Parse(parts[1]);
                }
                else if (parts.Length == 3)
                {
                    hours = int.Parse(parts[0]);
                    minutes = int.Parse(parts[1]);
                    seconds = int.Parse(parts[2]);
                }
            }

            return new TimeSpan(hours, minutes, seconds);
        }

        public void Pause()
        {
            if (!Paused)
            {
                if (CurrentGame != null) CurrentGame.LogEvent("Paused");
                Paused = true;
                UpdateDisplay();
            }
        }

        public void Resume()
        {
            if (Paused)
            {
                if (CurrentGame != null)
                {                                        
                    CurrentGame.LogEvent("Resumed");
                    GamePeriod period = CurrentGame.Periods.CurrentPeriod;
                    if (period != null)
                    {
                        DateTime targetTime = DateTime.Now + period.TimeRemaining;

                        if (period.Status == GamePeriodStatus.Pending)
                        {
                            period.ModifyStartTime(targetTime - period.StartTime, false);
                        }
                        else if (period.Status == GamePeriodStatus.Active)
                        {
                            period.ModifyEndTime(targetTime - period.EndTime, false);
                        }
                    }                    
                }
                Paused = false;                                
                
                UpdateTime();
                UpdateDisplay();
            }
        }

        public void PauseResume()
        {
            if (!Paused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        public void Team2NoGoal()
        {
            if (CurrentOrEndedGame != null)
            {
                CurrentOrEndedGame.Team2Score--;
                CurrentOrEndedGame.LogEvent("No Goal", CurrentOrEndedGame.Team2, null, CurrentOrEndedGame.Team1Score.ToString() + " to " + CurrentOrEndedGame.Team2Score.ToString());
                SaveGames();
            }
        }

        public void Team2Goal(string player)
        {
            if (CurrentOrEndedGame != null)
            {
                CurrentOrEndedGame.Team2Score++;                
                CurrentOrEndedGame.LogEvent("Goal", CurrentOrEndedGame.Team2, player, CurrentOrEndedGame.Team1Score.ToString() + " to " + CurrentOrEndedGame.Team2Score.ToString());
                SaveGames();
            }
        }

        public static string VersesTeamName(string team1, string team2)
        {
            return team1 + " - " + team2;
        }
        
        public string CalculateTopGoalScorers()
        {                                    
            Dictionary<GamePlayer, int> players = new Dictionary<GamePlayer, int>();
            Dictionary<GameTeam, int> teams = new Dictionary<GameTeam, int>();

            foreach (Game game in Games)
            {
                GamePlayer lastPlayer = null;
                GameTeam lastTeam = null;
                foreach (GameEvent gameEvent in game.GameEvents)
                {
                    if (gameEvent.EventType == "Goal")
                    {
                        if (!String.IsNullOrWhiteSpace(gameEvent.Player))
                        {
                            GamePlayer player = GamePlayer.AddOrGetGamePlayer(players, null /* game.Pool */, gameEvent.Team, gameEvent.Player);
                            GamePlayer.AddPoints(players, player, 1);
                            lastPlayer = player;
                        }
                        GameTeam team = GameTeam.AddOrGetGameTeam(teams, null /* game.Pool */, gameEvent.Team);
                        GameTeam.AddPoints(teams, team, 1);
                        lastTeam = team;
                    }
                    else if (gameEvent.EventType == "No Goal")
                    {
                        if (lastPlayer != null)
                        {
                            GamePlayer.AddPoints(players, lastPlayer, -1);
                        }

                        if (lastTeam != null)
                        {
                            GameTeam.AddPoints(teams, lastTeam, -1);
                        }
                    }
                }                
            }

            var sortedPlayers = from player in players
                    orderby player.Value descending
                        , player.Key.Pool ascending
                        , player.Key.Team ascending
                        , player.Key.Player ascending
                    select player;

            var sortedTeams = from team in teams
                                orderby team.Value descending                                    
                                    , team.Key.Team ascending                                    
                                select team;

            StringBuilder statistics = new StringBuilder();
            statistics.AppendLine("Top Goal Scorers");
            statistics.AppendLine();

            if (sortedPlayers.Count() == 0)
            {
                statistics.AppendLine("No goals have been recorded against players.");
            }
            else
            {
                statistics.AppendLine("Top Players");
                statistics.AppendLine();
                statistics.Append("Team");
                statistics.Append("\tPlayer");
                statistics.Append("\tGoals");
                statistics.AppendLine();

                foreach (var player in sortedPlayers)
                { 
                    // Display Totals
                    statistics.Append(player.Key.Team);
                    statistics.Append("\t" + player.Key.Player);
                    statistics.Append("\t" + player.Value);
                    statistics.AppendLine();
                }

                statistics.AppendLine();
                statistics.AppendLine("Top Teams");
                statistics.AppendLine();
                statistics.Append("Team");                
                statistics.Append("\tGoals");
                statistics.AppendLine();

                foreach (var team in sortedTeams)
                {
                    // Display Totals
                    statistics.Append(team.Key.Team);                    
                    statistics.Append("\t" + team.Value);
                    statistics.AppendLine();
                }
            }

            return statistics.ToString();
        }

        public string CalculateStatistics(int winPoints, int drawPoints, int lossPoints)
        {
            Dictionary<GameTeam, int> played = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> points = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> wins = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> losses = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> draws = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> goalsFor = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> goalsAgainst = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> versesPoints = new Dictionary<GameTeam, int>();
            Dictionary<GameTeam, int> versesGoalDifference = new Dictionary<GameTeam, int>();

            foreach (Game game in Games)
            {
                if (game.Result != GameResult.None)
                {
                    GameTeam team1 = GameTeam.AddOrGetGameTeam(played, game.Pool, game.Team1);
                    GameTeam team2 = GameTeam.AddOrGetGameTeam(played, game.Pool, game.Team2);
                    GameTeam versesTeam1 = GameTeam.AddOrGetGameTeam(played, game.Pool, VersesTeamName(game.Team1, game.Team2));
                    GameTeam versesTeam2 = GameTeam.AddOrGetGameTeam(played, game.Pool, VersesTeamName(game.Team2, game.Team1));

                    // The teams have played games
                    GameTeam.AddPoints(played, team1, 1);
                    GameTeam.AddPoints(played, team2, 1);

                    // Record points for and against
                    GameTeam.AddPoints(goalsFor, team1, game.Team1Score);
                    GameTeam.AddPoints(goalsAgainst, team1, game.Team2Score);
                    GameTeam.AddPoints(versesGoalDifference, versesTeam1, game.Team1Score - game.Team2Score);

                    GameTeam.AddPoints(goalsFor, team2, game.Team2Score);
                    GameTeam.AddPoints(goalsAgainst, team2, game.Team1Score);
                    GameTeam.AddPoints(versesGoalDifference, versesTeam2, game.Team2Score - game.Team1Score);

                    // Record game points
                    if (game.Result == GameResult.Team1)
                    {
                        GameTeam.AddPoints(points, team1, winPoints);
                        GameTeam.AddPoints(versesPoints, versesTeam1, winPoints);
                        GameTeam.AddPoints(wins, team1, 1);

                        GameTeam.AddPoints(points, team2, lossPoints);
                        GameTeam.AddPoints(versesPoints, versesTeam2, lossPoints);
                        GameTeam.AddPoints(losses, team2, 1);
                    }
                    else if (game.Result == GameResult.Team2)
                    {
                        GameTeam.AddPoints(points, team1, lossPoints);
                        GameTeam.AddPoints(versesPoints, versesTeam1, lossPoints);
                        GameTeam.AddPoints(losses, team1, 1);

                        GameTeam.AddPoints(points, team2, winPoints);
                        GameTeam.AddPoints(versesPoints, versesTeam2, winPoints);
                        GameTeam.AddPoints(wins, team2, 1);
                    }
                    else
                    {
                        GameTeam.AddPoints(points, team1, drawPoints);
                        GameTeam.AddPoints(versesPoints, versesTeam1, drawPoints);
                        GameTeam.AddPoints(draws, team1, 1);

                        GameTeam.AddPoints(points, team2, drawPoints);
                        GameTeam.AddPoints(versesPoints, versesTeam2, drawPoints);
                        GameTeam.AddPoints(draws, team2, 1);
                    }

                    // Ensure that team1 is present in every dictionary.                                        
                    GameTeam.AddPoints(wins, team1, 0);
                    GameTeam.AddPoints(losses, team1, 0);
                    GameTeam.AddPoints(draws, team1, 0);

                    // Ensure that team2 is present in every dictionary.                                        
                    GameTeam.AddPoints(wins, team2, 0);
                    GameTeam.AddPoints(losses, team2, 0);
                    GameTeam.AddPoints(draws, team2, 0);
                }
            }

            // Get points list sorted by pool and points.
            var sortedPoints = from teamPoints in points
                               orderby teamPoints.Key.Pool ascending
                                    , teamPoints.Value descending
                                    , goalsFor[teamPoints.Key] - goalsAgainst[teamPoints.Key] descending
                                    , goalsFor[teamPoints.Key] descending
                               select teamPoints;

            StringBuilder statistics = new StringBuilder();
            if (sortedPoints.Count() == 0)
            {
                statistics.AppendLine("No games have completed.");
            }
            else
            {
                string pool = string.Empty;
                foreach (var teamPoints in sortedPoints)
                {
                    if (!String.Equals(teamPoints.Key.Pool, pool))
                    {
                        pool = teamPoints.Key.Pool;
                        if (statistics.Length > 0)
                        {
                            statistics.AppendLine("");
                        }
                        statistics.AppendLine("<b>Pool: " + pool);
                        statistics.AppendLine();
                        // Display headings
                        statistics.Append("<b>Team\tP\tGD\tGF\tPL");
                        // Display Verses Team Headings
                        foreach (var versesTeam in sortedPoints)
                        {
                            if (String.Equals(versesTeam.Key.Pool, pool))
                            {
                                statistics.Append("\t" + versesTeam.Key.Team);
                            }
                        }
                        statistics.AppendLine();
                    }
                    // Display Totals
                    statistics.Append(teamPoints.Key.Team);
                    statistics.Append("\t" + teamPoints.Value); // P: Points
                    statistics.Append("\t" + (goalsFor[teamPoints.Key] - goalsAgainst[teamPoints.Key])); // GD: Goal Difference
                    statistics.Append("\t" + goalsFor[teamPoints.Key]); // GF: Goals For
                    //statistics.Append(" GA " + goalsAgainst[teamPoints.Key]); // GA: Goals Against
                    statistics.Append("\t" + played[teamPoints.Key]); // PL: Played
                                                                      //statistics.Append(" W " + wins[teamPoints.Key]); // W: Wins
                                                                      //statistics.Append(" L " + losses[teamPoints.Key]); // L: Losses
                                                                      //statistics.Append(" D " + draws[teamPoints.Key]); // D: Draws

                    // Display Verses Team Statistics
                    foreach (var versesTeam in sortedPoints)
                    {
                        if (String.Equals(versesTeam.Key.Pool, pool))
                        {
                            string versesTeamName = VersesTeamName(teamPoints.Key.Team, versesTeam.Key.Team);
                            int? versesPointsValue = GameTeam.FindPoints(versesPoints, pool, versesTeamName);
                            int? versesGoalDifferencValue = GameTeam.FindPoints(versesGoalDifference, pool, versesTeamName);
                            GameTeam versesKey = GameTeam.FindGameTeam(versesPoints, pool, versesTeamName);

                            statistics.Append("\t");
                            if (versesPointsValue.HasValue)
                            {
                                statistics.Append(versesPointsValue.Value + " / " + versesGoalDifferencValue.Value);
                            }
                        }
                    }
                    statistics.AppendLine();
                }
                // Display the Key
                statistics.AppendLine();
                statistics.AppendLine("<b>Key");
                statistics.AppendLine("P: Competition Points");
                statistics.AppendLine("GD: Goal Difference");
                statistics.AppendLine("GF: Goals For");
                statistics.AppendLine("PL: Games Played");
                statistics.Append("Verses Team: Points / Goal Difference");
            }

            return statistics.ToString();
        }

        public string ResultsText()
        {
            StringBuilder statistics = new StringBuilder();

            statistics.AppendLine("Time\tTeam 1\tScore\tTeam 2\tScore\tGroup");

            foreach (Game game in Games)
            {
                statistics.Append(game.StartTime.HasValue ? game.StartTime.Value.ToString("HH:mm") : "--:--");
                statistics.Append("\t");
                statistics.Append(game.Team1);
                statistics.Append("\t");
                statistics.Append(game.Team1Score);
                statistics.Append("\t");
                statistics.Append(game.Team2);
                statistics.Append("\t");
                statistics.Append(game.Team2Score);
                statistics.Append("\t");
                statistics.Append(game.Pool);
                statistics.AppendLine();
            }

            return statistics.ToString();
        }

        public string LogText()
        {
            StringBuilder statistics = new StringBuilder();

            statistics.AppendLine("Time\tEvent\tTeam\tPlayer\tNotes");
            foreach (Game game in Games)
            {
                statistics.Append(game.StartTime.HasValue ? game.StartTime.Value.ToString("HH:mm") : "--:--");
                statistics.Append("\t");
                statistics.Append("Start Game");
                statistics.Append("\t");
                statistics.Append("\t");
                statistics.Append("\t");
                statistics.Append(game.Team1 + " vs " + game.Team2 + (!String.IsNullOrEmpty(game.Pool) ? " (" + game.Pool + ")" : String.Empty));                
                statistics.AppendLine();   
                foreach (GameEvent gameEvent in game.GameEvents)
                {
                    statistics.AppendLine(gameEvent.ToString());
                }
            }

            return statistics.ToString();
        }

        private SimpleWebServerOptions _serverOptions;
        public SimpleWebServerOptions ServerOptions
        {
            get
            {
                return _serverOptions;
            }
        }

        private ScoreboardServer _server;

        public void StartStopServer()
        {
            if (ServerOptions.Active && _server == null)
            {
                InitialiseServer();
            }
            else if (_server != null)
            {
                StopServer();
            }
        }

        public void InitialiseServer()
        {
            try
            {
                ServerOptions.StartButtonText = "Starting";
                _server = new ScoreboardServer(this);
                ServerOptions.StartButtonText = "Stop";                
            }
            catch
            {
                StopServer();
            }
        }

        public void StopServer()
        {
            if (_server != null)
            {
                try
                {
                    _server.Stop();
                }
                catch
                {
                    // Ignore errors while stoping
                }
                _server = null;
            };
            ServerOptions.StartButtonText = "Start";
        }

		public bool SimplifySecondaryDisplay
		{
			get
			{
				return Properties.Settings.Default.SimplifySecondaryDisplay;
			}
		}        
    }
}