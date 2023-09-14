
/*
 * NodeLayoutHandler.cs
 * --------------------
 * Provides functionality to manage the layout of nodes in a graph.
 * 
 * Key Components:
 * - ISimpleGraphNode<T>: A generic interface for graph nodes.
 * - NodeLayoutHandler<TNodeData>: A generic class to handle node layouts.
 * 
 * Usage:
 * - NodeLayoutHandler is responsible for determining node positions within the graph view.
 * - Provides a mechanism to quickly retrieve node position based on its associated data.
 */
using UnityEditor.Experimental.GraphView; // Added this for access to the Port class
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
            Debug.Log("processing: " + node.node_id);

            nodes.Add(node);
            dataToNodeLookup[node.Data] = node;
            Debug.Log("processing: " + dataToNodeLookup.Count);

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
                    Debug.Log("processing: " + currentNode.node_id);

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


