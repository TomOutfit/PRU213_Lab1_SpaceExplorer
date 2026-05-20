using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enhanced spawn manager that handles all game objects:
/// - Asteroids (existing)
/// - Stars (existing, now with types)
/// - Power-ups (new)
/// - Enemies (new)
/// - UFOs (new)
/// </summary>
public class SpawnManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Spawn Configuration
    // -------------------------------------------------------------------------

    [Header("Spawn Intervals")]
    public float baseSpawnInterval = 2f;
    public float minSpawnInterval = 0.5f;
    public float difficultyScaling = 0.02f; // How much faster spawns get over time

    [Header("Asteroid Prefabs")]
    public GameObject[] asteroidPrefabs;

    [Header("Star Prefabs")]
    public GameObject[] starPrefabs;
    public float starSpawnChance = 0.3f;

    [Header("Power-Up Prefabs")]
    public GameObject[] powerUpPrefabs;
    public float powerUpSpawnChance = 0.1f;

    [Header("Enemy Prefabs")]
    public GameObject[] basicEnemyPrefabs;
    public GameObject[] zigzagEnemyPrefabs;
    public GameObject[] chaserEnemyPrefabs;
    public GameObject[] shooterEnemyPrefabs;
    public GameObject[] ufoPrefabs;
    public float enemySpawnChance = 0.25f;

    [Header("Spawn Ranges")]
    public float xSpawnRange = 8f;
    public float ySpawnPos = 6f;

    [Header("Difficulty")]
    public bool enableDifficultyScaling = true;
    public float difficultyIncreaseRate = 0.001f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float _currentDifficulty = 1f;
    private float _gameTime = 0f;
    private float _nextAsteroidSpawn = 0f;
    private float _nextStarSpawn = 0f;
    private float _nextPowerUpSpawn = 0f;
    private float _nextEnemySpawn = 0f;
    private float _nextUFOSpawn = 0f;

    private bool _isSpawning = true;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        StartAllSpawnCoroutines();
    }

    void Update()
    {
        if (enableDifficultyScaling)
        {
            _gameTime += Time.deltaTime;
            _currentDifficulty = 1f + (_gameTime * difficultyIncreaseRate);
        }
    }

    // -------------------------------------------------------------------------
    // Spawn Coroutines
    // -------------------------------------------------------------------------

    private void StartAllSpawnCoroutines()
    {
        StartCoroutine(SpawnAsteroidRoutine());
        StartCoroutine(SpawnStarRoutine());
        StartCoroutine(SpawnPowerUpRoutine());
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnUFORoutine());
    }

    private IEnumerator SpawnAsteroidRoutine()
    {
        while (_isSpawning)
        {
            float interval = GetScaledInterval(baseSpawnInterval);
            yield return new WaitForSeconds(interval);

            if (asteroidPrefabs != null && asteroidPrefabs.Length > 0)
            {
                SpawnAsteroid();
            }
        }
    }

    private IEnumerator SpawnStarRoutine()
    {
        while (_isSpawning)
        {
            float interval = GetScaledInterval(baseSpawnInterval * 1.5f);
            yield return new WaitForSeconds(interval);

            if (Random.value < starSpawnChance && starPrefabs != null && starPrefabs.Length > 0)
            {
                SpawnStar();
            }
        }
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        while (_isSpawning)
        {
            float interval = GetScaledInterval(baseSpawnInterval * 4f);
            yield return new WaitForSeconds(interval);

            if (Random.value < powerUpSpawnChance && powerUpPrefabs != null && powerUpPrefabs.Length > 0)
            {
                SpawnPowerUp();
            }
        }
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (_isSpawning)
        {
            float interval = GetScaledInterval(baseSpawnInterval * 2f);
            yield return new WaitForSeconds(interval);

            if (Random.value < enemySpawnChance * GetDifficultyMultiplier())
            {
                SpawnEnemy();
            }
        }
    }

    private IEnumerator SpawnUFORoutine()
    {
        while (_isSpawning)
        {
            float interval = GetScaledInterval(baseSpawnInterval * 8f);
            yield return new WaitForSeconds(interval);

            if (Random.value < 0.3f && ufoPrefabs != null && ufoPrefabs.Length > 0)
            {
                SpawnUFO();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Spawn Methods
    // -------------------------------------------------------------------------

    private void SpawnAsteroid()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0) return;

        GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        Vector3 spawnPos = GetRandomSpawnPosition();
        Instantiate(prefab, spawnPos, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
    }

    private void SpawnStar()
    {
        if (starPrefabs == null || starPrefabs.Length == 0) return;

        // Randomly select star type based on difficulty
        GameObject prefab = GetRandomStarPrefab();
        Vector3 spawnPos = GetRandomSpawnPosition();
        Vector3 adjustedPos = new Vector3(spawnPos.x, ySpawnPos, 0);

        GameObject star = Instantiate(prefab, adjustedPos, Quaternion.identity);

        // Randomize star type
        Star starScript = star.GetComponent<Star>();
        if (starScript != null)
        {
            StarType type = GetRandomStarType();
            starScript.SetStarType(type);
        }
    }

    private GameObject GetRandomStarPrefab()
    {
        if (starPrefabs.Length == 0) return null;
        return starPrefabs[Random.Range(0, starPrefabs.Length)];
    }

    private StarType GetRandomStarType()
    {
        float roll = Random.value;

        // Higher difficulty = more gold stars
        if (roll < 0.1f * GetDifficultyMultiplier())
        {
            return StarType.Gold;
        }
        else if (roll < 0.3f)
        {
            return StarType.Silver;
        }
        else
        {
            return StarType.Blue;
        }
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        Vector3 spawnPos = GetRandomSpawnPosition();
        Vector3 adjustedPos = new Vector3(spawnPos.x, ySpawnPos * 0.8f, 0);

        Instantiate(prefab, adjustedPos, Quaternion.identity);
    }

    private void SpawnEnemy()
    {
        // Select random enemy type based on difficulty
        EnemyType type = GetRandomEnemyType();
        GameObject[] prefabs = GetEnemyPrefabs(type);

        if (prefabs == null || prefabs.Length == 0) return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        Vector3 spawnPos = GetRandomSpawnPosition();
        Vector3 adjustedPos = new Vector3(spawnPos.x, ySpawnPos, 0);

        GameObject enemy = Instantiate(prefab, adjustedPos, Quaternion.identity);

        // Set enemy type
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.enemyType = type;
            ScaleEnemyStats(enemyScript);
        }
    }

    private EnemyType GetRandomEnemyType()
    {
        float roll = Random.value;
        float difficulty = GetDifficultyMultiplier();

        if (roll < 0.1f * difficulty)
        {
            return EnemyType.Shooter;
        }
        else if (roll < 0.25f * difficulty)
        {
            return EnemyType.Chaser;
        }
        else if (roll < 0.4f)
        {
            return EnemyType.Zigzag;
        }
        else
        {
            return EnemyType.Basic;
        }
    }

    private GameObject[] GetEnemyPrefabs(EnemyType type)
    {
        return type switch
        {
            EnemyType.Basic => basicEnemyPrefabs,
            EnemyType.Zigzag => zigzagEnemyPrefabs,
            EnemyType.Chaser => chaserEnemyPrefabs,
            EnemyType.Shooter => shooterEnemyPrefabs,
            _ => basicEnemyPrefabs
        };
    }

    private void ScaleEnemyStats(Enemy enemy)
    {
        // Scale enemy stats based on difficulty
        enemy.speed *= GetDifficultyMultiplier();
        enemy.pointsValue = Mathf.RoundToInt(enemy.pointsValue * GetDifficultyMultiplier());
    }

    private void SpawnUFO()
    {
        if (ufoPrefabs == null || ufoPrefabs.Length == 0) return;

        GameObject prefab = ufoPrefabs[Random.Range(0, ufoPrefabs.Length)];

        // UFO spawns at top, moving horizontally
        float xPos = Random.value > 0.5f ? -8f : 8f;
        Vector3 spawnPos = new Vector3(xPos, ySpawnPos * 0.8f, 0);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    // -------------------------------------------------------------------------
    // Utility
    // -------------------------------------------------------------------------

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-xSpawnRange, xSpawnRange);
        return new Vector3(x, ySpawnPos, 0);
    }

    private float GetScaledInterval(float baseInterval)
    {
        return Mathf.Max(baseInterval / GetDifficultyMultiplier(), minSpawnInterval);
    }

    private float GetDifficultyMultiplier()
    {
        return Mathf.Min(_currentDifficulty, 3f); // Cap at 3x difficulty
    }

    // -------------------------------------------------------------------------
    // Control Methods
    // -------------------------------------------------------------------------

    public void PauseSpawning()
    {
        _isSpawning = false;
    }

    public void ResumeSpawning()
    {
        _isSpawning = true;
        StartAllSpawnCoroutines();
    }

    public void StopAllSpawning()
    {
        _isSpawning = false;
        StopAllCoroutines();
    }

    public void ResetDifficulty()
    {
        _currentDifficulty = 1f;
        _gameTime = 0f;
    }

    public float GetCurrentDifficulty()
    {
        return _currentDifficulty;
    }
}
