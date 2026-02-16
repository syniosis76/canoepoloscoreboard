using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Utilities;
using System.Reflection;
using System.Diagnostics;
using System.Drawing.Text;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {                
        private Score _score;
        public Score Score
        {
            get { return _score; }
            set
            {
                _score = value;
                Score.OnAddGames = OnAddGames;
                Score.OnAddExtraPeriod = OnAddExtraPeriod;
                Score.OnGameCompleted = OnGameCompleted;
            }            
        }
        
        private Secondary _secondary;
        public Secondary Secondary
        {
            get { return _secondary; }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = "Tourney Scoreboard " + version.ToString();
            CheckForExistingProcess();          
        }

        public void CheckForExistingProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            Process[] scoreboardProcesses = Process.GetProcessesByName("scoreboard");

            foreach (Process process in scoreboardProcesses)
            {
                if (process.Id != currentProcess.Id)
                {
                    MessageBoxResult result = MessageBox.Show("Tourney Scoreboard is already running.\n\nAre you sure you want to run another copy.", "Already Running", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        System.Windows.Application.Current.Shutdown();                        
                    }
                    break;
                }
            }
        }

        protected void CreateSecondary(int screenIndex)
        {
            if (_secondary != null)
            {
                try
                {
                    _secondary.Close();                    
                }
                catch
                {
                    // Don't care.
                }
                _secondary = null;
            }

            _secondary = new Secondary
            {
                DataContext = Score,
                SecondaryKeyUp = MainKeyUp
            };

            double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
            double virtualScreenWidth = SystemParameters.VirtualScreenWidth;

            if (virtualScreenWidth > primaryScreenWidth)
            {
                double left = screenIndex == 0 ? 0 : primaryScreenWidth + 1;
                double top = 0;
                
                Secondary.WindowStyle = WindowStyle.None;
                Secondary.ResizeMode = ResizeMode.NoResize;

                Secondary.Show();

                Secondary.Left = left;
                Secondary.Top = top;

                Secondary.WindowState = WindowState.Maximized;
            }
            else
            {
                Secondary.WindowStyle = WindowStyle.ThreeDBorderWindow;
                Secondary.ResizeMode = ResizeMode.CanResize;
                Secondary.WindowState = WindowState.Normal;
                Secondary.Show();
            }
        }

        private void Team1ScoreSubtract_Click(object sender, RoutedEventArgs e)
        {
            Score.Team1NoGoal();            
        }

        private void Team1Goal()
        {
            if (Score.CurrentGame != null)
            {
                TeamGoal(Score.CurrentGame.Team1);
            }
        }

        private void TeamGoal(string team)
        {
            if (Score.CurrentGame != null)
            {
                string player = String.Empty;

                if (Score.RecordGoalScorers)
                {
                    if (!Players.SelectPlayer(this, Score.CurrentGame, ref team, ref player))
                    {
                        return;
                    }
                }

                if (team == Score.CurrentGame.Team1)
                { 
                    Score.Team1Goal(player);
                }
                else
                {
                    Score.Team2Goal(player);
                }
                ScrollEventsToEnd();
            }
        }

        private void Team1ScoreAdd_Click(object sender, RoutedEventArgs e)
        {
            Team1Goal();            
        }
        
        private void Team2ScoreSubtract_Click(object sender, RoutedEventArgs e)
        {
            Score.Team2NoGoal();            
        }

        private void Team2Goal()
        {
            if (Score.CurrentGame != null)
            {
                TeamGoal(Score.CurrentGame.Team2);
            }
        }

        private void Team2ScoreAdd_Click(object sender, RoutedEventArgs e)
        {
            Team2Goal();
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            MainKeyUp(sender, e); 
        }

        public void MainKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                Team1Goal();
                e.Handled = true;
            }
            else if (e.Key == Key.Z)
            {
                Score.Team1NoGoal();
                e.Handled = true;
            }
            else if (e.Key == Key.S)
            {
                Team1Card_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.K)
            {
                Team2Goal();
                e.Handled = true;
            }
            else if (e.Key == Key.M)
            {
                Score.Team2NoGoal();
                e.Handled = true;
            }
            else if (e.Key == Key.J)
            {
                Team2Card_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.P)
            {
                PauseResume();
                e.Handled = true;
            }
            else if (e.Key == Key.E)
            {
                Score.ResetShotTime();
                e.Handled = true;
            }
            else if (e.Key == Key.R)
            {
                Score.ResetResumeShotTime();
                e.Handled = true;
            }
            else if (e.Key == Key.T)
            {
                Score.PauseResumeShotTime();
                e.Handled = true;
            }
            else if (e.Key == Key.U)
            {
                Score.AddNewGames();
                e.Handled = true;
            }
            else if (e.Key == Key.I)
            {
                EditGame();
                e.Handled = true;
            }
            else if (e.Key == Key.O)
            {
                ShowScoreboardMenu();
                e.Handled = true;
            }
            else if (e.Key == Key.Y)
            {
                ShowTourneyMenu();
                e.Handled = true;
            }
        }

        private void Team1Card_Click(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentGame != null)
            {
                Score.SelectCard(this, Score.CurrentGame.Team1);
                ScrollEventsToEnd();
            }
        }

        private void Team2Card_Click(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentGame != null)
            {
                Score.SelectCard(this, Score.CurrentGame.Team2);
                ScrollEventsToEnd();
            }
        }

        private void Main_Closed(object sender, EventArgs e)
        {
            if (Score != null)
            {
                Score.SaveGames();
            }
            if (Secondary != null)
            {
                Secondary.Close();
            }
        }

        private void PauseResume()
        {
            Score.PauseResume();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PauseResume();
        }        

        private void SendLog_Click(object sender, RoutedEventArgs e)
        {
            /*TextDialog dlg = new TextDialog("Email Address", "Email Address");
            dlg.Text = Properties.Settings.Default.EmailAddress;

            Action sendEmailDelegate = delegate
            {
                try
                {
                    string emailAddress = dlg.Text;
                    Properties.Settings.Default.EmailAddress = emailAddress;
                    Properties.Settings.Default.Save();

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("canoepoloscoreboard@gmail.com");
                    mail.To.Add(emailAddress);
                    mail.Subject = "Canoe Polo Log";
                    mail.IsBodyHtml = false;
                    mail.Body = Score.Games.ToString();

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                    smtp.EnableSsl = true;
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("canoepoloscoreboard@gmail.com", "sc0r3b0ard");
                    smtp.Send(mail);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Unable to send email.\n\n" + exception.Message);
                }
            };

            dlg.ActionDelegate = sendEmailDelegate;
            dlg.ActionMessage = "Sending Email...";

            dlg.ShowDialog();
            */
        }

        private void SecondaryScreen1_Click(object sender, RoutedEventArgs e)
        {
            CreateSecondary(0);
        }

        private void SecondaryScreen2_Click(object sender, RoutedEventArgs e)
        {
            CreateSecondary(1);
        }

        private void PauseResumeShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.PauseResumeShotTime();
        }

        private void ResetShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.ResetShotTime();
        }

        private void ResetResumeShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.ResetResumeShotTime();
        }

        private void IncrementShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.IncrementShotTime();
        }

        private void DecrementShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.DecrementShotTime();
        }        

        private bool OnAddGames(object input, object output)
        {
            return AddGamesWindow.AddGames(this, (Score)input, (GameList)output);
        }

        private bool OnAddExtraPeriod(object input, object output)
        {
            return AddPeriodWindow.AddPeriod(this, (Game)input);
        }

        private bool OnGameCompleted(object input, object output)
        {
            GameCompletedWindow.ShowGameCompletedWindow(this, (Game)input, (Game)output);
            Score.Tourney.UpdateGameDetails();
            return true;
        }

        private void SaveGamesClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "Scoreboard games (.xml)|*.xml"
            };

            if (saveDialog.ShowDialog() == true)
            {
                Score.SaveGamesToFile(saveDialog.FileName);
            }

        }

        protected void LoadGames(bool clearStatus, bool clearExistingGames)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "Scoreboard games (.xml)|*.xml"
            };

            if (openDialog.ShowDialog() == true)
            {
                Score.LoadGamesFromFile(openDialog.FileName, clearStatus, clearExistingGames);
            }
        }

        protected void LoadGamesFromTourney()
        {
            Score.Tourney.LoadGames(this);
        }

        private void LoadGamesClick(object sender, RoutedEventArgs e)
        {
            LoadGames(false, true);  
        }

        private void LoadGamesWithoutStatusClick(object sender, RoutedEventArgs e)
        {
            LoadGames(true, true);
        }

        private void LoadGamesFromTourneyClick(object sender, RoutedEventArgs e)
        {
            LoadGamesFromTourney();
        }

        private void UpdateGamesFromTourneyClick(object sender, RoutedEventArgs e)
        {
            Score.Tourney.UpdateGameDetails();
        }

        private void TourneyLogoutClick(object sender, RoutedEventArgs e)
        {
            Score.Tourney.LogOut();
        }

        private void TourneyReauthenticateClick(object sender, RoutedEventArgs e)
        {
            Score.Tourney.LogOut();
            Score.Tourney.Authenticate();
        }

        private void TourneyShowLogClick(object sender, RoutedEventArgs e)
        {
            string tourneyEvents = String.Join("\n", Score.Tourney.Events);
            StatisticsWindow.ShowStatistics(this, tourneyEvents);
        }

        private void MergeGamesClick(object sender, RoutedEventArgs e)
        {
            LoadGames(false, false);
        }        

        private void RestartGamesClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will find the game that should be in progress (based on the current time) and resume operation from there.\n\n"
                        + "You would use this after modifying the times of games.\n\n"
                        + "Do you want to continue?"
                        , "Restart Games?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Score.RestartGames();
            }
        }

        private void GameEventListViewScrollChanged
            (object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
            {
                ((ScrollViewer)e.OriginalSource).ScrollToEnd();
            }
        }

        private void CalculateStatisticsClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.CalculateStatistics(3, 1, 0);
            FlowDocumentWindow.ShowStatistics(this, statistics);
        }

        private void CalculateTopGoalScorersClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.CalculateTopGoalScorers();
            FlowDocumentWindow.ShowStatistics(this, statistics);
        }        

        private void CopyResultsClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.ResultsText();
            StatisticsWindow.ShowStatistics(this, statistics);
        }

        private void CopyLogClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.LogText();
            StatisticsWindow.ShowStatistics(this, statistics);
        }        

        private void Team1EditClick(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentOrEndedGame != null)
            {
                string text = Score.CurrentOrEndedGame.Team1Color;
                if (TextDialog.ShowTextDialog(this, "Enter Team 1 Colour", "Colour", ref text))
                {
                    Score.CurrentOrEndedGame.Team1Color = String.IsNullOrEmpty(text) ? null : text;
                }
            }
        }

        private void Team2EditClick(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentOrEndedGame != null)
            {
                string text = Score.CurrentOrEndedGame.Team2Color;
                if (TextDialog.ShowTextDialog(this, "Enter Team 2 Colour", "Colour", ref text))
                {
                    Score.CurrentOrEndedGame.Team2Color = String.IsNullOrEmpty(text) ? null : text;
                }
            }
        }

        private void EditGameClick(object sender, RoutedEventArgs e)
        {
            EditGame();
        }

        private void EditGame()
        {
            bool canEdit = true;
            if (Score.LockResults)
            {
                canEdit = WindowsAuthentication.Authenticate("Results are Locked. Authenticate to Continue.", true);
            }

            if (canEdit && _gamesListView.SelectedItem != null)
            {
                Game game = (Game)_gamesListView.SelectedItem;
                EditGameWindow.EditGame(this, game);
                Score.Tourney.ApplyGame(game);
                Score.SendGame(true);
                game.LogEvent("Edit Game");                
            }
        }

        private void RemoveGameClick(object sender, RoutedEventArgs e)
        {
            RemoveGame();
        }

        private void RemoveGame()
        {
            if (_gamesListView.SelectedItem != null)
            {
                Game game = (Game)_gamesListView.SelectedItem;
                if (game != null)
                {
                    game.LogEvent("Remove Game");
                    Score.RemoveGame(game);                    
                }
            }
        }

        private void EndExtraPeriodClick(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null && _periodsListView.SelectedItem != null)
            {
                GamePeriod period = (GamePeriod)_periodsListView.SelectedItem;

                period.EndNow();

                Score.NextGame(false);
                if (Score.CurrentGame != null && !Score.CurrentGame.HasEnded)
                {
                    Score.CurrentGame.StartFirstPeriodInSeconds(60);
                }                
            }
        }

        private void ScoreboardMenuButtonClick(object sender, RoutedEventArgs e)
        {
            ShowScoreboardMenu();
        }

        private void TourneyMenuButtonClick(object sender, RoutedEventArgs e)
        {
            ShowTourneyMenu();
        }

        private void ScoreboardServerClick(object sender, RoutedEventArgs e)
        {
            ScoreboardServerWindow window = new ScoreboardServerWindow(Score)
            {
                Owner = this
            };
            window.ShowDialog();

            Properties.Settings.Default.ServerPort = Score.ServerOptions.Port;
            Properties.Settings.Default.ServerActive = Score.ServerOptions.Active;
            Properties.Settings.Default.Save();
        }

        private void ShowScoreboardMenu()
        {
            _scoreboardContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            _scoreboardContextMenu.PlacementTarget = _scoreboardMenuButton;
            _scoreboardContextMenu.IsOpen = true;
        }

        private void ShowTourneyMenu()
        {
            _tourneyContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            _tourneyContextMenu.PlacementTarget = _tourneyMenuButton;
            _tourneyContextMenu.IsOpen = true;
        }

        private void EventButtonClick(object sender, RoutedEventArgs e)
        {
            ShowEventMenu();
        }
		
        private void ShowEventMenu()
        {
            _eventContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            _eventContextMenu.PlacementTarget = _eventMenuButton;
            _eventContextMenu.IsOpen = true;
        }

        private void AddExtraPeriodClick(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null)
            {                
                Game game = (Game)_gamesListView.SelectedItem;
                AddPeriodWindow.AddPeriod(this, game); 
            }    
        }

        private void RunSelectedPeriodClick(object sender, RoutedEventArgs e)
        {
            RunSelectedPeriod();
        }

        private void RunSelectedPeriod()
        {
            if (_gamesListView.SelectedItem != null && _periodsListView.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("This will start the selected period with at least 1 minute on the clock.\n\nAre you sure?", "Run Reriod", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Game game = (Game)_gamesListView.SelectedItem;
                    GamePeriod period = (GamePeriod)_periodsListView.SelectedItem;            
                    RunPeriod(game, period);
                }
            }
            else
            {
                MessageBox.Show("Select a Period");
            }
        }

        private void RunPeriod(Game game, GamePeriod period)
        {
            if (period.EndTime < DateTime.Now)
            {
                // Start Paused with 1 minute remaining;                
                TimeSpan remaining = new TimeSpan(0, 1, 0);                
                TimeSpan adjustment = DateTime.Now - period.EndTime + remaining;
                period.EndTime = DateTime.Now + remaining;
                game.Periods.ModifyFollowingTimes(period, adjustment, true);
            }
            
            game.CalculateResult();

            int selectedIndex = game.Periods.IndexOf(period);
            game.Periods.CurrentIndex = selectedIndex;
            Score.CurrentGame = game;
        }

        private void PeriodButtonClick(object sender, RoutedEventArgs e)
        {
            ShowPeriodMenu();
        }
		
        private void ShowPeriodMenu()
        {
            _periodContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            _periodContextMenu.PlacementTarget = _periodMenuButton;
            _periodContextMenu.IsOpen = true;
        }

        private void Main_Activated(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                DataContext = Score;                
            }
        }

        private void EventMenuClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Border)
            {
                ContextMenu contextMenu = _eventMenu;
                contextMenu.IsOpen = true;
            }
        }

        private void EventEditClick(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null && _gameEventListView.SelectedItem != null)
            {
                GameEvent gameEvent = ((GameEventView)_gameEventListView.SelectedItem).GameEvent;
                if (gameEvent.EventType.Contains("Card"))
                {
                    EditCard(gameEvent);
                }
                else if (gameEvent.EventType == "Goal")
                {
                    EditGoal(gameEvent);
                }
            }
        }

        private void EditCard(GameEvent gameEvent)
        {
            Score.EditCard(this, (Game)_gamesListView.SelectedItem, gameEvent);
            ScrollEventsToEnd();
        }

        private void EditGoal(GameEvent gameEvent)
        {
            Game game = (Game)_gamesListView.SelectedItem;

            string team = gameEvent.Team;
            string player = gameEvent.Player;

            if (Players.SelectPlayer(this, game, ref team, ref player))
            {
                gameEvent.Team = team;
                gameEvent.Player = player;

                game.FilterGameEvents();
                game.CalculateScoreFromEvents();
                Score.SaveGames();
                ScrollEventsToEnd();
                Score.Tourney.ApplyGame(game);
            }
        }

        private void EventRemoveClick(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null && _gameEventListView.SelectedItem != null)
            {
                if (MessageBox.Show("Are you sure you want to remove this event?", "Remove Event", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Game game = (Game)_gamesListView.SelectedItem;
                    GameEvent gameEvent = ((GameEventView)_gameEventListView.SelectedItem).GameEvent;
                    Score.RemoveGameEvent(game, gameEvent);
                }
            }
        }        

        private void ScrollEventsToEnd()
        {            
            if (_gameEventListView.Items.Count > 0)
            {
                Object item = _gameEventListView.Items[^1];                
                _gameEventListView.UpdateLayout();
                _gameEventListView.ScrollIntoView(item);
            }
        }

        private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollEventsToEnd();
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        private void EventScrollLeft(object sender, RoutedEventArgs e)
        {
            if (GetScrollViewer(_gameEventListView) is ScrollViewer scrollViwer)
            {
                scrollViwer.ScrollToHorizontalOffset(scrollViwer.HorizontalOffset - 1);
            }
        }

        private void EventScrollRight(object sender, RoutedEventArgs e)
        {
            if (GetScrollViewer(_gameEventListView) is ScrollViewer scrollViwer)
            {
                scrollViwer.ScrollToHorizontalOffset(scrollViwer.HorizontalOffset + 1);
            }
        }

        private void EventAddGoal(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null)
            {
                Game game = (Game)_gamesListView.SelectedItem;
                string team = String.Empty;
                string player = String.Empty;


                if (Players.SelectPlayer(this, game, ref team, ref player))
                {
                    if (!String.IsNullOrWhiteSpace(team) && !String.IsNullOrWhiteSpace(player))
                    game.LogEvent("Goal", team, player, String.Empty);                    
                    game.FilterGameEvents();
                    game.CalculateScoreFromEvents();
                    Score.SendGame(true);
                    Score.SaveGames();
                    ScrollEventsToEnd();
                }
            }
        }

        private void EventAddCard(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null)
            {
                Game game = (Game)_gamesListView.SelectedItem;
                                
                string card = String.Empty;
                string team = String.Empty;
                string player = String.Empty;
                string infringement = String.Empty;
                string penaltyDuration = String.Empty;

                if (Cards.SelectCard(this, game, ref team, ref card, ref player, ref infringement, ref penaltyDuration))
                {
                    game.LogEvent(card + " Card", team, player, infringement);
                    game.FilterGameEvents();
                    game.CalculateScoreFromEvents();
                    Score.SaveGames();
                    ScrollEventsToEnd();
                }
            }
        }

        private void Team1CardsClick(object sender, RoutedEventArgs e)
        {
            _team1CardsMenu.IsOpen = true;
        }

        private void Team1CardCancelClick(object sender, RoutedEventArgs e)
        {
            if (_team1Cards.SelectedItem != null && _team1Cards.SelectedItem is Card card)
            {
                Score.Team1Cards.Remove(card);
            }
        }

        private void Team2CardsClick(object sender, RoutedEventArgs e)
        {
            _team2CardsMenu.IsOpen = true;
        }

        private void Team2CardCancelClick(object sender, RoutedEventArgs e)
        {
            if (_team2Cards.SelectedItem != null && _team2Cards.SelectedItem is Card card)
            {
                Score.Team2Cards.Remove(card);
            }
        }

        private void RestartProtoSlaveClick(object sender, RoutedEventArgs e)
        {
            Score.RestartProtoSlave();
        }        
    }
}