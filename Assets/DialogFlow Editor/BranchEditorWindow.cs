using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;

public class BranchEditorWindow : EditorWindow
{
    private string filePath;
    private JsonNodesHandler nodesHandler = new JsonNodesHandler();
    private JsonGraphView graphView;

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

        graphView = new JsonGraphView();
        graphView.StretchToParentSize();
        var jsonGraphVisualElement = rootVisualElement.Q<VisualElement>("JsonGraph");
        jsonGraphVisualElement.Add(graphView);

        // Hook up the filePath field
        var filePathField = rootVisualElement.Q<TextField>("filePathField");
        filePath = EditorPrefs.GetString("SavedFilePath", "Assets/DialogFlow Editor/");
        filePathField.value = filePath;

        filePathField.RegisterValueChangedCallback(e =>
        {
            filePath = e.newValue;
            EditorPrefs.SetString("SavedFilePath", filePath);
        });

        // Hook up the buttons
        var loadButton = rootVisualElement.Q<Button>("load");
        loadButton.clicked += OnLoadButtonClick;

        var incrementButton = rootVisualElement.Q<Button>("increment");
        incrementButton.clicked += OnIncrementButtonClick;

        var saveButton = rootVisualElement.Q<Button>("save");
        saveButton.clicked += OnSaveButtonClick;

        UpdateContentLabel();
    }

    private void UpdateContentLabel()
    {
        if (!string.IsNullOrEmpty(filePath) && nodesHandler.Nodes.ContainsKey(filePath))
        {
            JsonNode rootNode = nodesHandler.Nodes[filePath];
            var contentLabel = rootVisualElement.Q<Label>("contentLabel");
            contentLabel.text = rootNode.ToString();
        }
        else
        {
            Debug.LogError($"File not found or not loaded: {filePath}");
        }
    }

    private void OnLoadButtonClick()
    {
        nodesHandler.Clear();

        if (!string.IsNullOrEmpty(filePath))
        {
            nodesHandler.LoadRecursive(filePath);
            graphView.PopulateGraph(nodesHandler);
            UpdateContentLabel();
        }
        else
        {
            Debug.LogError($"File path not specified.");
        }
    }

    private void OnIncrementButtonClick()
    {
        if (nodesHandler.Nodes.ContainsKey(filePath))
        {
            JsonNode rootNode = nodesHandler.Nodes[filePath];
            rootNode.Number += 1;
            UpdateContentLabel();
        }
        else
        {
            Debug.LogError($"Could not find node for file: {filePath}");
        }
    }

    private void OnSaveButtonClick()
    {
        nodesHandler.SaveAll(); // Save all modified nodes
    }
}
