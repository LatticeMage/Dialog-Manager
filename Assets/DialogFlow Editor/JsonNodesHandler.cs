using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class JsonNode
{
    public int Number { get; set; }
    public string Id { get; set; }
    public string Dialog { get; set; }
    public List<Choice> Choices { get; set; } = new List<Choice>();
    public string FilePath { get; set; } // Storing the file path

    public string SourceFile { get; set; }  // Stores the path of the file this node was loaded from

    public static JsonNode FromFile(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        var node = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonNode>(jsonContent);
        node.FilePath = filePath; // Setting the file path when loading from file
        return node;
    }
    public void Save()
    {
        string updatedJsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(this);
        File.WriteAllText(this.FilePath, updatedJsonContent);
    }
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
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
        Nodes[currentPath] = node;
        loadedFiles.Add(currentPath);

        string directory = Path.GetDirectoryName(currentPath);

        foreach (var choice in node.Choices)
        {
            if (!string.IsNullOrEmpty(choice.NextNode))
            {
                // Resolving relative paths
                string nextPath = Path.Combine(directory, choice.NextNode);

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

}
