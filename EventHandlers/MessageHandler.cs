using AiDiscordFriend.Data;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace AiDiscordFriend.EventHandlers; 

public static class MessageHandler {
    public static readonly List<DateTime> LastMessages = new();
    private static readonly Random Random = new();
    private static DateTime _periodEnd = DateTime.Now;
    private static bool _isActive;

    public static async Task OnMessage(SocketMessage msg) {
        
        if (Bot.IsMe(msg.Author)) {
            return;
        }

        bool isDm = false;
        if (msg.Channel is IDMChannel) {
            DmHandler.Run(msg);
            isDm = true;
        }

        switch (_isActive) {
            case false when _periodEnd < DateTime.Now: {
                int activePeriodMinutes = int.Parse(Program.Config!["active_period_length_minutes"]);
                int activePeriodVariation = int.Parse(Program.Config["active_period_length_variation"]);
                // Make the period end in the activePeriodMinutes +- activePeriodVariation minutes
                _periodEnd = DateTime.Now.AddMinutes(activePeriodMinutes + Random.Next(-activePeriodVariation, activePeriodVariation));
                _isActive = true;
                await Bot.Client!.SetStatusAsync(UserStatus.Online);
                Logger.Debug("Active period started until " + _periodEnd);
                break;
            }
            case true when _periodEnd < DateTime.Now: {
                int downPeriodMinutes = int.Parse(Program.Config!["down_time_length_minutes"]);
                int downPeriodVariation = int.Parse(Program.Config["down_time_length_variation"]);
                // Make the period end in the activePeriodMinutes +- activePeriodVariation minutes
                _periodEnd = DateTime.Now.AddMinutes(downPeriodMinutes + Random.Next(-downPeriodVariation, downPeriodVariation));
                _isActive = false;
                await Bot.Client!.SetStatusAsync(UserStatus.Idle);
                Logger.Debug("Inactive period started until " + _periodEnd);
                break;
            }
        }
        
        if (!_isActive) {
            Logger.Debug("Inactive for another " + (_periodEnd - DateTime.Now).TotalMinutes + " minutes");
            return;
        }

        if (isDm) {
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
        IMessage[] messagesArray = messages.Reverse().ToArray();
        // Put context into format
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
        Logger.Debug("Sent message to #" + msg.Channel.Name + ": " + resp);
    }
    
}