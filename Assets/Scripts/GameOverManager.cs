using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Game Over scene: displays final score and handles navigation.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int displayedFinalScore = 0;
    [SerializeField] private int targetFinalScore = 0;
    [SerializeField] private float scoreRevealDelay = 1.5f;

    private Text finalScoreText;
    private float revealTimer = 0f;
    private bool hasRevealedScore = false;

    private void Start()
    {
        // Find UI elements
        Text[] texts = FindObjectsOfType<Text>();
        foreach (Text t in texts)
        {
            if (t.gameObject.name == "FinalScoreText")
            {
                finalScoreText = t;
            }
        }

        // Get score from GameManager
        if (GameManager.Instance != null)
        {
            targetFinalScore = GameManager.Instance.CurrentScore;
        }

        // Setup button listeners
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button btn in allButtons)
        {
            string btnName = btn.gameObject.name;

            if (btnName == "RestartButton")
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

    private void Update()
    {
        if (!hasRevealedScore)
        {
            revealTimer += Time.deltaTime;
            if (revealTimer >= scoreRevealDelay)
            {
                hasRevealedScore = true;
                if (finalScoreText != null)
                {
                    finalScoreText.text = $"FINAL SCORE: {targetFinalScore}";
                }
            }
        }
    }

    /// <summary>
    /// Called when Restart button is clicked.
    /// </summary>
    public void OnRestartClicked()
    {
        Debug.Log("[GameOver] Restarting game...");
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Called when Main Menu button is clicked.
    /// </summary>
    public void OnMainMenuClicked()
    {
        Debug.Log("[GameOver] Returning to menu...");
        SceneManager.LoadScene("MainMenu");
    }
}
