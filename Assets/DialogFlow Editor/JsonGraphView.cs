using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Dialog.Graph
{
    public class JsonGraphNode : Node, ISimpleGraphNode<Json.JsonNode>
    {
        public List<Port> InputSlots { get; private set; } = new List<Port>();
        public List<Port> OutputSlots { get; private set; } = new List<Port>();
        public Json.JsonNode Data { get; private set; }

        public JsonGraphNode(Json.JsonNode data)
        {
            Data = data;

            title = data.Id;
            AddToClassList("jsonGraphNode");

            var dialogLabel = new Label { text = data.Dialog };
            mainContainer.Add(dialogLabel);
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

            foreach (var entry in nodesHandler.Nodes)
            {
                var jsonNode = entry.Value;
                var graphNode = new JsonGraphNode(jsonNode);

                // Using the SimpleGraphPositions class to determine position
                graphPositions.AddNode(graphNode);
                graphPositions.calPos();
                var position = graphPositions.getPos(jsonNode);
                graphNode.SetPosition(new Rect(position.x, position.y, 100, 150));

                // Create output port for each node
                var outputPort = graphNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                outputPort.portName = "Out";
                graphNode.outputContainer.Add(outputPort);

                // Create input port for each node
                var inputPort = graphNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
                inputPort.portName = "In";
                graphNode.inputContainer.Add(inputPort);

                nodesLookup.Add(graphNode.Data.Id, graphNode);  // Use ID as the key
                this.AddElement(graphNode);
            }

            foreach (var graphNode in nodesLookup.Values)
            {
                foreach (var choice in graphNode.Data.Choices)
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
                        Debug.LogError($"Failed to find target node with ID: {targetNodeId} from source node {graphNode.Data.Id}");
                    }
                }
            }
        }
    }
}
