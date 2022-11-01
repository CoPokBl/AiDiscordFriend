using AiDiscordFriend.Data;
using GeneralPurposeLib;

namespace AiDiscordFriend;

internal static class Program {

    public const string Version = "0.0.1";
    public static Dictionary<string, string>? Config;
    public static Random Random { get; } = new ();
    public static string Personality { get; private set; }

    private static readonly Dictionary<string, string> DefaultConfig = new() {
        { "token", "discord bot token" },
        { "open_ai_token", "xxxxxxxxxxxxxxxxxxxxxxxxxxxx" },
        { "active_period_length_minutes", "10" },
        { "active_period_length_variation", "20" },
        { "down_time_length_minutes", "60" },
        { "down_time_length_variation", "120" },
        { "messages_per_hour_max", "60" }
    };

    public static int Main(string[] args) {

        // Init Logger
        try {
            Logger.Init(LogLevel.Debug);
        }
        catch (Exception e) {
            Console.WriteLine("Failed to initialize logger: " + e);
            return 1;
        }
        Logger.Info("Ai Discord Friend starting...");

        // Config
        try {
            ConfigManager configManager = new ("config.json", DefaultConfig);
            Config = configManager.LoadConfig();
        }
        catch (Exception e) {
            Logger.Error("Failed to load config: " + e);
            Logger.WaitFlush();
            return 1;
        }
        Personality = File.ReadAllText("personality.txt");

        // This event gets fired when the user tried to stop the bot with Ctrl+C or similar.
        Console.CancelKeyPress += (_, _) => {
            Logger.Info("Shutting down...");
            Logger.WaitFlush();
            Environment.Exit(0);
        };

        if (args.Length != 0) {

            switch (args[0].ToLower()) {

                default:
                    Console.WriteLine("Unknown command");
                    return 1;

            }
        }

        // Init Services
        ServiceManager.Init();

        // Run bot
        List<DateTime> errors = new();
        Exception? lastError = null;
        while (true) {
            try {
                Logger.Info("Starting bot...");
                Bot bot = new ();
                bot.Run().Wait();
                Logger.Warn("Bot task exited unexpectedly");
                break;
            }
            catch (Exception e) {
                Logger.Error(e);

                // Stop if there are more than 5 errors in 1 minute
                // Remove all errors older than 5 minutes
                errors.Add(DateTime.Now);
                errors.RemoveAll(x => x < DateTime.Now - TimeSpan.FromMinutes(5));
                if (errors.Count > 5) {
                    Logger.Error("Too many errors (Possible error loop), stopping...");
                    break;
                }

                // Stop if the same error happened twice in a row
                if (lastError == e) {
                    Logger.Error("Same error twice in a row, stopping...");
                    break;
                }
                lastError = e;

                Logger.Info("Restating bot in 5 seconds...");
                Thread.Sleep(5000);
            }
        }

        Logger.WaitFlush();
        return 0;
    }
}