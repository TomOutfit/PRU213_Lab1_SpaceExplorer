using UnityEngine;

/// <summary>
/// Manages global game state, scoring, and scene transitions for Space Explorer.
/// This singleton ensures persistent game data across all scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int initialLives = 3;
    [SerializeField] private int pointsPerStar = 10;
    [SerializeField] private int penaltyPerAsteroid = 5;

    [Header("Spawn Settings")]
    [SerializeField] private int initialAsteroidCount = 5;
    [SerializeField] private int initialStarCount = 8;
    [SerializeField] private float spawnInterval = 5f;

    private int currentScore = 0;
    private int currentLives = 3;
    private bool isGameActive = false;
    private float spawnTimer = 0f;

    public int CurrentScore => currentScore;
    public int CurrentLives => currentLives;
    public bool IsGameActive => isGameActive;
    public int InitialAsteroidCount => initialAsteroidCount;
    public int InitialStarCount => initialStarCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetGame();
    }

    private void Update()
    {
        if (isGameActive)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnObjects();
            }
        }
    }

    /// <summary>
    /// Resets all game variables to initial state for a new game.
    /// </summary>
    public void ResetGame()
    {
        currentScore = 0;
        currentLives = initialLives;
        isGameActive = false;
        spawnTimer = 0f;
    }

    /// <summary>
    /// Starts the gameplay session.
    /// </summary>
    public void StartGame()
    {
        ResetGame();
        isGameActive = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Called when player collects a star. Adds points to the score.
    /// </summary>
    public void CollectStar()
    {
        if (!isGameActive) return;
        currentScore += pointsPerStar;
        AudioManager.Instance?.PlayStarCollect();
        UpdateUI();
    }

    /// <summary>
    /// Called when player collides with an asteroid. Deducts points and reduces lives.
    /// </summary>
    public void HitByAsteroid()
    {
        if (!isGameActive) return;
        currentScore = Mathf.Max(0, currentScore - penaltyPerAsteroid);
        currentLives--;
        AudioManager.Instance?.PlayAsteroidHit();
        UpdateUI();

        if (currentLives <= 0)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Ends the game and transitions to the Game Over scene.
    /// </summary>
    public void EndGame()
    {
        isGameActive = false;
        AudioManager.Instance?.PlayGameOver();
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        ResetGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Spawns new asteroids and stars periodically during gameplay.
    /// </summary>
    private void SpawnObjects()
    {
        if (GamePlayManager.Instance == null) return;

        // Spawn a new asteroid
        GamePlayManager.Instance.SpawnAsteroid();

        // Spawn a new star
        GamePlayManager.Instance.SpawnStar();
    }

    /// <summary>
    /// Updates the gameplay UI with current score and lives.
    /// </summary>
    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateLives(currentLives);
        }
    }

    /// <summary>
    /// Adds score points directly.
    /// </summary>
    public void AddScore(int points)
    {
        if (!isGameActive) return;
        currentScore += points;
        UpdateUI();
    }

    /// <summary>
    /// Sets the player lives directly.
    /// </summary>
    public void SetPlayerLives(int lives)
    {
        currentLives = lives;
        UpdateUI();
    }

    /// <summary>
    /// Updates only the lives display.
    /// </summary>
    public void UpdateLives(int lives)
    {
        currentLives = lives;
        UpdateUI();
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        isGameActive = false;
        Time.timeScale = 0;
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void ResumeGame()
    {
        isGameActive = true;
        Time.timeScale = 1;
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("[GameManager] Quit requested");
    }

    /// <summary>
    /// Gets the selected ship index from player prefs.
    /// </summary>
    public int GetSelectedShipIndex()
    {
        return PlayerPrefs.GetInt("SelectedShip", 0);
    }

    /// <summary>
    /// Gets the selected ship name from player prefs.
    /// </summary>
    public string GetSelectedShipName()
    {
        return PlayerPrefs.GetString("SelectedShipName", "Fighter");
    }
}
