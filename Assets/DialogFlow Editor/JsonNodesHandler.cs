using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonNode
{
    public int Number { get; set; }
    public string Id { get; set; }  // Will store the ID without folder and .json
    public string Dialog { get; set; }
    public List<Choice> Choices { get; set; } = new List<Choice>();
    public string FilePath { get; set; }  // Storing the full file path including .json

    public static JsonNode FromFile(string filePath)
    {
        var id = Path.GetFileNameWithoutExtension(filePath);
        filePath += ".json";  // Append .json extension

        string jsonContent = File.ReadAllText(filePath);
        var node = JsonConvert.DeserializeObject<JsonNode>(jsonContent);
        node.FilePath = filePath;
        node.Id = id;
        return node;
    }

    public void Save()
    {
        string updatedJsonContent = JsonConvert.SerializeObject(this);
        File.WriteAllText(this.FilePath, updatedJsonContent);
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public string FileName
    {
        get
        {
            return Path.GetFileNameWithoutExtension(FilePath);
        }
    }
}

public class Choice
{
    public string TextId { get; set; }
    public string NextNode { get; set; }
}

public class JsonNodesHandler
{
    private HashSet<string> loadedFiles = new HashSet<string>();
    public Dictionary<string, JsonNode> Nodes { get; private set; } = new Dictionary<string, JsonNode>();

    public void LoadRecursive(string currentPath)
    {
        if (loadedFiles.Contains(currentPath))
        {
            Debug.LogError($"Cycle detected: {currentPath} has already been loaded. Stopping recursive load.");
            return;
        }

        JsonNode node = JsonNode.FromFile(currentPath);
        Nodes[node.Id] = node;  // Use Id property as the key for the dictionary

        loadedFiles.Add(currentPath);

        string directory = Path.GetDirectoryName(currentPath);

        foreach (var choice in node.Choices)
        {
            if (!string.IsNullOrEmpty(choice.NextNode))
            {
                // Resolving relative paths
                string nextPath = Path.Combine(directory, choice.NextNode);  // No need to append .json here

                // Canonicalize the path to remove any ".." or "." segments
                nextPath = Path.GetFullPath(nextPath);

                // Recursive load
                LoadRecursive(nextPath);
            }
        }
    }

    public void SaveAll()
    {
        foreach (var node in Nodes.Values)
        {
            node.Save();
        }
    }

    public void Clear()
    {
        Nodes.Clear();
        loadedFiles.Clear();
    }
}
