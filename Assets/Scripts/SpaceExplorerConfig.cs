using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Auto-configures the project for Space Explorer game.
/// Run via Component menu or Window > Space Explorer > Configure Project
/// </summary>
public class SpaceExplorerConfig : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Window/Space Explorer/Configure Project")]
    public static void ConfigureProject()
    {
        // Configure Player Settings for compatibility
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

        Debug.Log("[SpaceExplorer] Project configured!");

        EditorUtility.DisplayDialog("Configuration Complete",
            "Project has been configured for Space Explorer.\n\n" +
            "Make sure in Player Settings:\n" +
            "- Active Input Handling = 'Both'\n\n" +
            "You may need to restart Unity for changes to take effect.",
            "OK");
    }

    [MenuItem("Window/Space Explorer/Fix Input System Issues")]
    public static void FixInputSystem()
    {
        // Check if InputSystem package is installed
        bool hasInputSystem = false;
        string[] guids = AssetDatabase.FindAssets("InputSystem");
        if (guids.Length > 0)
        {
            hasInputSystem = true;
        }

        Debug.Log($"[SpaceExplorer] Input System installed: {hasInputSystem}");

        if (!hasInputSystem)
        {
            Debug.Log("[SpaceExplorer] No Input System found - using legacy input");
        }
        else
        {
            Debug.Log("[SpaceExplorer] Input System detected - make sure Active Input Handling is set to 'Both'");
        }

        EditorUtility.DisplayDialog("Input System Check",
            hasInputSystem
                ? "Input System is installed.\n\nMake sure in Player Settings:\n- Active Input Handling = 'Both'\n\nThis allows both old and new input to work."
                : "Input System package not found.\n\nUsing legacy input system.",
            "OK");
    }
#endif
}
