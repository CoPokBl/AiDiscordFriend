using System.Text;
using System.Text.Json;
using Discord;
using GeneralPurposeLib;

namespace AiDiscordFriend.Data; 

public static class AiManager {
    
    private static readonly List<string> Contexts = new();

    public static void Save() {
        string json = JsonSerializer.Serialize(Contexts);
        File.WriteAllText("contexts.json", json);
    }

    public static async Task<string> GetAiResponse(string context, IUser user) {

        if (await ModerationCheck(context)) {
            // Bad Content Flagged
            Logger.Warn("Bad content flagged, User: " + user.Username + "#" + user.Discriminator);
            return "I'm sorry, I can't respond to that.";
        }
        
        HttpClient client = new();
        client.BaseAddress = new Uri("https://api.openai.com/v1/completions");
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.Config!["open_ai_token"]);
        string promptContent = Program.Personality + "\n" + context + "\nBot: ";
        Contexts.Add(promptContent);
        Logger.Debug(promptContent);
        StringContent content = new(
            new {
                model = Program.Config["open_ai_model"],
                prompt = promptContent,
                temperature = 0.4,
                max_tokens = 100,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0,
                user = user.Id.ToString().Sha256Hash()  // Tell OpenAI who is talking
            }.ToJson(),
            Encoding.UTF8, 
            "application/json");
    
        HttpResponseMessage response = await client.PostAsync(client.BaseAddress, content);
        string responseString = await response.Content.ReadAsStringAsync();
        JsonDocument document = JsonDocument.Parse(responseString);
        JsonElement root = document.RootElement;
        JsonElement choices = root.GetProperty("choices");
        JsonElement choice = choices[0];
        JsonElement text = choice.GetProperty("text");
        string botResponse = text.GetString() ?? "";
        botResponse = botResponse.Replace("\n", "").Replace("  ", " ");
        return botResponse == "<empty>" ? "" : botResponse;
    }
    
    private static async Task<bool> ModerationCheck(string context) {
        HttpClient client = new();
        client.BaseAddress = new Uri("https://api.openai.com/v1/moderations");
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Program.Config!["open_ai_token"]);
        StringContent content = new(
            new {
                input = context
            }.ToJson(),
            Encoding.UTF8, 
            "application/json");
        HttpResponseMessage response = await client.PostAsync(client.BaseAddress, content);
        string responseString = await response.Content.ReadAsStringAsync();
        Logger.Debug(responseString);
        JsonDocument document = JsonDocument.Parse(responseString);
        JsonElement root = document.RootElement;
        JsonElement results = root.GetProperty("results");
        JsonElement result = results[results.GetArrayLength()-1];
        return result.GetProperty("flagged").GetBoolean();
    }
    
}