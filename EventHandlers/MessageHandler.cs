using AiDiscordFriend.Data;
using AiDiscordFriend.Data.Schemas;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace AiDiscordFriend.EventHandlers; 

public static class MessageHandler {
    public static readonly List<DateTime> LastMessages = new();
    private static readonly Random Random = new();
    private static readonly List<ActiveChannel> ActiveChannels = new();

    public static async Task OnMessage(SocketMessage msg) {
        
        if (Bot.IsMe(msg.Author)) {
            return;
        }
        
        if (msg.Channel is IDMChannel) {
            DmHandler.Run(msg);
        }

        // Check if activeChannels has a channel with the id of this channel
        if (ActiveChannels.All(x => x.Id != msg.Channel.Id)) {
            ActiveChannels.Add(new ActiveChannel(msg.Channel.Id, Random));
        }
        ActiveChannel activeChannel = ActiveChannels.First(x => x.Id == msg.Channel.Id);

        switch (activeChannel.IsActive) {
            case false when activeChannel.PeriodEnd < DateTime.Now: {
                int activePeriodMinutes = int.Parse(Program.Config!["active_period_length_minutes"]);
                int activePeriodVariation = int.Parse(Program.Config!["active_period_length_variation"]);
                // Make the period end in the activePeriodMinutes +- activePeriodVariation minutes
                activeChannel.PeriodEnd = DateTime.Now.AddMinutes(activePeriodMinutes + Random.Next(-activePeriodVariation, activePeriodVariation));
                activeChannel.IsActive = true;
                Logger.Debug("Active period started for channel " + msg.Channel.Name + " until " + activeChannel.PeriodEnd);
                break;
            }
            case true when activeChannel.PeriodEnd < DateTime.Now: {
                int downPeriodMinutes = int.Parse(Program.Config!["down_time_length_minutes"]);
                int downPeriodVariation = int.Parse(Program.Config["down_time_length_variation"]);
                // Make the period end in the activePeriodMinutes +- activePeriodVariation minutes
                activeChannel.PeriodEnd = DateTime.Now.AddMinutes(downPeriodMinutes + Random.Next(-downPeriodVariation, downPeriodVariation));
                activeChannel.IsActive = false;
                Logger.Debug("Inactive period started for channel " + msg.Channel.Name + " until " + activeChannel.PeriodEnd);
                break;
            }
        }
        
        // Replace the activeChannel in the list with the updated one
        ActiveChannels[ActiveChannels.FindIndex(x => x.Id == msg.Channel.Id)] = activeChannel;

        if (!activeChannel.IsActive) {
            Logger.Debug("Channel " + msg.Channel.Name + " is inactive for another " + (activeChannel.PeriodEnd - DateTime.Now).TotalMinutes + " minutes");
            return;
        }
        
        // If there have been more than 60 messages in the last hour, ignore the message
        if (LastMessages.Count > int.Parse(Program.Config!["messages_per_hour_max"])) {
            // Remove all messages older than an hour
            LastMessages.RemoveAll(x => x < DateTime.Now.AddHours(-1));
        }
        LastMessages.Add(DateTime.Now);
        
        // Gather the last 10 messages to use as context for ai
        IEnumerable<IMessage>? messages = await msg.Channel.GetMessagesAsync(5).FlattenAsync();
        IMessage[] messagesArray = messages.ToArray();
        // Put context into format
        string currentContext = "\nDate and Time: " + DateTime.Now + "\n" + "\n" + "Discord Channel: " + msg.Channel.Name + "\n";
        string context = currentContext + string.Join("\n", messagesArray.Select(m => 
            (Bot.IsMe(m.Author) ? "Bot: " : m.Author.Username) + ": " + m.Content));
        context += "\n" + (Bot.IsMe(msg.Author) ? "Bot: " : msg.Author.Username) + ": " + msg.Content;

        await msg.Channel.TriggerTypingAsync();
        
        // Send the context to the ai and get a response
        string resp = await AiManager.GetAiResponse(context, msg.Author);
        
        // Send the response to the user
        await msg.Channel.SendMessageAsync(resp);
        Logger.Debug("Sent message to #" + msg.Channel.Name + ": " + resp);
    }
    
}