using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu scene: handles Play, Instructions, and Quit button clicks,
/// and manages the visibility of the Instructions panel.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button instructionsButton;
    public Button closeInstructionsButton;
    public Button quitButton;
    public GameObject instructionsPanel;

    [Header("Ship Selection (Optional Visual Variety)")]
    public Sprite[] menuBackgroundShipSprites;

    private void Start()
    {
        // Hide instructions panel at start
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        // Wire up button listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(OnInstructionsClicked);

        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.AddListener(OnCloseInstructionsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
            
        // Add UI Button Animation dynamically to all buttons
        Button[] allButtons = { playButton, instructionsButton, closeInstructionsButton, quitButton };
        foreach (Button btn in allButtons)
        {
            if (btn != null && btn.gameObject.GetComponent<UIButtonAnimation>() == null)
            {
                btn.gameObject.AddComponent<UIButtonAnimation>();
            }
        }
    }

    public void OnPlayClicked()
    {
        Debug.Log("[MenuController] Play clicked — loading Play scene.");
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
    }

    public void OnInstructionsClicked()
    {
        Debug.Log("[MenuController] Instructions panel opened.");
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    public void OnCloseInstructionsClicked()
    {
        Debug.Log("[MenuController] Instructions panel closed.");
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Debug.Log("[MenuController] Quit clicked.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
        if (instructionsButton != null)
            instructionsButton.onClick.RemoveListener(OnInstructionsClicked);
        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.RemoveListener(OnCloseInstructionsClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
}
