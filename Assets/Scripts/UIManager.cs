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

    [Header("Combo & Dash UI References")]
    public TMP_Text comboText;
    public TMP_Text grazeNotificationText;
    public TMP_Text dashCooldownText;

    private Coroutine grazeCoroutine;

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
        EnsureUIElements();

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

    private void EnsureUIElements()
    {
        // Try to find if they already exist in the children
        if (comboText == null)
        {
            Transform found = transform.Find("ComboText");
            if (found != null) comboText = found.GetComponent<TMP_Text>();
        }

        // Programmatically create if not assigned
        if (comboText == null)
        {
            GameObject comboObj = new GameObject("ComboText");
            comboObj.transform.SetParent(this.transform, false);
            
            comboText = comboObj.AddComponent<TextMeshProUGUI>();
            comboText.fontSize = 28;
            comboText.fontStyle = FontStyles.Bold;
            comboText.color = new Color(1f, 0.85f, 0f, 1f); // Gold color
            comboText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = comboText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -90);
        }

        if (grazeNotificationText == null)
        {
            Transform found = transform.Find("GrazeText");
            if (found != null) grazeNotificationText = found.GetComponent<TMP_Text>();
        }

        if (grazeNotificationText == null)
        {
            GameObject grazeObj = new GameObject("GrazeText");
            grazeObj.transform.SetParent(this.transform, false);
            
            grazeNotificationText = grazeObj.AddComponent<TextMeshProUGUI>();
            grazeNotificationText.fontSize = 36;
            grazeNotificationText.fontStyle = FontStyles.Bold;
            grazeNotificationText.color = new Color(0f, 1f, 0.5f, 1f); // Neon green
            grazeNotificationText.alignment = TextAlignmentOptions.Center;
            grazeNotificationText.text = "";
            
            RectTransform rect = grazeNotificationText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 100);
            
            grazeNotificationText.gameObject.SetActive(false);
        }

        if (dashCooldownText == null)
        {
            Transform found = transform.Find("DashCooldownText");
            if (found != null) dashCooldownText = found.GetComponent<TMP_Text>();
        }

        if (dashCooldownText == null)
        {
            GameObject dashObj = new GameObject("DashCooldownText");
            dashObj.transform.SetParent(this.transform, false);
            
            dashCooldownText = dashObj.AddComponent<TextMeshProUGUI>();
            dashCooldownText.fontSize = 18;
            dashCooldownText.fontStyle = FontStyles.Bold;
            dashCooldownText.color = Color.white;
            dashCooldownText.alignment = TextAlignmentOptions.Left;
            
            RectTransform rect = dashCooldownText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(20, 20); // bottom left
        }
    }

    public void TriggerGrazeUI()
    {
        if (grazeNotificationText == null) return;
        if (grazeCoroutine != null) StopCoroutine(grazeCoroutine);
        grazeCoroutine = StartCoroutine(GrazeNotificationRoutine());
    }

    private System.Collections.IEnumerator GrazeNotificationRoutine()
    {
        grazeNotificationText.text = "GRAZE!";
        grazeNotificationText.gameObject.SetActive(true);
        grazeNotificationText.color = new Color(0f, 1f, 0.5f, 1f);
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Pulse: start small, get big, and fade out
            float scale = Mathf.Lerp(0.8f, 1.6f, t);
            grazeNotificationText.transform.localScale = new Vector3(scale, scale, 1f);
            
            Color c = grazeNotificationText.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            grazeNotificationText.color = c;
            
            yield return null;
        }
        
        grazeNotificationText.gameObject.SetActive(false);
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

        // Update Combo UI
        if (comboText != null)
        {
            int multiplier = GameManager.Instance.ComboMultiplier;
            float timer = GameManager.Instance.ComboTimer;

            if (multiplier > 1 && GameManager.Instance.IsGameActive)
            {
                comboText.text = $"COMBO x{multiplier}\n{timer:F1}s";
                comboText.gameObject.SetActive(true);

                // Pulse scale
                float scale = 1f + (timer / GameManager.MaxComboTime) * 0.25f + Mathf.PingPong(Time.time * 6f, 0.12f);
                comboText.transform.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        // Update Dash status
        if (dashCooldownText != null && GameManager.Instance.IsGameActive)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    float cdSec = pc.GetDashCooldownSeconds();
                    if (cdSec > 0f)
                    {
                        dashCooldownText.text = $"DASH COOLDOWN: {cdSec:F1}s";
                        dashCooldownText.color = new Color(1f, 0.3f, 0.3f, 1f);
                        dashCooldownText.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        dashCooldownText.text = "DASH READY (SHIFT)";
                        dashCooldownText.color = new Color(0f, 0.8f, 1f, 1f);
                        float pulse = 1f + Mathf.PingPong(Time.time * 2f, 0.08f);
                        dashCooldownText.transform.localScale = new Vector3(pulse, pulse, 1f);
                    }
                }
            }
            else
            {
                dashCooldownText.text = "";
            }
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

        if (comboText != null) comboText.gameObject.SetActive(false);
        if (grazeNotificationText != null) grazeNotificationText.gameObject.SetActive(false);
        if (dashCooldownText != null) dashCooldownText.gameObject.SetActive(false);
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
