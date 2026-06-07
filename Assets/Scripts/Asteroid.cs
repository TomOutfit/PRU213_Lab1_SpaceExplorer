using UnityEngine;

/// <summary>
/// Asteroid behaviour: drops downwards from the top of the screen.
/// Destroys itself when leaving the bottom of the screen.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Asteroid : MonoBehaviour
{
    [Header("Movement")]
    public float minMoveSpeed = 2f;
    public float maxMoveSpeed = 4f;
    public float rotateSpeed = 50f;

    [Header("Appearance")]
    public Sprite[] asteroidSprites;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float moveSpeed;
    private Camera mainCam;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        // Randomize asteroid scale
        float randomScale = Random.Range(0.6f, 1.4f);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        if (sr != null) 
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 100;
            // Subtle color tint variation for organic feel
            float r = Random.Range(0.85f, 1.0f);
            float g = Random.Range(0.85f, 1.0f);
            float b = Random.Range(0.85f, 1.0f);
            sr.color = new Color(r, g, b, 1f);
            sr.enabled = true;
        }

        if (asteroidSprites != null && asteroidSprites.Length > 0)
        {
            sr.sprite = asteroidSprites[Random.Range(0, asteroidSprites.Length)];
        }

        // Tiny asteroids are faster, big ones are slower
        float baseSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        moveSpeed = baseSpeed * (1.2f / randomScale);
        
        // Random direction slightly angled downwards
        float angle = Random.Range(-45f, 45f);
        moveDirection = Quaternion.Euler(0, 0, angle) * Vector3.down;

        // Randomize initial rotation
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Move in the random angled downward direction
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        
        CheckOffScreen();
    }

    private void CheckOffScreen()
    {
        if (mainCam == null) return;
        
        // Destroy if it goes below the screen or off the sides
        float camBottom = mainCam.transform.position.y - mainCam.orthographicSize - 2f;
        float camSide = mainCam.orthographicSize * mainCam.aspect + 2f;
        
        if (transform.position.y < camBottom || Mathf.Abs(transform.position.x - mainCam.transform.position.x) > camSide)
        {
            Destroy(gameObject);
        }
    }

    // Player collision is handled in PlayerController.cs
    // Laser collision is handled in Laser.cs

    private void OnDestroy()
    {
        GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
        if (gpm != null) gpm.OnAsteroidDestroyed();
    }
}
