using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Enhanced HUD Controller displaying:
/// - Score with multiplier
/// - Combo counter
/// - Streak counter
/// - Active power-ups with timers
/// - High score
/// </summary>
public class HUDController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // UI References
    // -------------------------------------------------------------------------

    [Header("Core UI")]
    public Text scoreText;
    public Text highScoreText;

    [Header("Combo & Streak")]
    public Text comboText;
    public Text streakText;
    public Image comboBarFill;
    public Image streakBarFill;

    [Header("Power-Up Indicators")]
    public Image[] powerUpIndicators;
    public Text[] powerUpTimerTexts;
    public Image[] powerUpTimerBars;

    [Header("Settings")]
    public float comboBarMaxTime = 3f;
    public float streakBarMaxStreak = 5f;

    [Header("Visual Settings")]
    public Color comboActiveColor = Color.yellow;
    public Color comboInactiveColor = Color.gray;
    public Color streakActiveColor = Color.cyan;
    public Color streakInactiveColor = Color.gray;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int _lastKnownScore = 0;
    private int _lastKnownCombo = 0;
    private int _lastKnownStreak = 0;
    private float _lastKnownMultiplier = 1f;
    private float _lastKnownComboTimer = 0f;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        UpdateAllDisplays();
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void Initialize()
    {
        // Subscribe to ScoreManager events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            ScoreManager.Instance.OnComboChanged += OnComboChanged;
            ScoreManager.Instance.OnStreakChanged += OnStreakChanged;
            ScoreManager.Instance.OnMultiplierChanged += OnMultiplierChanged;

            ScoreManager.Instance.ResetScore();
        }

        // Initialize power-up indicator visuals
        InitializePowerUpIndicators();
    }

    private void InitializePowerUpIndicators()
    {
        if (powerUpIndicators == null) return;

        for (int i = 0; i < powerUpIndicators.Length; i++)
        {
            if (powerUpIndicators[i] != null)
            {
                powerUpIndicators[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
            ScoreManager.Instance.OnComboChanged -= OnComboChanged;
            ScoreManager.Instance.OnStreakChanged -= OnStreakChanged;
            ScoreManager.Instance.OnMultiplierChanged -= OnMultiplierChanged;
        }
    }

    // -------------------------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------------------------

    private void OnScoreChanged(int newScore)
    {
        _lastKnownScore = newScore;
    }

    private void OnComboChanged(int newCombo)
    {
        _lastKnownCombo = newCombo;
    }

    private void OnStreakChanged(int newStreak)
    {
        _lastKnownStreak = newStreak;
    }

    private void OnMultiplierChanged(float newMultiplier)
    {
        _lastKnownMultiplier = newMultiplier;
    }

    // -------------------------------------------------------------------------
    // Update Displays
    // -------------------------------------------------------------------------

    private void UpdateAllDisplays()
    {
        UpdateScoreDisplay();
        UpdateComboDisplay();
        UpdateStreakDisplay();
        UpdatePowerUpIndicators();
    }

    private void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance == null) return;

        int currentScore = ScoreManager.Instance.Score;
        int highScore = ScoreManager.Instance.HighScore;
        float multiplier = ScoreManager.Instance.CurrentMultiplier;

        // Update score text
        if (scoreText != null)
        {
            string multiplierText = multiplier > 1f ? $" <color=yellow>x{multiplier:F1}</color>" : "";
            scoreText.text = $"Score: {currentScore}{multiplierText}";
        }

        // Update high score
        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {highScore}";
        }
    }

    private void UpdateComboDisplay()
    {
        if (ScoreManager.Instance == null) return;

        int combo = ScoreManager.Instance.ComboCount;
        float remainingTime = 0f;

        if (PowerUpManager.Instance != null)
        {
            // Combo timer is handled by ScoreManager internally
            remainingTime = ScoreManager.Instance.comboExpireTime;
        }

        // Update combo text
        if (comboText != null)
        {
            if (combo >= 2)
            {
                comboText.text = $"COMBO x{combo}";
                comboText.color = comboActiveColor;
                comboText.gameObject.SetActive(true);

                // Animate combo text
                float scale = 1f + Mathf.Sin(Time.time * 10f) * 0.1f;
                comboText.transform.localScale = Vector3.one * scale;
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        // Update combo bar
        if (comboBarFill != null)
        {
            if (combo >= 2)
            {
                float progress = remainingTime / comboBarMaxTime;
                comboBarFill.fillAmount = Mathf.Clamp01(progress);
                comboBarFill.color = Color.Lerp(Color.red, Color.green, progress);
            }
            else
            {
                comboBarFill.fillAmount = 0;
            }
        }
    }

    private void UpdateStreakDisplay()
    {
        if (ScoreManager.Instance == null) return;

        int streak = ScoreManager.Instance.StreakCount;
        int threshold = ScoreManager.Instance.streakThresholdForBonus;

        // Update streak text
        if (streakText != null)
        {
            if (streak >= threshold)
            {
                int bonusLevel = streak / threshold;
                streakText.text = $"STREAK x{bonusLevel}!";
                streakText.color = streakActiveColor;
                streakText.gameObject.SetActive(true);

                // Pulse effect for high streaks
                if (bonusLevel >= 2)
                {
                    float alpha = Mathf.Sin(Time.time * 8f) * 0.5f + 0.5f;
                    Color c = streakText.color;
                    c.a = alpha;
                    streakText.color = c;
                }
            }
            else
            {
                streakText.text = $"Streak: {streak}/{threshold}";
                streakText.color = streakInactiveColor;
                streakText.gameObject.SetActive(true);
            }
        }

        // Update streak bar
        if (streakBarFill != null)
        {
            float progress = (float)streak / threshold;
            streakBarFill.fillAmount = Mathf.Clamp01(progress);
            streakBarFill.color = Color.Lerp(streakInactiveColor, streakActiveColor, progress);
        }
    }

    private void UpdatePowerUpIndicators()
    {
        if (PowerUpManager.Instance == null) return;
        if (powerUpIndicators == null || powerUpIndicators.Length == 0) return;

        // Map power-up types to indicator slots
        PowerUpType[] powerUpTypes = new PowerUpType[]
        {
            PowerUpType.Shield,
            PowerUpType.SpeedBoost,
            PowerUpType.TripleShot,
            PowerUpType.RapidFire,
            PowerUpType.PiercingShot,
            PowerUpType.ScoreBonus
        };

        for (int i = 0; i < powerUpTypes.Length && i < powerUpIndicators.Length; i++)
        {
            PowerUpType type = powerUpTypes[i];
            bool isActive = PowerUpManager.Instance.IsPowerUpActive(type);
            float remainingTime = PowerUpManager.Instance.GetPowerUpRemainingTime(type);

            // Update indicator visibility
            if (powerUpIndicators[i] != null)
            {
                powerUpIndicators[i].gameObject.SetActive(isActive);
            }

            // Update timer text
            if (powerUpTimerTexts != null && i < powerUpTimerTexts.Length && powerUpTimerTexts[i] != null)
            {
                if (isActive)
                {
                    powerUpTimerTexts[i].text = $"{remainingTime:F1}s";
                }
                else
                {
                    powerUpTimerTexts[i].text = "";
                }
            }

            // Update timer bar
            if (powerUpTimerBars != null && i < powerUpTimerBars.Length && powerUpTimerBars[i] != null)
            {
                if (isActive)
                {
                    float progress = 1f - PowerUpManager.Instance.GetPowerUpProgress(type);
                    powerUpTimerBars[i].fillAmount = progress;
                }
                else
                {
                    powerUpTimerBars[i].fillAmount = 0;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Utility Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Show a floating text notification
    /// </summary>
    public void ShowFloatingText(string text, Vector3 position, Color color)
    {
        // Can be implemented with a floating text prefab
        Debug.Log($"[Floating] {text}");
    }

    /// <summary>
    /// Flash the score display (e.g., when bonus points are earned)
    /// </summary>
    public void FlashScore()
    {
        if (scoreText == null) return;

        StartCoroutine(FlashRoutine(scoreText));
    }

    private System.Collections.IEnumerator FlashRoutine(Text text)
    {
        Color originalColor = text.color;
        Color flashColor = Color.yellow;

        text.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        text.color = originalColor;
    }

    /// <summary>
    /// Show milestone notification (e.g., "NEW HIGH SCORE!")
    /// </summary>
    public void ShowMilestoneNotification(string message)
    {
        // Can be implemented with a notification UI
        Debug.Log($"[Milestone] {message}");
    }
}
