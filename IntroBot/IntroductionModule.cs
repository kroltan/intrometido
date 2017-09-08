using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace IntroBot {
    public class IntroductionModule : ModuleBase<SocketCommandContext> {
        private readonly IntroductionService _introduction;

        public IntroductionModule(IntroductionService introduction) {
            _introduction = introduction;
        }

        [
            Command(nameof(GetIntroChannel)),
            Summary("Sets the introduction channel for this server"),
            RequireUserPermission(GuildPermission.ManageChannels)
        ]
        public async Task GetIntroChannel() {
            var channel = await _introduction.GetChannel(Context.Guild);
            if (channel == null) {
                await Context.Channel.SendMessageAsync("This server hasn't configured an introduction channel!");
                return;
            }
            await Context.Channel.SendMessageAsync($"This server's introduction channel is {channel.Mention()}");
        }

        [
            Command(nameof(SetIntroChannel)), 
            Summary("Sets the introduction channel for this server"),
            RequireUserPermission(GuildPermission.ManageChannels)
        ]
        public async Task SetIntroChannel(IChannel channel) {
            await _introduction.SetChannel(Context.Guild, channel);
            await Context.Channel.SendMessageAsync($"Successfully set {channel.Mention()} as the introduction channel");
        }

        [
            Command(nameof(GetUserIntroduction)),
            Summary("Displays an user's introduction")
        ]
        public async Task GetUserIntroduction(SocketGuildUser user) {
            var message = await _introduction.GetIntroduction(user.Guild, user);
            if (message == null) {
                await Context.Channel.SendMessageAsync($"It appears that {user.Username} didn't introduce itself yet!");
                return;
            }

            var quote = new EmbedBuilder()
                .WithAuthor(message.Author.Username, message.Author.GetAvatarUrl())
                .WithTimestamp(message.Timestamp)
                .WithFooter($"#{message.Channel.Name}")
                .WithDescription(message.Content)
                .Build();

            await Context.Channel.SendMessageAsync("", embed: quote);
        }
    }
}
