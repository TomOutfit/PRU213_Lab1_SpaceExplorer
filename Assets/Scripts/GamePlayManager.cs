using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene setup in the Play scene: spawning the player, asteroids, and stars,
/// and scrolling the background star layers for a parallax space feel.
/// </summary>
public class GamePlayManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject asteroidPrefab;
    public GameObject[] powerUpPrefabs;
    public GameObject laserPrefab;

    [Header("Spawning")]
    public float spawnInterval = 0.5f;
    public int maxAsteroids = 50;
    public int maxStars = 15;

    [Header("Background Scrolling")]
    public Transform[] backgroundStars;
    public float starScrollSpeed = 0.5f;

    [Header("Player Ship Variety")]
    public Sprite[] playerShipSprites;

    private float spawnTimer;
    private int activeAsteroids;
    private int activeStars;
    private Camera mainCam;
    private float spawnY;
    private float minSpawnX;
    private float maxSpawnX;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnGameOverEvent += OnGameOver;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnGameOverEvent -= OnGameOver;
    }

    private void OnGameOver()
    {
        // Clean up the scene when game is over
        string[] tagsToDestroy = { "Player", "Asteroid", "Star", "Laser" };
        foreach (string t in tagsToDestroy)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(t);
            foreach (GameObject obj in objects)
            {
                // Only destroy if it's not a UI element (layer 5 is UI)
                if (obj.layer != 5)
                {
                    Destroy(obj);
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Play")
        {
            StopAllCoroutines();
            StartCoroutine(DelayedSetup());
        }
    }

    private System.Collections.IEnumerator DelayedSetup()
    {
        yield return null; // wait one frame for scene objects to fully initialize
        if (mainCam == null) mainCam = Camera.main;
        CalculateSpawnBounds();
        SetupScene();
    }

    private void Start()
    {
        mainCam = Camera.main;
        CalculateSpawnBounds();
        SetupScene();
    }

    private void CalculateSpawnBounds()
    {
        if (mainCam == null) mainCam = Camera.main;
        float vertExtent = mainCam.orthographicSize;
        float horzExtent = vertExtent * mainCam.aspect;

        spawnY = mainCam.transform.position.y + vertExtent + 2f; // slightly above screen
        minSpawnX = mainCam.transform.position.x - horzExtent + 1f;
        maxSpawnX = mainCam.transform.position.x + horzExtent - 1f;
    }

    private void Update()
    {
        ScrollBackground();
        HandleSpawning();
    }

    private void SetupScene()
    {
        activeAsteroids = 0;
        activeStars = 0;
        spawnTimer = 0f;

        // Reset GameManager state
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGameState();

        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null) Destroy(existingPlayer);

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerShipSprites == null || playerShipSprites.Length == 0)
        {
            playerShipSprites = Resources.LoadAll<Sprite>("Ship");
        }

#if UNITY_EDITOR
        if (laserPrefab == null) laserPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Laser.prefab");
#endif
        Vector3 spawnPos = new Vector3(0, -mainCam.orthographicSize + 2f, 0); // Near bottom
        GameObject player;

        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Dynamically create the player if no prefab is assigned
            player = new GameObject("PlayerShip");
            player.transform.position = spawnPos;
            
            player.AddComponent<SpriteRenderer>().sortingOrder = 10;
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            
            player.AddComponent<BoxCollider2D>();
            
            var pc = player.AddComponent<PlayerController>();
            if (pc != null && laserPrefab != null) pc.laserPrefab = laserPrefab;
            
            player.tag = "Player";
        }

        // Apply random ship sprite if available
        if (playerShipSprites != null && playerShipSprites.Length > 0)
        {
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = playerShipSprites[Random.Range(0, playerShipSprites.Length)];
                sr.color = Color.white;
                sr.enabled = true;
                
                // Re-add PolygonCollider to fit the new sprite perfectly
                var col = player.GetComponent<PolygonCollider2D>();
                if (col != null) DestroyImmediate(col);
                player.AddComponent<PolygonCollider2D>();
            }
        }
    }

    private void SpawnAsteroid()
    {
#if UNITY_EDITOR
        if (asteroidPrefab == null) asteroidPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/bold_silver_0.prefab");
#endif
        if (asteroidPrefab == null || activeAsteroids >= maxAsteroids) return;

        Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0f);
        GameObject astObj = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
        
        Asteroid astScript = astObj.GetComponent<Asteroid>();
        // Ensure the script is attached so it actually falls down!
        if (astScript == null)
        {
            astScript = astObj.AddComponent<Asteroid>();
            if (astObj.GetComponent<Rigidbody2D>() == null)
            {
                var rb = astObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
            }
            if (astObj.GetComponent<Collider2D>() == null)
            {
                var col = astObj.AddComponent<PolygonCollider2D>();
                col.isTrigger = false;
            }
        }
        
        SpriteRenderer sr = astObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 100;
            sr.color = Color.white;
            sr.enabled = true;
#if UNITY_EDITOR
            if (sr.sprite == null)
            {
                string[] asteroidPaths = new string[] {
                    "Assets/Sprites/Asteroids/Asteroid_Large.png",
                    "Assets/Sprites/Asteroids/Asteroid_Medium.png",
                    "Assets/Sprites/Asteroids/Asteroid_Small.png",
                    "Assets/Sprites/Meteors/meteorBrown_big1.png",
                    "Assets/Sprites/Meteors/meteorBrown_med1.png",
                    "Assets/Sprites/Meteors/meteorBrown_small1.png",
                    "Assets/Sprites/Meteors/meteorGrey_big1.png",
                    "Assets/Sprites/Meteors/meteorGrey_med1.png",
                    "Assets/Sprites/Meteors/meteorGrey_small1.png"
                };
                string randomPath = asteroidPaths[Random.Range(0, asteroidPaths.Length)];
                sr.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(randomPath);
            }
#endif
        }
        
        activeAsteroids++;
    }

    private void SpawnStar()
    {
#if UNITY_EDITOR
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            string[] paths = new string[] {
                "Assets/Prefabs/powerupYellow_star_0.prefab",
                "Assets/Prefabs/powerupBlue_shield_0.prefab",
                "Assets/Prefabs/powerupRed_bolt_0.prefab"
            };
            powerUpPrefabs = new GameObject[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                powerUpPrefabs[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            }
        }
#endif
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0 || activeStars >= maxStars) return;

        GameObject selectedPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        if (selectedPrefab == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0f);
        GameObject starObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        
        // Make the item bigger so it is clearly visible to the player
        starObj.transform.localScale = new Vector3(2f, 2f, 2f);
        
        PowerUp pu = starObj.GetComponent<PowerUp>();
        // Ensure the script is attached so it actually falls down!
        if (pu == null)
        {
            pu = starObj.AddComponent<PowerUp>();
        }
        
        // Determine type based on prefab name
        if (selectedPrefab.name.ToLower().Contains("shield"))
            pu.type = PowerUp.PowerUpType.Shield;
        else if (selectedPrefab.name.ToLower().Contains("bolt") || selectedPrefab.name.ToLower().Contains("pill"))
            pu.type = PowerUp.PowerUpType.RapidFire;
        else
            pu.type = PowerUp.PowerUpType.ScoreBonus;

        starObj.tag = "Star"; // Ensure lasers ignore it and GamePlayManager cleans it up on GameOver

        if (starObj.GetComponent<Rigidbody2D>() == null)
        {
            var rb = starObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        if (starObj.GetComponent<Collider2D>() == null)
        {
            var col = starObj.AddComponent<PolygonCollider2D>();
            col.isTrigger = true;
        }
        
        SpriteRenderer sr = starObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 100;
            sr.color = Color.white;
            sr.enabled = true;
        }
        
        activeStars++;
    }

    private void HandleSpawning()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            spawnInterval = Random.Range(0.2f, 0.6f);

            int spawnCount = Random.Range(1, 4);
            for (int i = 0; i < spawnCount; i++)
            {
                if (activeAsteroids < maxAsteroids)
                {
                    SpawnAsteroid();
                }
            }

            // Randomly spawn a powerup/star occasionally
            if (activeStars < maxStars && Random.value < 0.3f)
            {
                SpawnStar();
            }
        }
    }

    private void ScrollBackground()
    {
        if (backgroundStars == null || mainCam == null) return;

        foreach (Transform star in backgroundStars)
        {
            if (star == null) continue;
            star.Translate(Vector3.down * starScrollSpeed * Time.deltaTime, Space.World);

            // Wrap around when star passes below the camera
            float camBottom = mainCam.transform.position.y - mainCam.orthographicSize - 2f;
            if (star.position.y < camBottom)
            {
                float newY = mainCam.transform.position.y + mainCam.orthographicSize + 2f;
                float randomX = Random.Range(
                    mainCam.transform.position.x - 10f,
                    mainCam.transform.position.x + 10f
                );
                star.position = new Vector3(randomX, newY, star.position.z);
            }
        }
    }

    // Called by Asteroid/Star when destroyed so the spawner can track counts
    public void OnAsteroidDestroyed() { if (activeAsteroids > 0) activeAsteroids--; }
    public void OnStarCollected() { if (activeStars > 0) activeStars--; }

    public void CreateExplosionEffect(Vector3 pos)
    {
        GameObject fx = new GameObject("ExplosionFX");
        fx.transform.position = pos;
        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.3f;
        main.startSpeed = 10f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.5f, 0f);
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;
        renderer.sortingLayerName = "Default";

        ps.Play();
        Destroy(fx, 1f);
    }

    public void CreateCollectEffect(Vector3 pos)
    {
        GameObject fx = new GameObject("CollectFX");
        fx.transform.position = pos;
        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.4f;
        main.startSpeed = 5f;
        main.startSize = 0.15f;
        main.startColor = Color.yellow;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;
        renderer.sortingLayerName = "Default";

        ps.Play();
        Destroy(fx, 1f);
    }
}
