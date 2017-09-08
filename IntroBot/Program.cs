using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using IntroBot.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace IntroBot {
    internal class Program {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private FileBackedKeyValueDatabase _database;

        private static void Main() {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync() {
            using (_database = new FileBackedKeyValueDatabase(Settings.Default.DatabasePath)) {
                _client = new DiscordSocketClient();
                _client.Log += Log;
                _client.MessageReceived += Client_MessageReceived;

                _commands = new CommandService();
                await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

                _services = SetupInjection();

                await _client.LoginAsync(TokenType.Bot, Settings.Default.DiscordBotToken);
                await _client.StartAsync();

                await Task.Delay(-1);
            }
        }

        private IServiceProvider SetupInjection() {
            var collection = new ServiceCollection();
            collection.AddTransient(_ => _client);
            collection.AddTransient<IKeyValueDatabase, FileBackedKeyValueDatabase>(_ => _database);
            collection.AddTransient<IntroductionService>();

            return collection.BuildServiceProvider();
        }

        private async Task Client_MessageReceived(SocketMessage messageParam) {
            if (!(messageParam is SocketUserMessage)) {
                return;
            }
            var message = (SocketUserMessage) messageParam;

            var argPos = 0;
            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos)) {
                if (!message.Author.IsBot) {
                    await HandleIntroMessage(message);
                }
                return;
            }
            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private async Task HandleIntroMessage(SocketUserMessage message) {
            var guild = (message.Channel as SocketGuildChannel)?.Guild;
            if (guild == null) {
                return;
            }
            var introChannel = await _database.GetValue<ulong>(Keys.IntroductionChannel(guild));
            if (!introChannel.HasValue || introChannel.Value != message.Channel.Id) {
                return;
            }

            var introKey = Keys.UserIntroduction(guild, message.Author);
            var previousIntroId = await _database.GetValue<ulong>(introKey);
            if (previousIntroId.HasValue) {
                await message.DeleteAsync();
                return;
            }
            await _database.SetValue(introKey, message.Id);
        }

        private static Task Log(LogMessage message) {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}
