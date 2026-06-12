using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Game Over scene: displays the final score and
/// handles Return-to-Menu and Quit button clicks.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalScoreLabel;
    public Button returnToMenuButton;
    public Button playAgainButton;

    [Header("Score Message Thresholds")]
    public string excellentMessage = "EXCELLENT!";
    public string goodMessage = "GOOD JOB!";
    public string averageMessage = "NICE TRY!";
    public int excellentThreshold = 100;
    public int goodThreshold = 50;

    private void Start()
    {
        DisplayScore();
        WireButtons();
    }

    private void DisplayScore()
    {
        int score = 0;
        if (GameManager.Instance != null)
            score = GameManager.Instance.Score;

        if (finalScoreText != null)
            finalScoreText.text = score.ToString();

        if (finalScoreLabel != null)
        {
            if (score >= excellentThreshold)
                finalScoreLabel.text = excellentMessage;
            else if (score >= goodThreshold)
                finalScoreLabel.text = goodMessage;
            else
                finalScoreLabel.text = averageMessage;
        }
    }

    private void WireButtons()
    {
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
    }

    public void OnReturnToMenuClicked()
    {
        Debug.Log("[GameOverController] Return to Menu clicked.");
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void OnPlayAgainClicked()
    {
        Debug.Log("[GameOverController] Play Again clicked.");
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
    }

    private void OnDestroy()
    {
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
        if (playAgainButton != null)
            playAgainButton.onClick.RemoveListener(OnPlayAgainClicked);
    }
}
