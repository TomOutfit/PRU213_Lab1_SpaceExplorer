using UnityEngine;

/// <summary>
/// Controls asteroid behavior: random movement, rotation, and collision handling.
/// Asteroids float through space and can damage the player on collision.
/// </summary>
public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Size Settings")]
    [SerializeField] private float minSize = 0.5f;
    [SerializeField] private float maxSize = 1.5f;

    [Header("Health Settings")]
    [SerializeField] private float health = 30f;
    [SerializeField] private int scoreValue = 10;

    [Header("Visual")]
    [SerializeField] private Color asteroidColor = Color.gray;
    [SerializeField] private GameObject explosionEffect;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float currentSpeed;
    private float currentSize;
    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private bool isDestroyed = false;

    public float Health => currentHealth;
    public float MaxHealth => health;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitializeAsteroid();
    }

    private void Update()
    {
        RotateAsteroid();
    }

    /// <summary>
    /// Initializes asteroid with random size, speed, and direction.
    /// </summary>
    private void InitializeAsteroid()
    {
        // Random size
        currentSize = Random.Range(minSize, maxSize);
        transform.localScale = Vector3.one * currentSize;

        // Set health based on size
        health = 20f + (currentSize * 20f);
        currentHealth = health;

        // Random speed (bigger = slower)
        currentSpeed = Random.Range(minSpeed, maxSpeed) / currentSize;
        currentSpeed = Mathf.Clamp(currentSpeed, 0.5f, maxSpeed);

        // Random direction
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

        // Apply movement
        rb.linearVelocity = moveDirection * currentSpeed;

        // Random color variation
        if (spriteRenderer != null)
        {
            float colorVariation = Random.Range(-0.1f, 0.1f);
            spriteRenderer.color = new Color(
                asteroidColor.r + colorVariation,
                asteroidColor.g + colorVariation,
                asteroidColor.b + colorVariation
            );
        }
    }

    /// <summary>
    /// Rotates the asteroid continuously for visual effect.
    /// </summary>
    private void RotateAsteroid()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * Random.Range(0.8f, 1.2f));
    }

    /// <summary>
    /// Applies damage to the asteroid.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (isDestroyed) return;

        currentHealth -= damageAmount;

        // Visual feedback
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Destroy();
        }
    }

    /// <summary>
    /// Flash effect when damaged.
    /// </summary>
    private System.Collections.IEnumerator DamageFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Destroys the asteroid with effects.
    /// </summary>
    private void Destroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Play sound
        AudioManager.Instance?.PlayAsteroidDestroy();

        // Spawn smaller asteroids
        if (currentSize > 0.8f)
        {
            SpawnDebris();
        }

        RespawnAtEdge();
    }

    /// <summary>
    /// Spawns smaller debris asteroids.
    /// </summary>
    private void SpawnDebris()
    {
        int debrisCount = Random.Range(2, 4);

        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = Instantiate(gameObject, transform.position, Quaternion.identity);
            debris.transform.localScale = Vector3.one * (currentSize * 0.4f);

            // Random velocity
            Rigidbody2D debrisRb = debris.GetComponent<Rigidbody2D>();
            if (debrisRb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                debrisRb.linearVelocity = randomDir * currentSpeed * 1.5f;
            }

            // Reduce health
            Asteroid asteroid = debris.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.SetHealth(health * 0.3f);
            }
        }
    }

    /// <summary>
    /// Sets the health value directly.
    /// </summary>
    public void SetHealth(float newHealth)
    {
        health = newHealth;
        currentHealth = newHealth;
    }

    /// <summary>
    /// Called when asteroid exits the screen boundary.
    /// </summary>
    private void OnBecameInvisible()
    {
        // Respawn at a random edge of the screen
        RespawnAtEdge();
    }

    /// <summary>
    /// Respawns the asteroid at a random screen edge with new direction.
    /// </summary>
    private void RespawnAtEdge()
    {
        if (!gameObject.activeInHierarchy || isDestroyed)
        {
            isDestroyed = false;
            gameObject.SetActive(true);
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 minPos = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxPos = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Choose random edge
        int edge = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (edge)
        {
            case 0: // Top
                spawnPos = new Vector3(Random.Range(minPos.x, maxPos.x), maxPos.y + 1, 0);
                moveDirection = Vector2.down;
                break;
            case 1: // Bottom
                spawnPos = new Vector3(Random.Range(minPos.x, maxPos.x), minPos.y - 1, 0);
                moveDirection = Vector2.up;
                break;
            case 2: // Left
                spawnPos = new Vector3(minPos.x - 1, Random.Range(minPos.y, maxPos.y), 0);
                moveDirection = Vector2.right;
                break;
            case 3: // Right
                spawnPos = new Vector3(maxPos.x + 1, Random.Range(minPos.y, maxPos.y), 0);
                moveDirection = Vector2.left;
                break;
        }

        transform.position = spawnPos;
        rb.linearVelocity = moveDirection * currentSpeed;

        // Reset health if respawning
        currentHealth = health;
        isDestroyed = false;

        // Randomize size and speed on respawn
        currentSize = Random.Range(minSize, maxSize);
        transform.localScale = Vector3.one * currentSize;
        currentSpeed = Random.Range(minSpeed, maxSpeed) / currentSize;
        rb.linearVelocity = moveDirection * currentSpeed;
    }

    /// <summary>
    /// Called when asteroid collides with laser.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser != null)
            {
                TakeDamage(laser.damage);
            }
            Destroy(other.gameObject);
        }
    }
}
