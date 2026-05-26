using UnityEngine;

/// <summary>
/// Controls laser projectile behavior: movement, damage, and destruction.
/// Lasers are fired by the player and destroy asteroids on collision.
/// </summary>
public class Laser : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float maxDistance = 20f;
    public float damage = 15f;
    [SerializeField] private Color laserColor = Color.red;

    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Light pointLight;

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (trail == null)
            trail = GetComponent<TrailRenderer>();
        if (pointLight == null)
            pointLight = GetComponent<Light>();
    }

    private void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifetime);

        // Set visual color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = laserColor;
        }
    }

    private void Update()
    {
        // Check if laser has traveled too far
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the damage value for this laser.
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// Sets the color of the laser projectile.
    /// </summary>
    public void SetColor(Color color)
    {
        laserColor = color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Called when laser collides with another object.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Asteroid"))
        {
            Asteroid asteroid = other.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when laser becomes invisible (exits screen).
    /// </summary>
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
