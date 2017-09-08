using Discord;

namespace IntroBot {
    public static class Keys {
        public static string IntroductionChannel(IGuild guild) => $"intro_channel_{guild.Id}";
        public static string UserIntroduction(IGuild guild, IUser user) => $"user_intro_{guild.Id}_{user.Id}";
    }
}
