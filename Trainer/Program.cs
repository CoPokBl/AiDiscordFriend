using Newtonsoft.Json;
using Trainer;

Console.WriteLine("Starting...");

// Read contexts.json file
string[]? contexts = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("contexts.json"));

if (contexts == null) {
    Console.WriteLine("No contexts found");
    return 1;
}

Console.WriteLine("\nStarting trainer, to exit type -quit, to skip type -skip");
List<TrainEntry> trainData = new();
foreach (string context in contexts) {
    // Print the messages
    Console.WriteLine("\n\nContext:\n");
    Console.WriteLine(context);
    
    // Ask user for response
    Console.WriteLine();
    Console.Write("Response: ");
    string response = Console.ReadLine() ?? "";
    if (response == "") {
        response = "<empty>";
    }
    if (response == "-quit") {
        Console.WriteLine("Exiting...");
        break;
    }
    if (response == "-skip") {
        Console.WriteLine("Skipping...");
        continue;
    }
    
    // Add to train data
    trainData.Add(new TrainEntry(context, response));
}

// Save train data
Console.WriteLine("Saving train data...");
string trainDataJson = trainData.Aggregate("", (current, trainEntry) => current + JsonConvert.SerializeObject(trainEntry) + "\n");
trainDataJson = trainDataJson[..^1];

File.WriteAllText("traindata.json", trainDataJson);

return 0;