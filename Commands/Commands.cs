using AiDiscordFriend.Commands.ExecutionFunctions;
using Discord;

namespace AiDiscordFriend.Commands;

public static class Commands {

    public static readonly SlashCommand[] SlashCommands = {
        new ("dev-tools", "This is only for the developer",
            new [] {
                new SlashCommandArgument("command", "Dev command", false, ApplicationCommandOptionType.String)
            },
            new DevToolsCommand(),
            null,
            false),
    };

}
