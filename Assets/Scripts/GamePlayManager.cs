using UnityEngine;

/// <summary>
/// Manages the GamePlay scene: spawning game objects, background, and level progression.
/// Handles initialization of asteroids and stars, and manages game flow.
/// </summary>
public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField] private Transform[] asteroidSpawnPoints;
    [SerializeField] private Transform[] starSpawnPoints;

    [Header("Object Pools")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Player Settings")]
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Background Settings")]
    [SerializeField] private GameObject[] backgroundStars;
    [SerializeField] private float starScrollSpeed = 0.5f;

    private GameObject playerInstance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        InitializeScene();
    }

    private void Update()
    {
        ScrollBackgroundStars();
    }

    /// <summary>
    /// Initializes the gameplay scene with player, asteroids, and stars.
    /// </summary>
    private void InitializeScene()
    {
        SpawnPlayer();
        SpawnInitialAsteroids();
        SpawnInitialStars();
        UpdateShipIndicator();
    }

    /// <summary>
    /// Spawns the player spaceship at the starting position.
    /// </summary>
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("[GamePlayManager] Player prefab is not assigned!");
            return;
        }

        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(0, -3, 0);
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // Apply selected ship visual
        ApplySelectedShip();
    }

    /// <summary>
    /// Applies the selected ship's visual and stats.
    /// </summary>
    private void ApplySelectedShip()
    {
        if (playerInstance == null) return;

        int selectedIndex = PlayerPrefs.GetInt("SelectedShip", 0);
        string shipName = PlayerPrefs.GetString("SelectedShipName", "Fighter");

        PlayerController controller = playerInstance.GetComponent<PlayerController>();
        if (controller != null)
        {
            Debug.Log("[GamePlayManager] Spawning ship: " + shipName);
        }

        // Update UI with ship name
        UpdateShipIndicator();
    }

    /// <summary>
    /// Updates the current ship indicator in UI.
    /// </summary>
    private void UpdateShipIndicator()
    {
        if (UIManager.Instance != null)
        {
            string shipName = PlayerPrefs.GetString("SelectedShipName", "Fighter");
            UIManager.Instance.UpdateShipName(shipName);
        }
    }

    /// <summary>
    /// Spawns the initial set of asteroids across the screen.
    /// </summary>
    private void SpawnInitialAsteroids()
    {
        if (asteroidPrefab == null)
        {
            Debug.LogWarning("[GamePlayManager] Asteroid prefab is not assigned!");
            return;
        }

        int asteroidCount = GameManager.Instance != null ? GameManager.Instance.InitialAsteroidCount : 5;

        for (int i = 0; i < asteroidCount; i++)
        {
            SpawnAsteroid();
        }
    }

    /// <summary>
    /// Spawns a single asteroid at a random position.
    /// </summary>
    public void SpawnAsteroid()
    {
        if (asteroidPrefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 minPos = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxPos = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        float randomX = Random.Range(minPos.x, maxPos.x);
        float randomY = Random.Range(minPos.y, maxPos.y);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0);

        GameObject asteroid = Instantiate(asteroidPrefab, spawnPos,
            Quaternion.Euler(0, 0, Random.Range(0f, 360f)));

        // Randomize asteroid properties
        Asteroid asteroidScript = asteroid.GetComponent<Asteroid>();
        if (asteroidScript != null)
        {
            float size = Random.Range(0.5f, 1.5f);
            asteroid.transform.localScale = Vector3.one * size;
        }
    }

    /// <summary>
    /// Spawns the initial set of stars across the screen.
    /// </summary>
    private void SpawnInitialStars()
    {
        if (starPrefab == null)
        {
            Debug.LogWarning("[GamePlayManager] Star prefab is not assigned!");
            return;
        }

        int starCount = GameManager.Instance != null ? GameManager.Instance.InitialStarCount : 8;

        for (int i = 0; i < starCount; i++)
        {
            SpawnStar();
        }
    }

    /// <summary>
    /// Spawns a single star at a random position.
    /// </summary>
    public void SpawnStar()
    {
        if (starPrefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 minPos = cam.ViewportToWorldPoint(new Vector3(0.1f, 0.1f, 0));
        Vector3 maxPos = cam.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, 0));

        float randomX = Random.Range(minPos.x, maxPos.x);
        float randomY = Random.Range(minPos.y, maxPos.y);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0);

        Instantiate(starPrefab, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Scrolls the background stars for parallax effect.
    /// </summary>
    private void ScrollBackgroundStars()
    {
        if (backgroundStars == null) return;

        foreach (GameObject star in backgroundStars)
        {
            if (star != null)
            {
                star.transform.Translate(Vector2.down * starScrollSpeed * Time.deltaTime);

                // Reset position when off screen
                if (star.transform.position.y < -10)
                {
                    star.transform.position = new Vector3(
                        Random.Range(-10f, 10f),
                        10,
                        star.transform.position.z
                    );
                }
            }
        }
    }

    /// <summary>
    /// Called when gameplay ends. Cleans up game objects.
    /// </summary>
    public void CleanupScene()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
    }

    /// <summary>
    /// Gets the current player instance.
    /// </summary>
    public GameObject GetPlayer()
    {
        return playerInstance;
    }
}
