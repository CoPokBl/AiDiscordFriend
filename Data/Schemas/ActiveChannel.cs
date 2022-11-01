namespace AiDiscordFriend.Data.Schemas; 

public class ActiveChannel {
    public ulong Id { get; set; }
    public DateTime PeriodEnd;
    public bool IsActive;
    
    public ActiveChannel(ulong id, Random random) {
        Id = id;
        int downPeriodMinutes = int.Parse(Program.Config!["down_time_length_minutes"]);
        int downPeriodVariation = int.Parse(Program.Config!["down_time_length_variation"]);
        // Make the period end in the activePeriodMinutes +- activePeriodVariation minutes
        PeriodEnd = DateTime.Now.AddMinutes(downPeriodMinutes + random.Next(-downPeriodVariation, downPeriodVariation));
        IsActive = false;
    }
}