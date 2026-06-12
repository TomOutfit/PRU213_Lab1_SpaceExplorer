using UnityEngine;

/// <summary>
/// Manages all audio: background music (looping) and one-shot sound effects.
/// Attach to a GameObject with AudioSource components assigned in the Inspector,
/// or let this script create them automatically.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource backgroundMusicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip laserSound;
    public AudioClip starCollectSound;
    public AudioClip asteroidHitSound;
    public AudioClip asteroidDestroySound;
    public AudioClip gameOverSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float defaultSfxVolume = 0.7f;
    [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // If the new scene has a different BGM, play it on the persistent Instance
            if (this.backgroundMusic != null && Instance.backgroundMusicSource.clip != this.backgroundMusic)
            {
                Instance.PlayMusic(this.backgroundMusic);
            }
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Ensure we have AudioSource components
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
            backgroundMusicSource.playOnAwake = false;
            backgroundMusicSource.volume = defaultMusicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = defaultSfxVolume;
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            if (backgroundMusicSource != null)
            {
                backgroundMusicSource.Stop();
            }
        }
        else if (scene.name == "Play")
        {
            if (backgroundMusicSource != null && backgroundMusic != null)
            {
                if (backgroundMusicSource.clip != backgroundMusic || !backgroundMusicSource.isPlaying)
                {
                    backgroundMusicSource.clip = backgroundMusic;
                    backgroundMusicSource.volume = defaultMusicVolume;
                    backgroundMusicSource.loop = true;
                    backgroundMusicSource.Play();
                }
            }
        }
    }

    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Play")
        {
            if (backgroundMusic != null && backgroundMusicSource != null)
            {
                backgroundMusicSource.clip = backgroundMusic;
                backgroundMusicSource.volume = defaultMusicVolume;
                backgroundMusicSource.loop = true;
                backgroundMusicSource.Play();
            }
        }
        else if (currentScene == "Menu")
        {
            if (backgroundMusicSource != null)
            {
                backgroundMusicSource.Stop();
            }
        }
    }

    public void PlayLaser() => PlayOneShot(laserSound);
    public void PlayStarCollect() => PlayOneShot(starCollectSound);
    public void PlayAsteroidHit() => PlayOneShot(asteroidHitSound);
    public void PlayAsteroidDestroy() => PlayOneShot(asteroidDestroySound);
    public void PlayGameOver() => PlayOneShot(gameOverSound);

    public void PlayMusic(AudioClip clip)
    {
        if (backgroundMusicSource != null && clip != null)
        {
            backgroundMusicSource.clip = clip;
            backgroundMusicSource.volume = defaultMusicVolume;
            backgroundMusicSource.Play();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, defaultSfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSfxVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }
}
