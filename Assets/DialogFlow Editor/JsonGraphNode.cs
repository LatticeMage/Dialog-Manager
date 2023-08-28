using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class JsonGraphNode : Node
{
    public JsonNode JsonNode { get; private set; }

    public JsonGraphNode(JsonNode jsonNode)
    {
        JsonNode = jsonNode;
        title = jsonNode.Id;
        AddToClassList("jsonGraphNode");

        var dialogLabel = new Label { text = jsonNode.Dialog };
        mainContainer.Add(dialogLabel);
    }
}
