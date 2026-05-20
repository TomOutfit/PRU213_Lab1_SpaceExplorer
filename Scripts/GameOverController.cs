using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the UI in the Game Over scene, displaying the final score,
/// coins earned, and handling button clicks.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    public Text finalScoreText;
    public Text highScoreText;
    public Text coinsEarnedText;
    public Text statsText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button shipSelectionButton;

    [Header("Visual Settings")]
    public Color newHighScoreColor = Color.yellow;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        InitializeUI();
        DisplayResults();
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void InitializeUI()
    {
        // Button click sounds are handled by GameManager
    }

    private void DisplayResults()
    {
        // Display final score
        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            finalScoreText.text = $"Final Score: {ScoreManager.Instance.Score}";
        }

        // Display and check high score
        if (highScoreText != null && ScoreManager.Instance != null)
        {
            int highScore = ScoreManager.Instance.HighScore;
            highScoreText.text = $"Best: {highScore}";

            // Check if new high score
            if (ScoreManager.Instance.Score >= highScore && ScoreManager.Instance.Score > 0)
            {
                highScoreText.color = newHighScoreColor;
                highScoreText.text = "NEW HIGH SCORE!";
            }
        }

        // Calculate and display coins earned
        if (coinsEarnedText != null && ScoreManager.Instance != null)
        {
            int coinsEarned = Mathf.Max(1, ScoreManager.Instance.Score / 10);
            coinsEarnedText.text = $"+{coinsEarned} Coins Earned!";

            // Add coins to player's total
            if (ShipSelectionController.Instance != null)
            {
                ShipSelectionController.Instance.AddCoins(coinsEarned);
            }
        }

        // Display game stats
        if (statsText != null && ScoreManager.Instance != null)
        {
            statsText.text = $"Asteroids: {ScoreManager.Instance.TotalAsteroidsDestroyed}\n" +
                           $"Stars: {ScoreManager.Instance.TotalStarsCollected}\n" +
                           $"Enemies: {ScoreManager.Instance.TotalEnemiesDestroyed}\n" +
                           $"Best Streak: {ScoreManager.Instance.StreakCount}";
        }
    }

    // -------------------------------------------------------------------------
    // Button Handlers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when Restart button is clicked.
    /// </summary>
    public void OnRestartButtonClicked()
    {
        PlayButtonSound();

        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Load game scene
        LoadScene("GamePlayScene");
    }

    /// <summary>
    /// Called when Main Menu button is clicked.
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        PlayButtonSound();
        LoadScene("MainMenuScreen");
    }

    /// <summary>
    /// Called when Ship Selection button is clicked.
    /// </summary>
    public void OnShipSelectionButtonClicked()
    {
        PlayButtonSound();
        LoadScene("MainMenuScreen");

        // After main menu loads, ship selection will be shown
        // This can be enhanced with a delayed call if needed
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
}
