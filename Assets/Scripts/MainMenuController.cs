using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the UI and interactions in the Main Menu scene.
/// Integrates with ShipSelectionController for ship selection.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject instructionsPanel;
    public GameObject shipSelectionPanel;
    public GameObject creditsPanel;

    [Header("UI References")]
    public UnityEngine.UI.Text highScoreText;
    public UnityEngine.UI.Text coinsText;

    [Header("Ship Selection")]
    public ShipSelectionController shipSelectionController;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        InitializeUI();
    }

    void Update()
    {
        UpdatePersistentUI();
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void InitializeUI()
    {
        // Hide panels at start
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (shipSelectionPanel != null) shipSelectionPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Initialize ship selection controller
        if (shipSelectionController == null)
        {
            shipSelectionController = FindObjectOfType<ShipSelectionController>();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update high score
        if (highScoreText != null && ScoreManager.Instance != null)
        {
            highScoreText.text = $"High Score: {ScoreManager.Instance.HighScore}";
        }

        // Update coins
        if (coinsText != null && ShipSelectionController.Instance != null)
        {
            coinsText.text = $"Coins: {ShipSelectionController.Instance.Coins}";
        }
    }

    private void UpdatePersistentUI()
    {
        // Continuously update coins display
        if (coinsText != null && ShipSelectionController.Instance != null)
        {
            coinsText.text = $"Coins: {ShipSelectionController.Instance.Coins}";
        }
    }

    // -------------------------------------------------------------------------
    // Button Handlers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the Play button is clicked.
    /// Shows ship selection or starts game directly.
    /// </summary>
    public void OnPlayButtonClicked()
    {
        PlayButtonSound();

        if (shipSelectionPanel != null)
        {
            // Show ship selection panel
            shipSelectionPanel.SetActive(true);
            if (shipSelectionController != null)
            {
                shipSelectionController.ShowShipSelection();
            }
        }
        else
        {
            // Start game directly
            StartGame();
        }
    }

    /// <summary>
    /// Starts the game with selected ship
    /// </summary>
    public void OnConfirmShipButtonClicked()
    {
        PlayButtonSound();

        if (shipSelectionController != null)
        {
            shipSelectionController.ConfirmSelection();
        }
        else
        {
            StartGame();
        }
    }

    /// <summary>
    /// Starts the game directly (for quick start)
    /// </summary>
    public void OnQuickPlayButtonClicked()
    {
        PlayButtonSound();
        StartGame();
    }

    private void StartGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.DeactivateAllPowerUps();
        }

        LoadScene("GamePlayScene");
    }

    /// <summary>
    /// Called when the Instructions button is clicked.
    /// </summary>
    public void OnInstructionsButtonClicked()
    {
        PlayButtonSound();

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(!instructionsPanel.activeSelf);
        }
    }

    /// <summary>
    /// Called when the Ship Selection button is clicked.
    /// </summary>
    public void OnShipSelectionButtonClicked()
    {
        PlayButtonSound();

        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(!shipSelectionPanel.activeSelf);
            if (shipSelectionPanel.activeSelf && shipSelectionController != null)
            {
                shipSelectionController.ShowShipSelection();
            }
        }
    }

    /// <summary>
    /// Called when the Credits button is clicked.
    /// </summary>
    public void OnCreditsButtonClicked()
    {
        PlayButtonSound();

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(!creditsPanel.activeSelf);
        }
    }

    /// <summary>
    /// Called when the Quit button is clicked.
    /// </summary>
    public void OnQuitButtonClicked()
    {
        PlayButtonSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Close ship selection and return to main menu
    /// </summary>
    public void OnCloseShipSelectionClicked()
    {
        PlayButtonSound();

        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(false);
        }
    }

    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    public void OnLeftShipArrowClicked()
    {
        PlayButtonSound();

        if (shipSelectionController != null)
        {
            shipSelectionController.PreviousShip();
        }
    }

    public void OnRightShipArrowClicked()
    {
        PlayButtonSound();

        if (shipSelectionController != null)
        {
            shipSelectionController.NextShip();
        }
    }

    // -------------------------------------------------------------------------
    // Audio
    // -------------------------------------------------------------------------

    private void PlayButtonSound()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayButtonClick();
        }
    }

    // -------------------------------------------------------------------------
    // Scene Loading
    // -------------------------------------------------------------------------

    private void LoadScene(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
