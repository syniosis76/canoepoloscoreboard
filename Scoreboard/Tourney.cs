using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace Scoreboard
{
    class Tourney
    {
        private static HttpClient _httpClient = new HttpClient();

        private string _baseUrl;

        public static bool SelectAndAddGames(Score score)
        {
            string tourneyUrl = Properties.Settings.Default.TourneyUrl;
            Tourney tourney = new Tourney(tourneyUrl);

            string tournamentId = tourney.SelectTournament();
            if (!String.IsNullOrWhiteSpace(tournamentId))
            {

            }

            return false;
        }

        public Tourney(string baseUrl)
        {
            _baseUrl = baseUrl;
            if (Tourney._httpClient == null)
            {
                Tourney._httpClient = new HttpClient();
            }
        }

        public string SelectTournament()
        {
            JObject tournaments = GetRequestAsJObject("/data/tournaments");
            if (tournaments != null)
            {                
                SelectListWindow selectListWindow = new SelectListWindow();
                selectListWindow.Title = "Select Tournament";

                foreach (JObject tournament in tournaments["tournaments"])
                {
                    selectListWindow.Items.Add(new SelectItem((string)(tournament["id"]["value"]), (string)tournament["name"]));                        
                }

                if (selectListWindow.ShowDialog() == true)
                {
                    return selectListWindow.SelectedItem.Id;
                }
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
