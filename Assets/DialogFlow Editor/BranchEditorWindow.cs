
/*
 * BranchEditorWindow.cs
 * ---------------------
 * Provides a custom Unity Editor window for managing branches in the dialog system.
 * 
 * Key Components:
 * - ShowWindow: Initializes the editor window with a minimum size.
 * - OnEnable: Sets up the UI layout, styles, and events for the window.
 * - Button Actions: Functions to handle user interactions like loading JSON data, incrementing node values, and saving.
 * 
 * Usage:
 * - Use the Unity Editor menu to open the Branch Editor window.
 * - Specify a JSON file path to visualize and manage dialog branches.
 */
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using Dialog.Graph;
using Dialog.Json;

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

        // Create the JsonGraphView and add it to the ScrollView named "JsonGraph"
        graphView = new JsonGraphView();
        graphView.StretchToParentSize();
        
        // Reference the ScrollView and ensure it stretches to its parent size
        var jsonGraphScrollView = rootVisualElement.Q<ScrollView>("JsonGraph");
        jsonGraphScrollView.StretchToParentSize();
        jsonGraphScrollView.contentContainer.StretchToParentSize();
        jsonGraphScrollView.Add(graphView);
    
    

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

    }

    private void UpdateContentLabel()
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);  // Extract filename

        if (!string.IsNullOrEmpty(fileName) && nodesHandler.Nodes.ContainsKey(fileName)) // Use filename for the lookup
        {
            JsonNode rootNode = nodesHandler.Nodes[fileName];
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
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (nodesHandler.Nodes.ContainsKey(fileName))
        {
            JsonNode rootNode = nodesHandler.Nodes[fileName];
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
