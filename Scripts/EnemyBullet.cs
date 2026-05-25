using UnityEngine;

/// <summary>
/// Enemy bullet projectile that travels in a specified direction.
/// Can damage the player on collision.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Configuration")]
    public float speed = 5f;
    public float lifetime = 5f;
    public int damage = 1;

    [Header("Visual")]
    public Color bulletColor = Color.red;

    private Vector3 _direction;
    private SpriteRenderer _spriteRenderer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = bulletColor;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (_direction != Vector3.zero)
        {
            transform.Translate(_direction * speed * Time.deltaTime);
        }

        // Check if off screen
        if (transform.position.y < -7f || transform.position.y > 8f ||
            transform.position.x < -12f || transform.position.x > 12f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    public void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    public void SetDirection(float x, float y, float z = 0f)
    {
        _direction = new Vector3(x, y, z).normalized;
    }

    // -------------------------------------------------------------------------
    // Collision
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnEnemyCollision(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Shield"))
        {
            // Shield blocks enemy bullets
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlaySound(GameManager.Instance.laserClip);
            }
            Destroy(gameObject);
        }
    }
}
