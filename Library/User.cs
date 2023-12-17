using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace GalacticSwampBot.Library {
    public class User {
        public static async Task<User> GetUserById(int id)
        {
            string uri = "https://api.galaxylifegame.net/users/get?id=" + id;
            return await HandleRequest(uri);
        }
        public static async Task<User> GetUserBySteamId(string steamId)
        {
            string uri = "https://api.galaxylifegame.net/users/steam?steamId=" + steamId;
            return await HandleRequest(uri);
        }
        public static async Task<User> GetUserByName(string name)
        {
            string uri = "https://api.galaxylifegame.net/users/name?name=" + name;
            return await HandleRequest(uri);
        }
        public static async Task<User> HandleRequest(string url)
        {
            string json = await Requester.GetRequest(url);
            return JsonConvert.DeserializeObject<User>(json);
        }

        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public string AllianceId { get; set; }
        public List<Planet> Planets { get; set; }

        #endregion
    }
}
