using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Mail;
using System.Net;
using Microsoft.Win32;

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

            _secondary = new Secondary();
            _secondary.DataContext = Score;

            // Todo - Do not use System.Windows.Forms and System.Drawing;
            if (screenIndex < System.Windows.Forms.SystemInformation.MonitorCount)
            {
                System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
                System.Windows.Forms.Screen screen = screens[screenIndex];

                Secondary.WindowStyle = WindowStyle.None;
                Secondary.ResizeMode = ResizeMode.NoResize;

                Secondary.Show();

                Secondary.Left = screen.Bounds.Left;
                Secondary.Top = screen.Bounds.Top;

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

        private void bTeam1ScoreSubtract_Click(object sender, RoutedEventArgs e)
        {
            Score.Team1NoGoal();            
        }

        private void Team1Goal()
        {
            string player = Score.RecordGoalScorers ? Players.SelectPlayer(this) : Players.Unknown;
            if (!String.IsNullOrEmpty(player))
            {
                if (player != Players.Unknown)
                {
                    Score.Team1Goal(player);
                }
                else
                {
                    Score.Team1Goal(String.Empty);
                }
            }
        }

        private void bTeam1ScoreAdd_Click(object sender, RoutedEventArgs e)
        {
            Team1Goal();            
        }
        
        private void bTeam2ScoreSubtract_Click(object sender, RoutedEventArgs e)
        {
            Score.Team2NoGoal();
        }

        private void Team2Goal()
        {
            string player = Score.RecordGoalScorers ? Players.SelectPlayer(this) : Players.Unknown;
            if (!String.IsNullOrEmpty(player))
            {
                if (player != Players.Unknown)
                {
                    Score.Team2Goal(player);
                }
                else
                {
                    Score.Team2Goal(String.Empty);                    
                }
            }
        }

        private void bTeam2ScoreAdd_Click(object sender, RoutedEventArgs e)
        {
            Team2Goal();
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
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
                bTeam1Card_Click(null, null);
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
                bTeam2Card_Click(null, null);
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
                Score.AddGames();
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
        }

        private void bTeam1Card_Click(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentGame != null)
            {
                Score.SelectCard(this, Score.CurrentGame.Team1);
            }
        }

        private void bTeam2Card_Click(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentGame != null)
            {
                Score.SelectCard(this, Score.CurrentGame.Team2);
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

        private void bPause_Click(object sender, RoutedEventArgs e)
        {
            PauseResume();
        }        

        private void bSendLog_Click(object sender, RoutedEventArgs e)
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

        private void _secondaryScreen1_Click(object sender, RoutedEventArgs e)
        {
            CreateSecondary(0);
        }

        private void _secondaryScreen2_Click(object sender, RoutedEventArgs e)
        {
            CreateSecondary(1);
        }

        private void _pauseResumeShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.PauseResumeShotTime();
        }

        private void _resetShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.ResetShotTime();
        }

        private void _resetResumeShotButtonClick(object sender, RoutedEventArgs e)
        {
            Score.ResetResumeShotTime();
        }

        private bool OnAddGames(object input, object output)
        {
            return AddGamesWindow.AddGames(this, (Score)input, (GameList)output);
        }

        private bool OnAddExtraPeriod(object input, object output)
        {
            if (AddPeriodWindow.AddPeriod(this, (Game)input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool OnGameCompleted(object input, object output)
        {
            GameCompletedWindow.ShowGameCompletedWindow(this, (Game)input, (Game)output);
            return true;
        }

        private void _saveGamesClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".xml";
            saveDialog.Filter = "Scoreboard games (.xml)|*.xml";

            if (saveDialog.ShowDialog() == true)
            {
                Score.SaveGamesToFile(saveDialog.FileName);
            }

        }

        protected void LoadGames(bool clearStatus, bool clearExistingGames)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.DefaultExt = ".xml";
            openDialog.Filter = "Scoreboard games (.xml)|*.xml";

            if (openDialog.ShowDialog() == true)
            {
                Score.LoadGamesFromFile(openDialog.FileName, clearStatus, clearExistingGames);
            }
        }

        protected void LoadGamesFromTourney()
        {
            Tourney.SelectAndAddGames(this, Score);
        }

        private void _loadGamesClick(object sender, RoutedEventArgs e)
        {
            LoadGames(false, true);  
        }

        private void _loadGamesWithoutStatusClick(object sender, RoutedEventArgs e)
        {
            LoadGames(true, true);
        }

        private void _loadGamesFromTourneyClick(object sender, RoutedEventArgs e)
        {
            LoadGamesFromTourney();
        }

        private void _mergeGamesClick(object sender, RoutedEventArgs e)
        {
            LoadGames(false, false);
        }        

        private void _restartGamesClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will find the game that should be in progress (based on the current time) and resume operation from there.\n\n"
                        + "You would use this after modifying the times of games.\n\n"
                        + "Do you want to continue?"
                        , "Restart Games?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Score.RestartGames();
            }
        }

        private void _gameEventListViewScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
            {
                ((ScrollViewer)e.OriginalSource).ScrollToEnd();
            }
        }

        private void _calculateStatisticsClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.CalculateStatistics(3, 1, 0);
            FlowDocumentWindow.ShowStatistics(this, statistics);
        }

        private void _calculateTopGoalScorersClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.CalculateTopGoalScorers();
            FlowDocumentWindow.ShowStatistics(this, statistics);
        }        

        private void _copyResultsClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.ResultsText();
            StatisticsWindow.ShowStatistics(this, statistics);
        }

        private void _copyLogClick(object sender, RoutedEventArgs e)
        {
            string statistics = Score.LogText();
            StatisticsWindow.ShowStatistics(this, statistics);
        }

        private void Team1EditClick(object sender, RoutedEventArgs e)
        {
            if (Score.CurrentOrEndedGame != null)
            {
                string text = Score.CurrentOrEndedGame.Team1Color;
                if (TextDialog.ShowTextDialog("Enter Team 1 Colour", "Colour", ref text))
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
                if (TextDialog.ShowTextDialog("Enter Team 2 Colour", "Colour", ref text))
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
            if (_gamesListView.SelectedItem != null)
            {
                Game game = (Game)_gamesListView.SelectedItem;
                EditGameWindow.EditGame(this, game);
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

        private void _endExtraPeriodClick(object sender, RoutedEventArgs e)
        {
            if (_gamesListView.SelectedItem != null && _periodsListView.SelectedItem != null)
            {
                Game game = (Game)_gamesListView.SelectedItem;
                GamePeriod period = (GamePeriod)_periodsListView.SelectedItem;

                period.EndNow();

                Score.NextGame(false);
                if (Score.CurrentGame != null && !Score.CurrentGame.HasEnded)
                {
                    Score.CurrentGame.StartFirstPeriodInSeconds(60);
                }                
            }
        }

        private void _scoreboardMenuButtonClick(object sender, RoutedEventArgs e)
        {
            ShowScoreboardMenu();
        }

        private void _scoreboardServerClick(object sender, RoutedEventArgs e)
        {            
            ScoreboardServerWindow window = new ScoreboardServerWindow(Score);
            window.Owner = this;
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

        private void Main_Activated(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                DataContext = Score;                
            }
        }
    }
}