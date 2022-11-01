using AiDiscordFriend.Data;
using AiDiscordFriend.EventHandlers;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace AiDiscordFriend;

public static class DmHandler {

    public static async void Run(SocketMessage msg) {
        // If there have been more than 60 messages in the last hour, ignore the message
        if (MessageHandler.LastMessages.Count > int.Parse(Program.Config!["messages_per_hour_max"])) {
            // Remove all messages older than an hour
            MessageHandler.LastMessages.RemoveAll(x => x < DateTime.Now.AddHours(-1));
        }
        MessageHandler.LastMessages.Add(DateTime.Now);
        
        // Gather the last 5 messages to use as context for ai
        IEnumerable<IMessage>? messages = await msg.Channel.GetMessagesAsync(5).FlattenAsync();
        IMessage[] messagesArray = messages.Reverse().ToArray();
        // Put context into format:
        // Username: Message
        // Username: Message
        // If the message is from the bot, then:
        // Bot: Message
        string currentContext = "\nDate and Time: " + DateTime.Now + "\n" + "\n" + "Discord Channel: " + msg.Channel.Name + "\n";
        string context = currentContext + string.Join("\n", messagesArray.Select(m => 
            (Bot.IsMe(m.Author) ? "Bot" : m.Author.Username) + ": " + m.Content));

        await msg.Channel.TriggerTypingAsync();
        
        // Send the context to the ai and get a response
        string resp = await AiManager.GetAiResponse(context, msg.Author);
        if (resp == "") {
            Logger.Debug("No response from ai");
            return;
        }
        
        // Send the response to the user
        await msg.Channel.SendMessageAsync(resp);
        Logger.Debug("Sent message to @" + msg.Author.Username + ": " + resp);
    }

}
