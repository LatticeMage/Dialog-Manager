/// <summary>
/// The JsonNodesHandler.cs contains classes to recursively load JSON files representing dialogue nodes and their choices.
/// </summary>
/// 
/// <remarks>
/// The primary class, JsonNodesHandler, provides functionality to:
/// - Recursively load JSON files starting from a root or entry node.
/// - Keep track of the loaded nodes to prevent duplicates.
/// - Save all loaded nodes back to their respective files.
///
/// The file also defines a structure for the individual dialogue nodes (JsonNode) and their choices (Choice).
///
/// Usage:
/// 1. Create an instance of the JsonNodesHandler class.
/// 2. Use the LoadRecursive method, providing the path to the root JSON file.
/// 3. Access the Root property to get the entry node, and use the Nodes property to access all loaded nodes.
/// 4. If any changes are made to the nodes, you can use the SaveAll method to save them back to the files.
/// </remarks>
///

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Dialog.Json
{

    public class Choice
    {
        public string TextId { get; set; }
        public string NextNode { get; set; }
    }

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
            string updatedJsonContent = JsonConvert.SerializeObject(this, Formatting.Indented); 
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

    public class JsonNodesHandler
    {
        private HashSet<string> loadedFiles = new HashSet<string>();
        public Dictionary<string, JsonNode> Nodes { get; private set; } = new Dictionary<string, JsonNode>();
        public JsonNode Root { get; private set; }

        public void LoadRecursive(string currentPath)
        {
            if (loadedFiles.Contains(currentPath))
            {
                Debug.Log($"Duplicated Node detected: {currentPath} has already been loaded. Stopping recursive load.");
                return;
            }

            JsonNode node = JsonNode.FromFile(currentPath);
            Nodes[node.Id] = node;

            // Assigning the root node if it hasn't been set yet
            if (Root == null)
            {
                Root = node;
            }

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

        public void Clear()
        {
            Nodes.Clear();
            loadedFiles.Clear();
        }
    }
}