using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages gameplay UI elements: score display, lives counter, and ship indicator.
/// Updates in real-time as the player collects stars or gets hit by asteroids.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text shipNameText;
    [SerializeField] private Image[] lifeIcons;

    [Header("UI Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    private int displayedScore = 0;
    private string currentShipName = "Fighter";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeUI();
    }

    private void Update()
    {
        // Animate score counting up
        AnimateScore();
    }

    /// <summary>
    /// Initializes UI with starting values.
    /// </summary>
    private void InitializeUI()
    {
        displayedScore = 0;
        UpdateScore(0);
        UpdateLives(3);
        UpdateShipName("Fighter");
    }

    /// <summary>
    /// Updates the score display with animation.
    /// </summary>
    public void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {newScore}";
    }

    /// <summary>
    /// Updates the lives display with visual feedback.
    /// </summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"LIVES: {lives}";

            // Change color based on remaining lives
            if (lives <= 1)
                livesText.color = dangerColor;
            else if (lives <= 2)
                livesText.color = warningColor;
            else
                livesText.color = normalColor;
        }

        // Update life icons
        UpdateLifeIcons(lives);
    }

    /// <summary>
    /// Updates the ship name display.
    /// </summary>
    public void UpdateShipName(string shipName)
    {
        currentShipName = shipName;
        if (shipNameText != null)
        {
            shipNameText.text = $">{shipName}<";
            shipNameText.color = GetShipColor(shipName);
        }
    }

    /// <summary>
    /// Gets the color associated with a ship type.
    /// </summary>
    private Color GetShipColor(string shipName)
    {
        switch (shipName.ToLower())
        {
            case "fighter": return Color.cyan;
            case "speeder": return Color.green;
            case "tank": return Color.yellow;
            case "sniper": return new Color(0.6f, 0.2f, 1f);
            case "bomber": return Color.red;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Updates the life icon array to show remaining lives.
    /// </summary>
    private void UpdateLifeIcons(int lives)
    {
        if (lifeIcons == null || lifeIcons.Length == 0) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].enabled = i < lives;
        }
    }

    /// <summary>
    /// Animates the score counting up to the current score.
    /// </summary>
    private void AnimateScore()
    {
        if (GameManager.Instance == null) return;

        int targetScore = GameManager.Instance.CurrentScore;
        if (displayedScore < targetScore)
        {
            displayedScore += Mathf.CeilToInt((targetScore - displayedScore) * Time.deltaTime * 5);
            if (displayedScore > targetScore)
                displayedScore = targetScore;

            if (scoreText != null)
                scoreText.text = $"SCORE: {displayedScore}";
        }
    }

    /// <summary>
    /// Shows a wave/start message.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (waveText != null)
        {
            waveText.text = message;
            waveText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the message display.
    /// </summary>
    public void HideMessage()
    {
        if (waveText != null)
            waveText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows game over effect.
    /// </summary>
    public void ShowGameOverEffect()
    {
        if (shipNameText != null)
        {
            shipNameText.text = "";
        }
    }
}
