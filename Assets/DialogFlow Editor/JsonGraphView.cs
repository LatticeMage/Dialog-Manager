using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

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

        int xOffset = 150; // Offset for each node on the X axis
        int xPosition = 0; // Starting position

        foreach (var entry in nodesHandler.Nodes)
        {
            var jsonNode = entry.Value;

            var graphNode = new JsonGraphNode(jsonNode);
            graphNode.SetPosition(new Rect(xPosition, 100, 100, 150));  // Set position
            xPosition += xOffset;  // Increment the position for the next node

            // Create output port for each node
            var outputPort = graphNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            graphNode.outputContainer.Add(outputPort);

            // Create input port for each node
            var inputPort = graphNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = "In";
            graphNode.inputContainer.Add(inputPort);

            nodesLookup.Add(graphNode.JsonNode.Id, graphNode);  // Use ID as the key
            this.AddElement(graphNode);
        }

        foreach (var graphNode in nodesLookup.Values)
        {
            foreach (var choice in graphNode.JsonNode.Choices)
            {
                var targetNodeId = Path.GetFileNameWithoutExtension(choice.NextNode);  // Extract the ID

                if (nodesLookup.TryGetValue(targetNodeId, out var targetGraphNode))  // Use the ID to look up
                {
                    // If the target node doesn't have an input port, create it
                    if (targetGraphNode.inputContainer.childCount == 0)
                    {
                        var inputPort = targetGraphNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
                        inputPort.portName = "In";
                        targetGraphNode.inputContainer.Add(inputPort);
                    }

                    var edge = new Edge
                    {
                        input = targetGraphNode.inputContainer[0] as Port,
                        output = graphNode.outputContainer[0] as Port
                    };

                    edge.input.Connect(edge);
                    edge.output.Connect(edge);

                    this.AddElement(edge);
                }
                else
                {
                    Debug.LogError($"Failed to find target node with ID: {targetNodeId} from source node {graphNode.JsonNode.Id}");
                }
            }
        }
    }
}
