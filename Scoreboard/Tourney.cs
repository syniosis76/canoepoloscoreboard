using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Windows;

namespace Scoreboard
{
    class Tourney
    {
        private static HttpClient _httpClient = new HttpClient();

        private string _baseUrl;
        private Window _owner;

        public static bool SelectAndAddGames(Window owner, Score score)
        {
            string tourneyUrl = Properties.Settings.Default.TourneyUrl;
            Tourney tourney = new Tourney(owner, tourneyUrl);

            string tournamentId = tourney.SelectTournament();
            if (!String.IsNullOrWhiteSpace(tournamentId))
            {

            }

            return false;
        }

        public Tourney(Window owner, string baseUrl)
        {
            _owner = owner;
            _baseUrl = baseUrl;
            if (Tourney._httpClient == null)
            {
                Tourney._httpClient = new HttpClient();
            }
        }

        public string SelectTournament()
        {           
            try
            {
                ProcessingWindow.ShowProcessing(_owner, "Listing Tournaments...");

                JObject tournaments = GetRequestAsJObject("/data/tournaments");
                if (tournaments != null)
                {
                    SelectListWindow selectListWindow = new SelectListWindow();
                    selectListWindow.Owner = _owner;
                    selectListWindow.Title = "Select Tournament";

                    foreach (JObject tournament in tournaments["tournaments"])
                    {
                        selectListWindow.Items.Add(new SelectItem((string)(tournament["id"]["value"]), (string)tournament["name"]));
                    }

                    ProcessingWindow.HideProcessing();

                    if (selectListWindow.ShowDialog() == true)
                    {
                        return selectListWindow.SelectedId;
                    }
                }
            }
            finally
            {
                ProcessingWindow.HideProcessing();
            }               

            return null;
        }

        public string GetRequestAsString(string urlSuffix)
        {
            HttpResponseMessage response = _httpClient.GetAsync(new Uri(_baseUrl + urlSuffix)).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        public JObject GetRequestAsJObject(string urlSuffix)
        {
            string result = GetRequestAsString(urlSuffix);
            if (!String.IsNullOrWhiteSpace(result))
            {
                return JObject.Parse(result);
            }
            return null;
        }
    }
}
