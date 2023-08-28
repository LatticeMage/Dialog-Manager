using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using Newtonsoft.Json.Linq;

public class BranchEditorWindow : EditorWindow
{
    private string filePath;

    // Static variable to hold the JSON content in memory
    private static string jsonInMemory = null;

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

        // Clone the VisualTreeAsset into the window's root
        visualTree.CloneTree(rootVisualElement);

        // Load the stylesheet
        var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/DialogFlow Editor/BranchEditorStyles.uss");
        rootVisualElement.styleSheets.Add(styles);

        // Hook up the filePath field
        var filePathField = rootVisualElement.Q<TextField>("filePathField");
        filePathField.RegisterValueChangedCallback(e => { filePath = e.newValue; });

        // Hook up the increment button click event
        var incrementButton = rootVisualElement.Q<Button>("increment");
        incrementButton.clicked += OnIncrementButtonClick;

        // Retrieve the saved path from EditorPrefs and set it as the default value
        filePath = EditorPrefs.GetString("SavedFilePath", "Assets/DialogFlow Editor/");
        filePathField.value = filePath;

        filePathField.RegisterValueChangedCallback(e =>
        {
            filePath = e.newValue;
            // Save the current filePath to EditorPrefs whenever it changes
            EditorPrefs.SetString("SavedFilePath", filePath);
        });

        UpdateContentLabel();
    }

    private void UpdateContentLabel()
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            // If jsonInMemory is null, read the file from disk
            if (jsonInMemory == null)
            {
                jsonInMemory = File.ReadAllText(filePath);
            }

            var contentLabel = rootVisualElement.Q<Label>("contentLabel");
            contentLabel.text = jsonInMemory;
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
            // Use the content in memory
            JObject jsonObj = JObject.Parse(jsonInMemory);

            int currentNumber = jsonObj["number"].Value<int>();
            jsonObj["number"] = currentNumber + 1;

            // Update the in-memory content and write it to disk
            jsonInMemory = jsonObj.ToString();
            File.WriteAllText(filePath, jsonInMemory);

            UpdateContentLabel();
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
    }

    public void OnGUI()
    {
        // Traditional IMGUI code can be placed here if needed
    }
}
