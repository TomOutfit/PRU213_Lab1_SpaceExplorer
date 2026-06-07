using UnityEngine;

/// <summary>
/// Laser projectile: travels upward each frame and destroys itself
/// when it leaves the screen or hits an asteroid.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Laser : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 3f;
    public Sprite[] laserSprites;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Camera mainCam;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 100;
            sr.color = Color.white;
            sr.enabled = true;
        }

        if (laserSprites != null && laserSprites.Length > 0)
        {
            sr.sprite = laserSprites[Random.Range(0, laserSprites.Length)];
        }
    }

    private Vector2 velocityDirection = Vector2.up;
    private bool directionSet = false;

    public void SetDirection(Vector2 dir)
    {
        velocityDirection = dir.normalized;
        directionSet = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = velocityDirection * speed;
        }

        // Rotate the laser sprite to align with the shooting angle
        float angle = Mathf.Atan2(velocityDirection.y, velocityDirection.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Start()
    {
        mainCam = Camera.main;
        spawnTime = Time.time;
        rb.bodyType = RigidbodyType2D.Kinematic; // kinematic so OnCollisionEnter2D still fires
        
        if (!directionSet)
        {
            rb.linearVelocity = velocityDirection * speed;
        }
    }

    private void Update()
    {
        // Safety: destroy after max lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        if (mainCam != null)
        {
            float camTop = mainCam.transform.position.y + mainCam.orthographicSize + 2f;
            if (transform.position.y > camTop)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Asteroid"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(GameManager.Instance.pointsPerLaserHit);

            GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
            if (gpm != null)
            {
                gpm.OnAsteroidDestroyed();
                gpm.CreateExplosionEffect(collider.transform.position);
            }

            AudioManager audioMgr = FindAnyObjectByType<AudioManager>();
            if (audioMgr != null) audioMgr.PlayAsteroidDestroy();

            Destroy(collider.gameObject); // asteroid
            Destroy(gameObject);          // laser
        }
    }
}
