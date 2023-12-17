using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
namespace GalacticSwampBot {
    public class RegenHandler {
        private string _content;
        private DiscordGuild _discordGuild;
        public RegenHandler(DiscordGuild guild, string messageContent)
        {
            _discordGuild = guild;
            _content = messageContent;
        }

        public async Task Handle()
        {
            var parsed = Parse();
            if (parsed != null){
                var messages = await _discordGuild.GetChannel(Constants.Channels.REGEN_LOGS).GetMessagesAsync();
                foreach (DiscordMessage message in messages){
                    //⬆️ Level {player['lvl']} {player['pseudo'].upper()}: 🌍 main base is now back 🌱 {score_per_base[player['MB_lvl']-1]} pts
                    if (message.Content.Contains(parsed.Item1) && message.Content.Replace("*",string.Empty).Contains(parsed.Item2)){
                        await message.DeleteAsync();
                        // break;
                    }
                }
            }
        }
        private Tuple<string, string> Parse()
        {
            var splitted = _content.Split(' ');
            if (splitted.Length > 8){
                return new Tuple<string, string>(
                splitted[4].Replace("*",string.Empty).TrimEnd(':'),
                (splitted[6] + " " + splitted[7] + (splitted[8] == "destroyed" ? string.Empty : " " + splitted[8])).Replace("*",string.Empty)
                );
            }
            return null;
        }
    }
}
