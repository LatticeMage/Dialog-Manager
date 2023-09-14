/// <summary>
/// The NodeLayoutHandler class is responsible for managing and calculating the positions of graph nodes in a two-dimensional layout.
/// </summary>
/// <typeparam name="TNodeData">The type of data associated with the nodes.</typeparam>
/// 
/// <example>
/// Usage:
/// <code>
/// // 1. Instantiate the NodeLayoutHandler.
/// NodeLayoutHandler<MyNodeType> layoutHandler = new NodeLayoutHandler<MyNodeType>();
///
/// // 2. Add nodes to the layout handler.
/// ISimpleGraphNode<MyNodeType> node = ...; // Obtain or create your node.
/// layoutHandler.AddNode(node);
///
/// // 3. (Optional) If you have a predefined root or starting node, you can use it to calculate node positions.
/// // Otherwise, the first added node will be considered as the root.
/// layoutHandler.CalPos(myRootNodeData);
///
/// // 4. Retrieve the position for a specific node's data.
/// Vector2 position = layoutHandler.GetPos(specificNodeData);
/// </code>
/// </example>
/// 
/// <remarks>
/// The class uses a breadth-first search (BFS) traversal mechanism to calculate node positions. 
/// Ensure all nodes are added to the layout handler before calling the CalPos method to guarantee correct position calculations.
/// </remarks>
/// 

using System.Collections.Generic;
using UnityEngine;


namespace Dialog.Graph
{
    public interface ISimpleGraphNode<T>
    {
        List<ISimpleGraphNode<T>> preNodes { get; }
        List<ISimpleGraphNode<T>> nextNodes { get; }
        T Data { get; }
        public string node_id { get; set; }

    }

    public class NodeLayoutHandler<TNodeData>
    {
        private HashSet<ISimpleGraphNode<TNodeData>> nodes;
        private Dictionary<TNodeData, ISimpleGraphNode<TNodeData>> dataToNodeLookup;
        private Dictionary<ISimpleGraphNode<TNodeData>, Vector2> nodePositions;
        private Dictionary<ISimpleGraphNode<TNodeData>, int> nodeDepths;
        private Dictionary<ISimpleGraphNode<TNodeData>, int> nodeIndices;

        public NodeLayoutHandler()
        {
            nodes = new HashSet<ISimpleGraphNode<TNodeData>>();
            nodePositions = new Dictionary<ISimpleGraphNode<TNodeData>, Vector2>();
            dataToNodeLookup = new Dictionary<TNodeData, ISimpleGraphNode<TNodeData>>();
            nodeDepths = new Dictionary<ISimpleGraphNode<TNodeData>, int>(); // Initialize this
            nodeIndices = new Dictionary<ISimpleGraphNode<TNodeData>, int>(); // Initialize this
        }


        public void AddNode(ISimpleGraphNode<TNodeData> node)
        {
            nodes.Add(node);
            dataToNodeLookup[node.Data] = node;
        }

        public void calPos(TNodeData root)
        {
            // Initialize a queue for BFS traversal
            Queue<ISimpleGraphNode<TNodeData>> queue = new Queue<ISimpleGraphNode<TNodeData>>();

            // Add the root node to the queue with depth 0
            queue.Enqueue(dataToNodeLookup[root]);
            int currentDepth = 0;



            while (queue.Count > 0)
            {
                int nodesAtCurrentDepth = queue.Count;

                for (int i = 0; i < nodesAtCurrentDepth; i++)
                {
                    var currentNode = queue.Dequeue();
                    if (!nodeDepths.TryGetValue(currentNode, out var existingDepth) || currentDepth > existingDepth)
                    {
                        nodeDepths[currentNode] = currentDepth;
                        nodeIndices[currentNode] = i;
                    }

                    // Enqueue all next nodes of the current node with increased depth
                    foreach (var nextNode in currentNode.nextNodes)
                    {
                        queue.Enqueue(nextNode);
                    }
                }

                // Increment the depth for the next level
                currentDepth++;
            }
        }

        public Vector2 getPos(TNodeData data)
        {
            if (dataToNodeLookup.TryGetValue(data, out var node))
            {
                int depth = nodeDepths.TryGetValue(node, out var nodeDepth) ? nodeDepth : 0;
                int index = nodeIndices.TryGetValue(node, out var nodeIndex) ? nodeIndex : 0;

                return new Vector2(depth * 150, index * 150);
            }
            return Vector2.zero;
        }
    }
}


