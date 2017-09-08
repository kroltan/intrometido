using Discord;

namespace IntroBot {
    public static class DiscordExtensions {
        public static string Mention(this IChannel self) => MentionUtils.MentionChannel(self.Id);
        public static string Mention(this IUser self) => MentionUtils.MentionUser(self.Id);
        public static string Mention(this IRole self) => MentionUtils.MentionRole(self.Id);
    }
}
