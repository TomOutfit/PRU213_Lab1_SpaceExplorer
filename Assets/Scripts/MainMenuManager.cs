using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Main Menu scene: button interactions and navigation.
/// Provides navigation to gameplay and displays game instructions.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Title Animation")]
    [SerializeField] private Text titleText;
    [SerializeField] private bool animateTitle = true;
    [SerializeField] private float colorCycleSpeed = 0.05f;

    private float titleHue = 0f;

    private void Start()
    {
        // Setup button click listeners
        SetupButtonListeners();

        // Load saved ship selection
        int savedIndex = PlayerPrefs.GetInt("SelectedShip", 0);
        Debug.Log($"[MainMenu] Loaded ship: {savedIndex}");
    }

    private void Update()
    {
        if (animateTitle && titleText != null)
        {
            AnimateTitleColor();
        }

        // Handle Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    /// <summary>
    /// Sets up click listeners for all menu buttons.
    /// </summary>
    private void SetupButtonListeners()
    {
        // Find buttons by name
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button btn in allButtons)
        {
            string btnName = btn.gameObject.name;

            if (btnName == "PlayButton")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnPlayClicked);
            }
            else if (btnName == "InstructionsButton")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnInstructionsClicked);
            }
            else if (btnName == "QuitButton")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnQuitClicked);
            }
            else if (btnName == "RestartButton")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnRestartClicked);
            }
            else if (btnName == "MainMenuButton")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnMainMenuClicked);
            }
        }
    }

    /// <summary>
    /// Called when Play button is clicked. Starts the game.
    /// </summary>
    public void OnPlayClicked()
    {
        PlayClickSound();
        Debug.Log("[MainMenu] Starting game...");

        // Load gameplay scene
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Called when Instructions button is clicked.
    /// </summary>
    public void OnInstructionsClicked()
    {
        PlayClickSound();
        Debug.Log("[MainMenu] Instructions shown");
        // Could show a panel or log instructions
    }

    /// <summary>
    /// Called when Quit button is clicked.
    /// </summary>
    public void OnQuitClicked()
    {
        PlayClickSound();
        Debug.Log("[MainMenu] Quitting...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Called when Restart button is clicked.
    /// </summary>
    public void OnRestartClicked()
    {
        PlayClickSound();
        Debug.Log("[MainMenu] Restarting...");
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Called when Main Menu button is clicked.
    /// </summary>
    public void OnMainMenuClicked()
    {
        PlayClickSound();
        Debug.Log("[MainMenu] Returning to menu...");
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Animates the title text with color cycling.
    /// </summary>
    private void AnimateTitleColor()
    {
        titleHue += Time.deltaTime * colorCycleSpeed;
        if (titleHue > 1f) titleHue = 0f;

        titleText.color = Color.HSVToRGB(titleHue, 0.8f, 1f);
    }

    /// <summary>
    /// Plays button click feedback sound.
    /// </summary>
    private void PlayClickSound()
    {
        AudioManager audio = FindObjectOfType<AudioManager>();
        if (audio != null)
        {
            // Try to play a sound if available
            Debug.Log("[MainMenu] Button clicked");
        }
    }
}
