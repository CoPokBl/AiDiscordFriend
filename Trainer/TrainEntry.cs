using Newtonsoft.Json;

namespace Trainer; 

public class TrainEntry {
    [JsonProperty("prompt")]
    public string Prompt { get; set; }
    
    [JsonProperty("completion")]
    public string Completion { get; set; }
    
    public TrainEntry(string prompt, string completion) {
        Prompt = prompt;
        Completion = completion;
    }
}