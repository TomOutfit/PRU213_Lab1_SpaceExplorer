using UnityEngine;

/// <summary>
/// Controls the player's spaceship: movement, shooting, and collision handling.
/// Uses Rigidbody2D physics for smooth movement and handles input for controls.
/// Supports multiple ship types with different stats.
/// Note: Uses legacy Input system for maximum compatibility.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Ship Settings (from ShipData)")]
    [SerializeField] private ShipData shipData;

    [Header("Current Ship Stats")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float laserSpeed = 15f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float boundaryPadding = 0.5f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject laserPrefab;

    [Header("Visual Effects")]
    [SerializeField] private GameObject engineEffect;
    [SerializeField] private ParticleSystem engineParticles;

    // Input state
    private Vector2 moveDirection;
    private float horizontalInput;
    private float verticalInput;
    private int currentLives;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private float nextFireTime = 0f;

    // Public properties
    public int CurrentLives => currentLives;
    public float CurrentSpeed => moveSpeed;
    public string CurrentShipName => shipData != null ? shipData.shipName : "Unknown";

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        LoadShipData();
        InitializePlayer();
    }

    /// <summary>
    /// Loads the selected ship data from PlayerPrefs or uses default.
    /// </summary>
    private void LoadShipData()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedShip", 0);

        // Try to load from ScriptableObject
        ShipData[] allShips = Resources.LoadAll<ShipData>("ScriptableObjects/Ships");

        if (allShips != null && allShips.Length > 0)
        {
            if (selectedIndex < allShips.Length)
            {
                shipData = allShips[selectedIndex];
            }
            else
            {
                shipData = allShips[0];
            }
        }

        // Apply ship stats
        if (shipData != null)
        {
            moveSpeed = shipData.speed;
            rotationSpeed = shipData.rotationSpeed;
            fireRate = shipData.fireRate;
            damage = shipData.damage;
            laserSpeed = shipData.laserSpeed;
            maxLives = shipData.maxLives;
            spriteRenderer.color = shipData.shipColor;
            transform.localScale = Vector3.one * shipData.size * 2f;
        }
    }

    /// <summary>
    /// Initializes player with starting values.
    /// </summary>
    private void InitializePlayer()
    {
        currentLives = maxLives;
        moveDirection = Vector2.up;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerLives(maxLives);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;

        HandleInput();
        HandleShooting();
        ClampPosition();
        HandleInvulnerability();
        UpdateEngineEffect();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;

        MovePlayer();
    }

    /// <summary>
    /// Captures player input using legacy Input system.
    /// Supports both Arrow Keys and WASD for movement.
    /// </summary>
    private void HandleInput()
    {
        // Get input from both arrow keys and WASD
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate normalized movement direction
        if (horizontalInput != 0 || verticalInput != 0)
        {
            moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    /// <summary>
    /// Moves the spaceship based on input and handles rotation towards movement direction.
    /// </summary>
    private void MovePlayer()
    {
        Vector2 movement = moveDirection * moveSpeed;
        rb.linearVelocity = movement;

        // Rotate ship towards movement direction
        if (movement != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Handles shooting laser projectiles based on fire rate.
    /// Uses Space key for shooting.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireLaser();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Instantiates laser projectiles from the fire point.
    /// </summary>
    private void FireLaser()
    {
        if (laserPrefab == null || firePoint == null) return;

        int projectileCount = shipData != null ? shipData.GetProjectileCount() : 1;
        float spreadAngle = 10f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = 0f;
            if (projectileCount > 1)
            {
                angleOffset = Mathf.Lerp(-spreadAngle, spreadAngle, (float)i / (projectileCount - 1));
            }

            GameObject laser = Instantiate(laserPrefab, firePoint.position,
                Quaternion.Euler(0, 0, transform.eulerAngles.z - 90f + angleOffset));

            Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
            if (laserRb != null)
            {
                Vector2 fireDirection = transform.up;
                laserRb.linearVelocity = fireDirection * laserSpeed;
            }

            Laser laserScript = laser.GetComponent<Laser>();
            if (laserScript != null)
            {
                laserScript.SetDamage(damage);
            }
        }

        AudioManager audioManager = FindObjectOfType<AudioManager>();
        audioManager?.PlayLaser();
    }

    /// <summary>
    /// Keeps the player within screen boundaries.
    /// </summary>
    private void ClampPosition()
    {
        if (Camera.main == null) return;

        Vector3 pos = transform.position;
        Vector3 minPos = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxPos = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        pos.x = Mathf.Clamp(pos.x, minPos.x + boundaryPadding, maxPos.x - boundaryPadding);
        pos.y = Mathf.Clamp(pos.y, minPos.y + boundaryPadding, maxPos.y - boundaryPadding);
        transform.position = pos;
    }

    /// <summary>
    /// Updates invulnerability state after being hit with flicker effect.
    /// </summary>
    private void HandleInvulnerability()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;

            // Flicker effect
            spriteRenderer.enabled = Mathf.Sin(Time.time * 20f) > 0;

            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
                spriteRenderer.enabled = true;
            }
        }
    }

    /// <summary>
    /// Updates engine particle effect based on movement.
    /// </summary>
    private void UpdateEngineEffect()
    {
        if (engineEffect != null)
        {
            engineEffect.SetActive(moveDirection != Vector2.zero);
        }
    }

    /// <summary>
    /// Handles collision with stars and asteroids.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isInvulnerable) return;

        if (other.CompareTag("Star"))
        {
            other.gameObject.SetActive(false);
            GameManager.Instance?.CollectStar();
        }
        else if (other.CompareTag("Asteroid"))
        {
            TakeDamage();
            other.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Applies damage to the player and handles invulnerability.
    /// </summary>
    public void TakeDamage()
    {
        if (isInvulnerable) return;

        currentLives--;
        GameManager.Instance?.UpdateLives(currentLives);

        AudioManager audioManager = FindObjectOfType<AudioManager>();
        audioManager?.PlayAsteroidHit();

        if (currentLives > 0)
        {
            isInvulnerable = true;
            invulnerabilityTimer = 2f;
        }
        else
        {
            GameManager.Instance?.EndGame();
        }
    }

    /// <summary>
    /// Adds bonus life to the player.
    /// </summary>
    public void AddLife()
    {
        currentLives++;
        GameManager.Instance?.UpdateLives(currentLives);
    }
}
