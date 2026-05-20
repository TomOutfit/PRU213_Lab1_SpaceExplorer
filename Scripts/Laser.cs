using UnityEngine;

/// <summary>
/// Enhanced laser script with piercing capability.
/// Can destroy asteroids and pass through enemies when piercing is active.
/// </summary>
public class Laser : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    public float lifetime = 3f;

    [Header("Combat Settings")]
    public int baseDamage = 1;
    public int asteroidPoints = 5;

    [Header("Piercing Settings")]
    public bool isPiercing = false;
    public int maxPierceCount = 3;
    public bool destroyOnAsteroidHit = false;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int _pierceCount = 0;
    private int _currentDamage;
    private SpriteRenderer _spriteRenderer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        _currentDamage = baseDamage;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        MoveLaser();
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void MoveLaser()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Destroy if off screen
        if (transform.position.y > 7f || transform.position.y < -7f ||
            transform.position.x < -12f || transform.position.x > 12f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Collision
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Asteroid"))
        {
            HandleAsteroidCollision(collision);
        }
        else if (collision.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision);
        }
    }

    private void HandleAsteroidCollision(Collider2D collision)
    {
        // Add score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddAsteroidDestroyScore();
        }

        // Destroy asteroid
        Destroy(collision.gameObject);

        // Determine if laser should be destroyed
        if (!isPiercing || destroyOnAsteroidHit)
        {
            Destroy(gameObject);
        }
        else
        {
            _pierceCount++;
            UpdatePiercingVisual();

            if (_pierceCount >= maxPierceCount)
            {
                Destroy(gameObject);
            }
        }
    }

    private void HandleEnemyCollision(Collider2D collision)
    {
        // Add score for enemy destroy
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddEnemyDestroyScore();
        }

        // Destroy enemy
        Destroy(collision.gameObject);

        // If not piercing, destroy laser
        if (!isPiercing)
        {
            Destroy(gameObject);
        }
        else
        {
            _pierceCount++;
            UpdatePiercingVisual();

            if (_pierceCount >= maxPierceCount)
            {
                Destroy(gameObject);
            }
        }
    }

    private void UpdatePiercingVisual()
    {
        if (_spriteRenderer == null) return;

        // Visual feedback for piercing - change color slightly with each pierce
        float alpha = 1f - ((float)_pierceCount / maxPierceCount) * 0.3f;
        Color c = _spriteRenderer.color;
        c.a = alpha;
        _spriteRenderer.color = c;
    }

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Enable piercing mode with custom settings
    /// </summary>
    public void EnablePiercing(int maxPierces = 3, bool destroyOnAsteroid = false)
    {
        isPiercing = true;
        maxPierceCount = maxPierces;
        destroyOnAsteroidHit = destroyOnAsteroid;
    }

    /// <summary>
    /// Set custom damage value
    /// </summary>
    public void SetDamage(int damage)
    {
        _currentDamage = damage;
    }

    /// <summary>
    /// Get remaining pierce count
    /// </summary>
    public int GetRemainingPierces()
    {
        return maxPierceCount - _pierceCount;
    }
}
