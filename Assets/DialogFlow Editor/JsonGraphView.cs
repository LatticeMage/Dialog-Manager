using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Collections.Generic;

public class JsonGraphView : GraphView
{
    public JsonGraphView()
    {
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    public void PopulateGraph(JsonNodesHandler nodesHandler)
    {
        // Retrieve all elements from the graph
        List<GraphElement> elementsToRemove = new List<GraphElement>(this.graphElements.ToList());

        // Remove all elements from the graph
        this.DeleteElements(elementsToRemove);

        Dictionary<string, JsonGraphNode> nodesLookup = new Dictionary<string, JsonGraphNode>();

        foreach (var entry in nodesHandler.Nodes)
        {
            var jsonNode = entry.Value;
            var graphNode = new JsonGraphNode(jsonNode);
            nodesLookup.Add(entry.Key, graphNode);
            this.AddElement(graphNode);
        }

        foreach (var graphNode in nodesLookup.Values)
        {
            foreach (var choice in graphNode.JsonNode.Choices)
            {
                if (string.IsNullOrEmpty(choice.NextNode))
                    continue; // Skip this iteration if there's no NextNode

                var targetPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(graphNode.JsonNode.FilePath), choice.NextNode));

                if (nodesLookup.TryGetValue(targetPath, out var targetGraphNode))
                {
                    // Ensure that you've defined input and output ports in your JsonGraphNode class
                    var inputPort = targetGraphNode.inputContainer.Q<Port>();
                    var outputPort = graphNode.outputContainer.Q<Port>();

                    if (inputPort == null || outputPort == null)
                        continue; // Skip this iteration if ports are not correctly set up

                    var edge = new Edge
                    {
                        input = inputPort,
                        output = outputPort
                    };

                    // Optional: Add a label to the edge here if needed.

                    this.AddElement(edge);
                }
            }
        }

    }
}
