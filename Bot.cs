using AiDiscordFriend.Commands;
using AiDiscordFriend.EventHandlers;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace AiDiscordFriend; 

public class Bot {
    public static DiscordSocketClient? Client;

    public static bool IsMe(SocketUser user) {
        return user.Id == Client!.CurrentUser.Id;
    }
    
    public static bool IsMe(IUser user) {
        return user.Id == Client!.CurrentUser.Id;
    }

    public async Task Run() {
        DiscordSocketConfig config = new() {
            GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        Client = new DiscordSocketClient(config);

        // Events
        Client.Log += Log;
        Client.Ready += ClientReady;
        Client.SlashCommandExecuted += SlashCommandHandler;
        Client.MessageReceived += MessageHandler.OnMessage;

        await Client.LoginAsync(TokenType.Bot, Program.Config!["token"]);
        await Client.StartAsync();
        
        // Block this task until the program is closed.
        await Task.Delay(-1);
        Logger.Warn("Bot is shutting down");
    }
    
    private Task SlashCommandHandler(SocketSlashCommand command) {
        try {
            CommandManager.Invoke(command, Client!);
        }
        catch (Exception e) {
            Logger.Error(e);
        }
        return Task.CompletedTask;
    }
    
    private async Task ClientReady() {
        Logger.Debug("Client ready");
        await Client!.SetGameAsync("I hate people");
        await Client.SetStatusAsync(UserStatus.Idle);
    }
    
    private static Task Log(LogMessage msg) {
        switch (msg.Severity) {
            case LogSeverity.Critical:
                Logger.Error(msg);
                break;
            case LogSeverity.Error:
                Logger.Error(msg);
                break;
            case LogSeverity.Warning:
                Logger.Warn(msg);
                break;
            case LogSeverity.Info:
                Logger.Info(msg);
                break;
            case LogSeverity.Verbose:
                Logger.Debug(msg);
                break;
            case LogSeverity.Debug:
                Logger.Debug(msg);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(msg));
        }

        return Task.CompletedTask;
    }
    
}