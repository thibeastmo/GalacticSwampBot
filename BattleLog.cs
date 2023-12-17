using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GalacticSwampBot.Library;
using GalacticSwampBot.Library.Database;

namespace GalacticSwampBot
{
    internal class BattleLog
    {
        public string PlayerName { get; set; }
        public short PlanetNumber { get; set; }
        public Coordinates Coordinates { get; set; }

        public static BattleLog Parse(string text)
        {
            if (text == null || text.Length == 0){
                return null;
            }
            const string playerNameRegexPattern = @"(.*)? \(";
            const string planetNumberRegexPattern = @"\((.*)?:";
            const string coordinatesRegexPattern = @"-?[0-9]*, -?[0-9]*";

            Regex playerNameRegex = new Regex(playerNameRegexPattern);
            Regex planetNumberRegex = new Regex(planetNumberRegexPattern);
            Regex coordinatesRegex = new Regex(coordinatesRegexPattern);

            var playerNameMatch = playerNameRegex.Match(text);
            var planetNumberMatch = planetNumberRegex.Match(text);
            var coordinatesMatch = coordinatesRegex.Match(text);

            if (playerNameMatch.Success && planetNumberMatch.Success && coordinatesMatch.Success) {
                string[] splittedPlanetNumber = planetNumberMatch.Groups[1].Value.Split(' ');
                short colonyNumber = 0;
                if (splittedPlanetNumber[1].ToLower() == "colony"){
                    colonyNumber = short.Parse(splittedPlanetNumber[0].Replace("rd", "th").Replace("nd", "th").Replace("st", "th").Replace("th", string.Empty));
                }
                string[] splitted = coordinatesMatch.Value.Replace(", ", ",").Split(',');
                var point = new Coordinates() { x = Int32.Parse(splitted[0]), y = Int32.Parse(splitted[1])};

                return new BattleLog()
                {
                    PlayerName = playerNameMatch.Groups[1].Value,
                    PlanetNumber = colonyNumber,
                    Coordinates = point
                };
            }
            else{
                return null;
            }
        }
        public async Task<int> AddToDatabase()
        {
            User user = null;
            try{
                user = await User.GetUserByName(PlayerName);
            }
            catch (Exception ex){
                return 3;
            }
            List<Colony> storedColonies = await DatabaseHandler.GetStoredColonies(user.Id);
            if (PlanetNumber > user.Planets.Count-1){
                return 4;
            }
            else if (PlanetNumber == 0){
                return 6;
            }
            if (Coordinates.x < 0 || Coordinates.y < 0){
                return 5;
            }
            
            foreach (Colony storedColony in storedColonies){
                if (storedColony.number == PlanetNumber){
                    storedColony.colo_coord = Coordinates;
                    bool updated = await DatabaseHandler.FindAndReplace(storedColony);
                    return updated ? 1 : 0;
                }
            }
            var newColony = new Colony()
            {
                colo_coord = Coordinates,
                number = PlanetNumber,
                colo_lvl = user.Planets[PlanetNumber].HQLevel,
                colo_sys_name = "?",
                id_gl = user.Id
            };
            bool added = await DatabaseHandler.AddColony(newColony);
            return added ? 2 : 0;
        }
    }
}
