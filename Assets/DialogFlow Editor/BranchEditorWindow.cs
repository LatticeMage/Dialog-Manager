using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using Newtonsoft.Json.Linq;

public class BranchEditorWindow : EditorWindow
{
    private string filePath;
    private JsonNodesHandler nodesHandler = new JsonNodesHandler();

    [MenuItem("Tools/Branch Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<BranchEditorWindow>("Branch Editor");
        window.minSize = new Vector2(400, 300);
    }

    public void OnEnable()
    {
        // Load the VisualTreeAsset
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DialogFlow Editor/BranchEditorTemplate.uxml");
        visualTree.CloneTree(rootVisualElement);

        // Load the stylesheet
        var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/DialogFlow Editor/BranchEditorStyles.uss");
        rootVisualElement.styleSheets.Add(styles);

        // Hook up the filePath field
        var filePathField = rootVisualElement.Q<TextField>("filePathField");
        filePath = EditorPrefs.GetString("SavedFilePath", "Assets/DialogFlow Editor/");
        filePathField.value = filePath;

        filePathField.RegisterValueChangedCallback(e =>
        {
            filePath = e.newValue;
            EditorPrefs.SetString("SavedFilePath", filePath);
        });

        // Hook up the increment button click event
        var incrementButton = rootVisualElement.Q<Button>("increment");
        incrementButton.clicked += OnIncrementButtonClick;

        // Load the text content from the file and set it to the label, if a filePath exists
        UpdateContentLabel();
    }

    private void UpdateContentLabel()
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            string textContent = File.ReadAllText(filePath);
            var contentLabel = rootVisualElement.Q<Label>("contentLabel");
            contentLabel.text = textContent;
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
    }

    private void OnIncrementButtonClick()
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            nodesHandler.LoadRecursive(filePath); // This will recursively load all nodes

            if (nodesHandler.Nodes.ContainsKey(filePath))
            {
                // Increment only the root json
                JsonNode rootNode = nodesHandler.Nodes[filePath];
                rootNode.Number += 1;

                // Save the modified root node
                rootNode.Save();
                UpdateContentLabel(); // Update the label with the new value
            }
            else
            {
                Debug.LogError($"Could not find node for file: {filePath}");
            }
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
    }

}
