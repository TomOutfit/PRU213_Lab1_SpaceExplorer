using UnityEngine;

/// <summary>
/// Controls the movement of the asteroid and handles collision with player and laser.
/// Enhanced with point values and effects.
/// </summary>
public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    [Header("Size Variants")]
    public float bigSizeMultiplier = 1.5f;
    public float smallSizeMultiplier = 0.5f;

    [Header("Visual Effects")]
    public bool useRotation = true;
    public bool useWobble = true;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float speed;
    private Vector3 direction;
    private float rotationSpeed;
    private float wobbleTime;
    private SpriteRenderer spriteRenderer;

    // Size categories
    public enum SizeCategory { Big, Medium, Small }
    private SizeCategory sizeCategory;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        InitializeAsteroid();
    }

    void Update()
    {
        MoveAsteroid();
        RotateAsteroid();
        CheckBounds();
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void InitializeAsteroid()
    {
        // Randomize speed
        speed = Random.Range(minSpeed, maxSpeed);

        // Randomize direction (mostly downward with slight horizontal variance)
        direction = new Vector3(Random.Range(-0.5f, 0.5f), -1f, 0f).normalized;

        // Randomize rotation
        rotationSpeed = Random.Range(-150f, 150f);

        // Randomize wobble
        wobbleTime = Random.Range(0f, 100f);

        // Determine size category based on name or random
        DetermineSizeCategory();
    }

    private void DetermineSizeCategory()
    {
        string objName = gameObject.name.ToLower();

        if (objName.Contains("big"))
        {
            sizeCategory = SizeCategory.Big;
            transform.localScale *= bigSizeMultiplier;
        }
        else if (objName.Contains("small") || objName.Contains("tiny"))
        {
            sizeCategory = SizeCategory.Small;
            transform.localScale *= smallSizeMultiplier;
        }
        else
        {
            sizeCategory = SizeCategory.Medium;
        }
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void MoveAsteroid()
    {
        // Add wobble effect
        Vector3 wobbleOffset = Vector3.zero;
        if (useWobble)
        {
            float wobbleX = Mathf.Sin(wobbleTime * 2f) * 0.05f;
            wobbleOffset = new Vector3(wobbleX, 0f, 0f);
            wobbleTime += Time.deltaTime;
        }

        transform.position += (direction * speed + wobbleOffset) * Time.deltaTime;
    }

    private void RotateAsteroid()
    {
        if (useRotation)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    private void CheckBounds()
    {
        // Destroy asteroid if out of bounds
        if (transform.position.y < -7f ||
            transform.position.x < -12f ||
            transform.position.x > 12f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Collision
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
        else if (collision.CompareTag("Laser"))
        {
            HandleLaserCollision(collision);
        }
    }

    private void HandlePlayerCollision(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnEnemyCollision();
        }

        // Play collision sound
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.loseClip);
        }

        Destroy(gameObject);
    }

    private void HandleLaserCollision(Collider2D collision)
    {
        // Award points
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddAsteroidDestroyScore();
        }

        // Play destroy sound
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.laserClip);
        }

        // Create explosion effect (can be enhanced)
        CreateExplosionEffect();

        // Destroy laser
        Destroy(collision.gameObject);

        // Destroy asteroid
        Destroy(gameObject);
    }

    private void CreateExplosionEffect()
    {
        // Simple particle burst effect
        GameObject effect = new GameObject("AsteroidExplosion");
        effect.transform.position = transform.position;

        // Add sprite renderer with asteroid sprite
        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        sr.sortingOrder = 10;

        // Animate expansion and fade
        StartCoroutine(AnimateExplosion(effect));
    }

    private System.Collections.IEnumerator AnimateExplosion(GameObject effect)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            effect.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            Color c = effect.GetComponent<SpriteRenderer>().color;
            c.a = 1f - t;
            effect.GetComponent<SpriteRenderer>().color = c;

            yield return null;
        }

        Destroy(effect);
    }

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the point value for destroying this asteroid
    /// </summary>
    public int GetPointValue()
    {
        return sizeCategory switch
        {
            SizeCategory.Big => 3,
            SizeCategory.Small => 8,
            _ => 5
        };
    }

    /// <summary>
    /// Sets a custom size for this asteroid
    /// </summary>
    public void SetSize(SizeCategory category)
    {
        sizeCategory = category;

        float multiplier = category switch
        {
            SizeCategory.Big => bigSizeMultiplier,
            SizeCategory.Small => smallSizeMultiplier,
            _ => 1f
        };

        transform.localScale = Vector3.one * multiplier;
    }
}
