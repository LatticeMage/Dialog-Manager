using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;

public class BranchEditorWindow : EditorWindow
{
    private string filePath = "Assets/DialogFlow Editor/test.txt";

    [MenuItem("Tools/Branch Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<BranchEditorWindow>("Branch Editor");
        window.minSize = new Vector2(400, 300);  // Set minimum window size
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

        // Load the text content from the file and set it to the label
        UpdateContentLabel();

        // Hook up the increment button click event
        var incrementButton = rootVisualElement.Q<Button>("increment");
        incrementButton.clicked += OnIncrementButtonClick;
    }

    private void UpdateContentLabel()
    {
        if (File.Exists(filePath))
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
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            if (int.TryParse(content, out int currentValue))
            {
                currentValue++;
                File.WriteAllText(filePath, currentValue.ToString());
                UpdateContentLabel(); // Update the label with the new value
            }
            else
            {
                Debug.LogError("Failed to parse content to integer.");
            }
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
