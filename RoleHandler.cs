using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
namespace GalacticSwampBot {
    public class RoleHandler {
        private DiscordClient _discordClient;
        private DiscordMessage _discordMessage;
        private DiscordEmoji _discordEmoji;
        private DiscordUser _discordUser;
        private bool _addRole;
        public RoleHandler(DiscordClient discordClient, DiscordMessage discordMessage, DiscordEmoji discordEmoji, DiscordUser discordUser, bool addRole)
        {
            _discordClient = discordClient;
            _discordMessage = discordMessage.Channel.GetMessageAsync(discordMessage.Id).Result;
            _discordEmoji = discordEmoji;
            _discordUser = discordUser;
            _addRole = addRole;
        }
        public async Task Handle()
        {
            if (IsWarMessage()){
                var discordRole = _discordMessage.Channel.Guild.GetRole(Constants.Roles.ACTIVE);
                var discordMember = await _discordMessage.Channel.Guild.GetMemberAsync(_discordUser.Id);
                if (_addRole){
                    await discordMember.GrantRoleAsync(discordRole);
                }
                else{
                    await discordMember.RevokeRoleAsync(discordRole);
                }
            }
        }
        public bool IsWarMessage()
        {
            return _discordMessage.Author.Id == Constants.Users.BOT && _discordEmoji.Name == "star";
        }
    }
}
