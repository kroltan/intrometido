using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace IntroBot {
    public class IntroductionService {
        private readonly IKeyValueDatabase _database;
        private readonly DiscordSocketClient _client;

        public IntroductionService(IKeyValueDatabase database, DiscordSocketClient client) {
            _database = database;
            _client = client;
        }


        public async Task<ISocketMessageChannel> GetChannel(IGuild guild) {
            var channelId = await _database.GetValue<ulong>(Keys.IntroductionChannel(guild));
            if (!channelId.HasValue) {
                return null;
            }

            return (ISocketMessageChannel) _client.GetChannel(channelId.Value);
        }

        public async Task SetChannel(IGuild guild, IChannel channel) {
            await _database.SetValue(Keys.IntroductionChannel(guild), channel.Id);
        }

        public async Task<IMessage> GetIntroduction(IGuild guild, IUser user) {
            var dbIntroKey = Keys.UserIntroduction(guild, user);

            var id = await _database.GetValue<ulong>(dbIntroKey);
            if (!id.HasValue) {
                return null;
            }

            var channel = await GetChannel(guild);
            if (channel == null) {
                return null;
            }

            var message = await channel.GetMessageAsync(id.Value);
            if (message == null) {
                await _database.RemoveKey(dbIntroKey);
            }
            return message;
        }
    }
}
