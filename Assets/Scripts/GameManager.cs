// =============================================================================
// GameManager.cs
// Purpose: Singleton MonoBehaviour that manages scene transitions,
//          audio playback, and game-over logic for Space Explorer.
// Updated: 2026-05-18 - Added new audio clips for power-ups and UI
// =============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that coordinates scene loading, audio playback,
/// and end-game sequencing across all scenes in Space Explorer.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }

    // Audio Clips
    [Header("Game Audio")]
    public AudioClip laserClip;
    public AudioClip loseClip;
    public AudioClip twoToneClip;
    public AudioClip backgroundMusic;

    [Header("New Audio Clips")]
    public AudioClip shieldHitClip;
    public AudioClip powerUpCollectClip;
    public AudioClip enemyDestroyClip;
    public AudioClip ufoDestroyClip;
    public AudioClip comboClip;
    public AudioClip streakClip;
    public AudioClip buttonClickClip;

    // Private Components
    private AudioSource sfxSource;
    private AudioSource bgmSource;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void Start()
    {
        if (backgroundMusic != null && bgmSource != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.Play();
        }
    }

    private void SetupAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            sfxSource = sources[0];
            bgmSource = sources[1];
        }

        bgmSource.loop = true;
        bgmSource.volume = 0.5f;
    }

    // -------------------------------------------------------------------------
    // Audio Methods
    // -------------------------------------------------------------------------

    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("GameManager.PlaySound: clip is null");
            return;
        }

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayComboSound()
    {
        PlaySound(comboClip);
    }

    public void PlayStreakSound()
    {
        PlaySound(streakClip);
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClickClip);
    }

    // -------------------------------------------------------------------------
    // Scene Management
    // -------------------------------------------------------------------------

    public void LoadScene(string sceneName)
    {
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load scene '{sceneName}': {ex.Message}");
        }
    }

    public void RestartGame()
    {
        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Reset power-ups
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.DeactivateAllPowerUps();
        }

        // Load game scene
        LoadScene("GamePlayScene");
    }

    public void ReturnToMainMenu()
    {
        LoadScene("MainMenuScreen");
    }

    // -------------------------------------------------------------------------
    // Game Over
    // -------------------------------------------------------------------------

    public void EndGame(GameObject asteroidToDestroy)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPlayerDamaged(20);
        }

        PlaySound(loseClip);

        if (asteroidToDestroy != null)
        {
            Destroy(asteroidToDestroy);
        }

        LoadScene("GameOverScreen");
    }

    // -------------------------------------------------------------------------
    // Coin System Integration
    // -------------------------------------------------------------------------

    public void AddCoinsFromGameOver()
    {
        if (ScoreManager.Instance != null && ShipSelectionController.Instance != null)
        {
            int coinsEarned = ScoreManager.Instance.Score / 10;
            ShipSelectionController.Instance.AddCoins(coinsEarned);
        }
    }
}
