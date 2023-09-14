/// <summary>
/// JsonGraphView.cs provides a visualization of JSON dialogue data as interconnected nodes within a graph.
/// </summary>
/// 
/// <remarks>
/// Core Components:
/// - JsonGraphNode: A visual representation of individual JSON nodes in the graph. It visualizes the node's data and provides ports for connections.
/// - JsonGraphView: Manages the overall visualization of the graph by arranging nodes and connecting them based on the JSON data.
///
/// Workflow:
/// - Initialize a JsonGraphView instance.
/// - Populate the graph view by providing a JsonNodesHandler instance containing the loaded JSON data to the `PopulateGraph` method.
/// - The JsonGraphView will automatically visualize the nodes, their data, and establish connections between them.
///
/// Note:
/// The visualization relies on Unity's UIElements and the GraphView API, ensuring smooth integration within Unity's Editor environment.
/// </remarks>
///

using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Dialog.Graph
{
    public class JsonGraphNode : Node, ISimpleGraphNode<Json.JsonNode>
    {
        public List<ISimpleGraphNode<Json.JsonNode>> preNodes { get; private set; } = new List<ISimpleGraphNode<Json.JsonNode>>();
        public List<ISimpleGraphNode<Json.JsonNode>> nextNodes { get; private set; } = new List<ISimpleGraphNode<Json.JsonNode>>();
        public Json.JsonNode Data { get; private set; }
        
        public string node_id { get; set; }

        public JsonGraphNode(Json.JsonNode data)
        {
            Data = data;
            node_id = data.Id;

            title = data.Id;
            AddToClassList("jsonGraphNode");

            var dialogLabel = new Label { text = data.Dialog };
            mainContainer.Add(dialogLabel);

            // Create output port for each node
            var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort.portName = "Out";
            this.outputContainer.Add(outputPort);

            // Create input port for each node
            var inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort.portName = "In";
            this.inputContainer.Add(inputPort);
        }
    }

    public class JsonGraphView : GraphView
    {
        private NodeLayoutHandler<Json.JsonNode> graphPositions;

        public JsonGraphView()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphPositions = new NodeLayoutHandler<Json.JsonNode>();
        }

        public void PopulateGraph(Json.JsonNodesHandler nodesHandler)
        {
            // Retrieve all elements from the graph
            List<GraphElement> elementsToRemove = new List<GraphElement>(this.graphElements.ToList());

            // Remove all elements from the graph
            this.DeleteElements(elementsToRemove);
            Dictionary<string, JsonGraphNode> nodesLookup = new Dictionary<string, JsonGraphNode>();

            // 1. Add all nodes first
            foreach (var entry in nodesHandler.Nodes)
            {
                var jsonNode = entry.Value;
                var graphNode = new JsonGraphNode(jsonNode);

                // Using the SimpleGraphPositions class to determine position
                graphPositions.AddNode(graphNode);

                nodesLookup.Add(graphNode.Data.Id, graphNode);  // Use ID as the key
            }


            // 2. Fill Data

            foreach (var entry in nodesHandler.Nodes)
            {
                var currentNodeId = entry.Key;
                var currentNodeData = entry.Value;

                if (nodesLookup.TryGetValue(currentNodeId, out var currentNode))
                {
                    foreach (var choice in currentNodeData.Choices)
                    {
                        var targetNodeId = Path.GetFileNameWithoutExtension(choice.NextNode);  // Extract the ID
                        if (string.IsNullOrEmpty(targetNodeId))
                        {
                            continue;
                        }

                        if (nodesLookup.TryGetValue(targetNodeId, out var targetNode))
                        {
                            currentNode.nextNodes.Add(targetNode);
                        }
                    }
                }
            }

            // 3. Calculate positions once for all nodes
            graphPositions.calPos(nodesHandler.Root);

            // 4. Assign calculated positions to all nodes
            foreach (var entry in nodesHandler.Nodes)
            {
                var jsonNode = entry.Value;
                var graphNode = nodesLookup[jsonNode.Id];  // Retrieve the previously added node using ID as the key
                var position = graphPositions.getPos(jsonNode);

                graphNode.SetPosition(new Rect(position.x, position.y, 100, 150));
                this.AddElement(graphNode);
            }

            // 5. Get Pose
            foreach (var graphNode in nodesLookup.Values)
            {
                foreach (var choice in graphNode.Data.Choices)
                {
                    var targetNodeId = Path.GetFileNameWithoutExtension(choice.NextNode);  // Extract the ID
                    if (string.IsNullOrEmpty(targetNodeId))
                    {
                        continue;
                    }

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
                        Debug.LogError($"Failed to find target node with ID: {targetNodeId} from source node {graphNode.Data.Id}");
                    }
                }
            }
        }
    }
}
