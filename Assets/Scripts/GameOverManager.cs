using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles score reveal animation on the Game Over screen:
/// counts up from 0 to the final score over time.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    public int displayedFinalScore = 0;
    public int targetFinalScore = 0;
    public float scoreRevealDelay = 1.5f;

    private float timer = 0f;
    private bool started = false;

    private void Start()
    {
        if (GameManager.Instance != null)
            targetFinalScore = GameManager.Instance.Score;

        displayedFinalScore = 0;
    }

    private void Update()
    {
        if (!started)
        {
            timer += Time.deltaTime;
            if (timer >= scoreRevealDelay)
            {
                started = true;
                timer = 0f;
            }
            return;
        }

        int display = Mathf.RoundToInt(Mathf.Lerp(0f, targetFinalScore, timer * 3f));
        displayedFinalScore = Mathf.Min(display, targetFinalScore);

        if (displayedFinalScore >= targetFinalScore)
        {
            displayedFinalScore = targetFinalScore;
            enabled = false; // stop once done
        }
    }
}
