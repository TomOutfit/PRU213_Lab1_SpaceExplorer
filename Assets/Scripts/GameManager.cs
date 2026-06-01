using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global singleton that persists across scenes. Manages score, lives,
/// and all scene-level transitions (Menu, Play, GameOver, Quit).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event System.Action OnGameOverEvent;

    [Header("Scoring")]
    public int pointsPerStar = 10;
    public int penaltyPerAsteroid = 5;
    public int pointsPerLaserHit = 5;

    // Requirement: 3 initial lives for the spaceship.
    public int initialLives = 3;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public float PlayTime { get; private set; }
    public bool IsGameActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (SceneTransitionManager.Instance == null)
        {
            GameObject transitionObj = new GameObject("TransitionManager");
            transitionObj.AddComponent<SceneTransitionManager>();
        }
    }

    private void Update()
    {
        if (IsGameActive)
        {
            PlayTime += Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Play")
        {
            ResetGameState();
        }
    }

    private void Start()
    {
        ResetGameState();
    }

    /// <summary>Called by GamePlayManager each time the Play scene loads.</summary>
    public void ResetGameState()
    {
        Score = 0;
        Lives = initialLives;
        PlayTime = 0f;
        IsGameActive = true;
        Time.timeScale = 1f; // Ensure time scale is reset to normal
    }

    public void AddScore(int amount)
    {
        if (!IsGameActive) return;
        Score += amount;
        Debug.Log($"[GameManager] Score +{amount}  Total: {Score}");
    }

    public void LoseLife()
    {
        if (!IsGameActive) return;
        Lives--;
        Debug.Log($"[GameManager] Lost a life! Remaining: {Lives}");

        if (Lives <= 0)
        {
            Lives = 0;
            GameOver();
        }
    }

    public void GameOver()
    {
        if (!IsGameActive) return;
        IsGameActive = false;
        Debug.Log($"[GameManager] Game Over! Final Score: {Score}");
        OnGameOverEvent?.Invoke();
    }

    /// <summary>Load the main menu scene (reached from GameOver screen).</summary>
    public void LoadMenu()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene("Menu");
        else
            SceneManager.LoadScene("Menu");
    }

    /// <summary>Start or restart the gameplay scene.</summary>
    public void StartGame()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene("Play");
        else
            SceneManager.LoadScene("Play");
    }

    /// <summary>Quit the application.</summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] Quit requested.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
