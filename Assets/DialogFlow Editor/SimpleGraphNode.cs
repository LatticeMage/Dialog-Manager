using UnityEditor.Experimental.GraphView; // Added this for access to the Port class
using System.Collections.Generic;
using UnityEngine;


namespace Dialog.Graph
{
    public interface ISimpleGraphNode<T>
    {
        List<Port> InputSlots { get; }
        List<Port> OutputSlots { get; }
        T Data { get; }
    }


    public class SimpleGraphNode<TNodeData> : ISimpleGraphNode<TNodeData>
    {
        public List<Port> InputSlots { get; private set; }
        public List<Port> OutputSlots { get; private set; }
        public TNodeData Data { get; private set; }

        public SimpleGraphNode(TNodeData data)
        {
            InputSlots = new List<Port>();
            OutputSlots = new List<Port>();
            Data = data;
        }
    }





    public class SimpleGraphPositions<TNodeData>
    {
        private HashSet<ISimpleGraphNode<TNodeData>> nodes;
        private Dictionary<TNodeData, ISimpleGraphNode<TNodeData>> dataToNodeLookup;
        private Dictionary<ISimpleGraphNode<TNodeData>, Vector2> nodePositions;

        public SimpleGraphPositions()
        {
            nodes = new HashSet<ISimpleGraphNode<TNodeData>>();
            nodePositions = new Dictionary<ISimpleGraphNode<TNodeData>, Vector2>();
            dataToNodeLookup = new Dictionary<TNodeData, ISimpleGraphNode<TNodeData>>();
        }

        public void AddNode(ISimpleGraphNode<TNodeData> node)
        {
            nodes.Add(node);
            dataToNodeLookup[node.Data] = node;
        }

        public void calPos()
        {
            int xOffset = 150;
            int xPosition = 0;
            foreach (var node in nodes)
            {
                nodePositions[node] = new Vector2(xPosition, 100);
                xPosition += xOffset;
            }
        }

        public Vector2 getPos(TNodeData data)
        {
            if (dataToNodeLookup.TryGetValue(data, out var node) &&
                nodePositions.TryGetValue(node, out var position))
            {
                return position;
            }
            return Vector2.zero;
        }
    }
}


