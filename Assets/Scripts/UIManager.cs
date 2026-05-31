using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Updates HUD UI elements in the Play scene: score, visual lives, and Game Over panel.
/// Reads from GameManager and refreshes every frame.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD References")]
    public Text scoreText;
    public TMP_Text scoreTextTMP; // Fallback for TMP
    public Text timeText;
    public TMP_Text timeTextTMP;
    
    [Header("Lives References")]
    // Icons should be assigned in order (e.g. Life 1, Life 2, Life 3)
    public Image[] lifeIcons; 
    
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public TMP_Text finalScoreTextTMP; // Fallback for TMP
    public Text finalTimeText;
    public TMP_Text finalTimeTextTMP;

    private int lastScore = -1;
    private int lastLives = -1;
    private int lastTimeSeconds = -1;

    private void OnEnable()
    {
        GameManager.OnGameOverEvent += ShowGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOverEvent -= ShowGameOver;
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            
            // Dynamically add UIButtonAnimation to any Button under the GameOver Panel
            Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.gameObject.GetComponent<UIButtonAnimation>() == null)
                {
                    btn.gameObject.AddComponent<UIButtonAnimation>();
                }
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        int score = GameManager.Instance.Score;
        int lives = GameManager.Instance.Lives;
        int timeSeconds = Mathf.FloorToInt(GameManager.Instance.PlayTime);

        if (score != lastScore)
        {
            if (scoreText != null) scoreText.text = score.ToString();
            if (scoreTextTMP != null) scoreTextTMP.text = score.ToString();
            lastScore = score;
        }

        if (timeSeconds != lastTimeSeconds)
        {
            int minutes = timeSeconds / 60;
            int seconds = timeSeconds % 60;
            string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            if (timeText != null) timeText.text = timeString;
            if (timeTextTMP != null) timeTextTMP.text = timeString;
            lastTimeSeconds = timeSeconds;
        }

        if (lives != lastLives)
        {
            UpdateLivesDisplay(lives);
            lastLives = lives;
        }
    }

    private void UpdateLivesDisplay(int currentLives)
    {
        if (lifeIcons == null || lifeIcons.Length == 0) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                // Show icon if we have enough lives, otherwise hide
                lifeIcons[i].enabled = (i < currentLives);
            }
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            int finalScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE: {finalScore}";
            if (finalScoreTextTMP != null) finalScoreTextTMP.text = $"FINAL SCORE: {finalScore}";

            if (GameManager.Instance != null)
            {
                int timeSeconds = Mathf.FloorToInt(GameManager.Instance.PlayTime);
                int minutes = timeSeconds / 60;
                int seconds = timeSeconds % 60;
                string timeString = string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
                if (finalTimeText != null) finalTimeText.text = timeString;
                if (finalTimeTextTMP != null) finalTimeTextTMP.text = timeString;
            }
        }

        // Hide HUD elements
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (scoreTextTMP != null) scoreTextTMP.gameObject.SetActive(false);
        if (timeText != null) timeText.gameObject.SetActive(false);
        if (timeTextTMP != null) timeTextTMP.gameObject.SetActive(false);
        if (lifeIcons != null)
        {
            foreach (var icon in lifeIcons)
            {
                if (icon != null) icon.gameObject.SetActive(false);
            }
        }
    }
    
    // UI Button callbacks
    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }
    
    public void OnMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMenu();
    }
}
