using UnityEngine;

/// <summary>
/// Manages all audio in the game: background music and sound effects.
/// Sound effects are sourced from Pixabay (royalty-free).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects - Sourced from Pixabay")]
    [SerializeField] private AudioClip laserSound;
    [SerializeField] private AudioClip starCollectSound;
    [SerializeField] private AudioClip asteroidHitSound;
    [SerializeField] private AudioClip asteroidDestroySound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Settings")]
    [SerializeField] private float defaultSfxVolume = 0.7f;
    [SerializeField] private float defaultMusicVolume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    /// <summary>
    /// Initializes audio source components.
    /// </summary>
    private void InitializeAudioSources()
    {
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

        if (backgroundMusic == null)
        {
            backgroundMusic = Resources.Load<AudioClip>("Audio/solarflex-space-495656");
        }
    }

    /// <summary>
    /// Plays the background music loop.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.Play();
        }
    }

    /// <summary>
    /// Stops the background music.
    /// </summary>
    public void StopBackgroundMusic()
    {
        backgroundMusicSource.Stop();
    }

    /// <summary>
    /// Plays the laser firing sound effect.
    /// </summary>
    public void PlayLaser()
    {
        if (laserSound != null)
            sfxSource.PlayOneShot(laserSound, defaultSfxVolume);
    }

    /// <summary>
    /// Plays the star collection sound effect.
    /// </summary>
    public void PlayStarCollect()
    {
        if (starCollectSound != null)
            sfxSource.PlayOneShot(starCollectSound, defaultSfxVolume);
    }

    /// <summary>
    /// Plays the asteroid hit sound effect.
    /// </summary>
    public void PlayAsteroidHit()
    {
        if (asteroidHitSound != null)
            sfxSource.PlayOneShot(asteroidHitSound, defaultSfxVolume);
    }

    /// <summary>
    /// Plays the asteroid destruction sound effect.
    /// </summary>
    public void PlayAsteroidDestroy()
    {
        if (asteroidDestroySound != null)
            sfxSource.PlayOneShot(asteroidDestroySound, defaultSfxVolume);
    }

    /// <summary>
    /// Plays the game over sound effect.
    /// </summary>
    public void PlayGameOver()
    {
        if (gameOverSound != null)
            sfxSource.PlayOneShot(gameOverSound, defaultSfxVolume);
    }

    /// <summary>
    /// Sets the background music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        backgroundMusicSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the sound effects volume.
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}
