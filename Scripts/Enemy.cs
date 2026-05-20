using UnityEngine;

/// <summary>
/// Defines different enemy types with unique behaviors and point values.
/// </summary>
public enum EnemyType
{
    Basic,      // Simple straight movement
    Zigzag,     // Moves in zigzag pattern
    Chaser,     // Slowly moves toward player
    Shooter,    // Can shoot at player
    UFO         // Moves horizontally at top
}

/// <summary>
/// Controls enemy ship movement and behavior.
/// Different enemy types have different movement patterns and attack strategies.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public EnemyType enemyType = EnemyType.Basic;
    public int pointsValue = 15;
    public int damageValue = 1;

    [Header("Movement")]
    public float speed = 2f;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Combat")]
    public float shootInterval = 2f;
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    [Header("Visual")]
    public Sprite[] sprites;
    public Color enemyColor = Color.white;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Vector3 _startPosition;
    private float _time;
    private float _randomOffset;
    private float _nextShootTime;
    private SpriteRenderer _spriteRenderer;
    private PlayerController _playerRef;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        SetupVisuals();
    }

    void Start()
    {
        _startPosition = transform.position;
        _time = 0f;
        _randomOffset = Random.Range(0f, 100f);

        if (enemyType == EnemyType.Shooter)
        {
            _nextShootTime = Random.Range(1f, shootInterval);
        }

        _playerRef = FindObjectOfType<PlayerController>();

        // Randomize speed
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        MoveEnemy();
        UpdateBehavior();
        CheckBounds();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    private void SetupVisuals()
    {
        if (_spriteRenderer != null && sprites.Length > 0)
        {
            _spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
            _spriteRenderer.color = enemyColor;
        }
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void MoveEnemy()
    {
        _time += Time.deltaTime;

        switch (enemyType)
        {
            case EnemyType.Basic:
                MoveBasic();
                break;
            case EnemyType.Zigzag:
                MoveZigzag();
                break;
            case EnemyType.Chaser:
                MoveChaser();
                break;
            case EnemyType.Shooter:
                MoveShooter();
                break;
            case EnemyType.UFO:
                MoveUFO();
                break;
        }
    }

    private void MoveBasic()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    private void MoveZigzag()
    {
        float xOffset = Mathf.Sin(_time * zigzagFrequency + _randomOffset) * zigzagAmplitude;
        float yOffset = -speed * Time.deltaTime;

        Vector3 newPos = _startPosition + new Vector3(xOffset, yOffset, 0f);
        newPos.x = Mathf.Clamp(newPos.x, -8f, 8f);
        transform.position = newPos;
    }

    private void MoveChaser()
    {
        if (_playerRef != null)
        {
            Vector3 direction = (_playerRef.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
    }

    private void MoveShooter()
    {
        // Move down slowly and side to side
        float xOffset = Mathf.Sin(_time * 1.5f + _randomOffset) * 2f;
        float yOffset = -speed * 0.5f * Time.deltaTime;

        Vector3 newPos = _startPosition + new Vector3(xOffset, yOffset, 0f);
        newPos.x = Mathf.Clamp(newPos.x, -8f, 8f);
        transform.position = newPos;
    }

    private void MoveUFO()
    {
        // UFO moves horizontally at the top
        float direction = Mathf.Sin(_time * 0.5f + _randomOffset) > 0 ? 1f : -1f;
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Keep in bounds
        if (transform.position.x > 9f || transform.position.x < -9f)
        {
            direction *= -1f;
        }
    }

    // -------------------------------------------------------------------------
    // Behavior
    // -------------------------------------------------------------------------

    private void UpdateBehavior()
    {
        if (enemyType == EnemyType.Shooter)
        {
            _nextShootTime -= Time.deltaTime;
            if (_nextShootTime <= 0)
            {
                Shoot();
                _nextShootTime = shootInterval;
            }
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

            // Bullet moves down toward player
            Vector3 direction = Vector3.down;
            if (_playerRef != null)
            {
                direction = (_playerRef.transform.position - transform.position).normalized;
            }

            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(direction);
            }
        }
    }

    private void CheckBounds()
    {
        // Destroy if off screen
        if (transform.position.y < -7f ||
            transform.position.y > 8f ||
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
            // Notify player
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnEnemyCollision(damageValue);
            }

            // Destroy enemy
            DestroyEnemy();
        }
        else if (collision.CompareTag("Laser"))
        {
            // Destroy laser
            Destroy(collision.gameObject);

            // Destroy enemy and award points
            DestroyEnemy();

            // Award points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddEnemyDestroyScore();
            }
        }
    }

    private void DestroyEnemy()
    {
        // Play destroy sound if available
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.laserClip);
        }

        Destroy(gameObject);
    }
}
