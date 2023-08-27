using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class BranchEditorWindow : EditorWindow
{
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

        // Load the text content from the file
        string textContent = System.IO.File.ReadAllText("Assets/DialogFlow Editor/test.txt");

        // Find the label and set its text content
        var contentLabel = rootVisualElement.Q<Label>("contentLabel");
        contentLabel.text = textContent;
    }

    public void OnGUI()
    {
        // Traditional IMGUI code can be placed here if needed
    }
}
