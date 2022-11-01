using Discord.WebSocket;

namespace AiDiscordFriend.Commands; 

public interface ICommandExecutionHandler {
    public Task Execute(SocketSlashCommand cmd, DiscordSocketClient client);
}