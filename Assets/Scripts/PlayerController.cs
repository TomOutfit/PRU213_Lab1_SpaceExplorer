using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Enhanced player controller supporting:
/// - Multiple ship types with unique stats
/// - Power-up system integration
/// - Shield visual effects
/// - Triple shot and piercing shot
/// - Damage states
/// </summary>
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float xBound = 8f;
    public float yBound = 4.5f;

    [Header("Shooting Settings")]
    public GameObject laserPrefab;
    public GameObject tripleLaserPrefab;
    public GameObject piercingLaserPrefab;
    public Transform firePoint;
    public Transform firePointLeft;
    public Transform firePointRight;
    public float baseFireRate = 0.25f;

    [Header("Health Settings")]
    public int baseLives = 3;
    public GameObject[] lifeIcons;

    [Header("Ship Data")]
    public ShipData currentShipData;

    [Header("Shield Settings")]
    public GameObject shieldEffect;
    public float shieldFlashInterval = 0.2f;

    [Header("Visual Effects")]
    public Sprite[] damageSprites;
    public GameObject explosionEffect;

    [Header("Audio")]
    public AudioClip damageClip;
    public AudioClip shieldHitClip;
    public AudioClip powerUpClip;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float _currentMoveSpeed;
    private float _currentFireRate;
    private int _currentLives;
    private int _currentDamageState = 0;

    private float _nextFireTime = 0f;
    private bool _isInvulnerable = false;
    private float _invulnerabilityDuration = 2f;
    private float _invulnerabilityTimer = 0f;

    private bool _isTripleShotActive = false;
    private bool _isPiercingShotActive = false;

    private SpriteRenderer _spriteRenderer;
    private GameObject _activeShieldEffect;

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public int CurrentLives => _currentLives;
    public bool IsInvulnerable => _isInvulnerable;
    public bool IsShieldActive => PowerUpManager.Instance != null && PowerUpManager.Instance.IsShieldActive;
    public bool IsTripleShotActive => _isTripleShotActive || (currentShipData != null && currentShipData.hasTripleShot);
    public bool IsPiercingShotActive => _isPiercingShotActive || (currentShipData != null && currentShipData.hasPiercingShot);

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeShipData();
    }

    void Start()
    {
        _currentLives = currentShipData != null ? currentShipData.startingLives : baseLives;
        UpdateLifeUI();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        UpdateInvulnerability();
        UpdatePowerUpState();
        UpdateShieldEffect();
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void InitializeShipData()
    {
        // Get selected ship from ShipSelectionController
        if (ShipSelectionController.Instance != null)
        {
            currentShipData = ShipSelectionController.Instance.GetSelectedShipData();
        }

        // Apply ship stats
        if (currentShipData != null)
        {
            // Calculate stats from ship data
            _currentMoveSpeed = currentShipData.GetSpeed();
            _currentFireRate = baseFireRate / currentShipData.GetFireRate();

            // Update sprite
            if (currentShipData.shipSprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = currentShipData.shipSprite;
                _spriteRenderer.color = currentShipData.shipColor;
            }
        }
        else
        // Fallback to base values
        {
            _currentMoveSpeed = baseMoveSpeed;
            _currentFireRate = baseFireRate;
        }

        // Initialize power-up manager if needed
        if (PowerUpManager.Instance == null)
        {
            GameObject powerUpManagerObj = new GameObject("PowerUpManager");
            powerUpManagerObj.AddComponent<PowerUpManager>();
        }
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void HandleMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) horizontalInput += 1f;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) verticalInput -= 1f;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) verticalInput += 1f;
        }

        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;

        // Apply speed multiplier from power-ups
        float speedMultiplier = 1f;
        if (PowerUpManager.Instance != null)
        {
            speedMultiplier = PowerUpManager.Instance.SpeedMultiplier;
        }

        transform.position += moveDirection * _currentMoveSpeed * speedMultiplier * Time.deltaTime;

        // Clamp position within bounds
        float clampedX = Mathf.Clamp(transform.position.x, -xBound, xBound);
        float clampedY = Mathf.Clamp(transform.position.y, -yBound, yBound);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    // -------------------------------------------------------------------------
    // Shooting
    // -------------------------------------------------------------------------

    private void HandleShooting()
    {
        if (Keyboard.current == null || !Keyboard.current.spaceKey.isPressed) return;
        if (Time.time < _nextFireTime) return;

        // Calculate fire rate with power-up modifier
        float fireRateMultiplier = 1f;
        if (PowerUpManager.Instance != null)
        {
            fireRateMultiplier = PowerUpManager.Instance.FireRateMultiplier;
        }

        _nextFireTime = Time.time + (_currentFireRate / fireRateMultiplier);

        // Determine which laser prefab to use
        GameObject laserToUse = laserPrefab;

        if (IsTripleShotActive)
        {
            FireTripleShot();
            return;
        }

        if (IsPiercingShotActive && piercingLaserPrefab != null)
        {
            laserToUse = piercingLaserPrefab;
        }

        if (laserToUse != null && firePoint != null)
        {
            GameObject laser = Instantiate(laserToUse, firePoint.position, firePoint.rotation);

            // Set piercing property
            if (IsPiercingShotActive)
            {
                Laser laserScript = laser.GetComponent<Laser>();
                if (laserScript != null)
                {
                    laserScript.isPiercing = true;
                }
            }

            PlayLaserSound();
        }
    }

    private void FireTripleShot()
    {
        if (laserPrefab == null) return;

        // Fire center laser
        if (firePoint != null)
        {
            Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
        }

        // Fire left laser
        if (firePointLeft != null)
        {
            Instantiate(laserPrefab, firePointLeft.position, firePointLeft.rotation);
        }

        // Fire right laser
        if (firePointRight != null)
        {
            Instantiate(laserPrefab, firePointRight.position, firePointRight.rotation);
        }

        PlayLaserSound();
    }

    private void PlayLaserSound()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.laserClip);
        }
    }

    // -------------------------------------------------------------------------
    // Power-Up State
    // -------------------------------------------------------------------------

    private void UpdatePowerUpState()
    {
        if (PowerUpManager.Instance == null) return;

        _isTripleShotActive = PowerUpManager.Instance.IsTripleShotActive;
        _isPiercingShotActive = PowerUpManager.Instance.IsPiercingShotActive;
    }

    private void UpdateShieldEffect()
    {
        if (IsShieldActive)
        {
            if (_activeShieldEffect == null && shieldEffect != null)
            {
                _activeShieldEffect = Instantiate(shieldEffect, transform.position, Quaternion.identity, transform);
            }

            // Flash effect
            if (_activeShieldEffect != null)
            {
                float flash = Mathf.Sin(Time.time * (1f / shieldFlashInterval) * Mathf.PI) * 0.5f + 0.5f;
                SpriteRenderer shieldRenderer = _activeShieldEffect.GetComponent<SpriteRenderer>();
                if (shieldRenderer != null)
                {
                    Color c = shieldRenderer.color;
                    c.a = 0.3f + flash * 0.4f;
                    shieldRenderer.color = c;
                }
            }
        }
        else
        {
            if (_activeShieldEffect != null)
            {
                Destroy(_activeShieldEffect);
                _activeShieldEffect = null;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Damage & Health
    // -------------------------------------------------------------------------

    private void UpdateInvulnerability()
    {
        if (!_isInvulnerable) return;

        _invulnerabilityTimer -= Time.deltaTime;

        // Flash effect during invulnerability
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = Mathf.Sin(Time.time * 10f) > 0;
        }

        if (_invulnerabilityTimer <= 0)
        {
            EndInvulnerability();
        }
    }

    private void BeginInvulnerability()
    {
        _isInvulnerable = true;
        _invulnerabilityTimer = _invulnerabilityDuration;
    }

    private void EndInvulnerability()
    {
        _isInvulnerable = false;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Called when player takes damage from enemy collision or enemy bullet
    /// </summary>
    public void OnEnemyCollision(int damage = 1)
    {
        if (_isInvulnerable) return;

        // Check shield
        if (IsShieldActive)
        {
            // Shield absorbs the hit
            if (shieldHitClip != null && GameManager.Instance != null)
            {
                GameManager.Instance.PlaySound(shieldHitClip);
            }
            return;
        }

        _currentLives--;
        UpdateLifeUI();

        // Deduct score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPlayerDamaged();
        }

        // Play damage sound
        if (damageClip != null && GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(damageClip);
        }

        // Update damage sprite
        UpdateDamageState();

        if (_currentLives > 0)
        {
            BeginInvulnerability();
        }
        else
        {
            // Game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(gameObject);
            }
        }
    }

    /// <summary>
    /// Adds an extra life (up to max 5)
    /// </summary>
    public void AddLife()
    {
        if (_currentLives < 5)
        {
            _currentLives++;
            UpdateLifeUI();

            if (powerUpClip != null && GameManager.Instance != null)
            {
                GameManager.Instance.PlaySound(powerUpClip);
            }
        }
    }

    private void UpdateLifeUI()
    {
        if (lifeIcons == null || lifeIcons.Length == 0) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < _currentLives);
            }
        }
    }

    private void UpdateDamageState()
    {
        if (damageSprites == null || damageSprites.Length == 0) return;
        if (_spriteRenderer == null) return;

        int stateIndex = Mathf.Min(_currentDamageState, damageSprites.Length - 1);
        if (damageSprites[stateIndex] != null)
        {
            _spriteRenderer.sprite = damageSprites[stateIndex];
        }

        _currentDamageState = Mathf.Min(_currentDamageState + 1, damageSprites.Length - 1);
    }

    // -------------------------------------------------------------------------
    // Collision Detection
    // -------------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Asteroid"))
        {
            OnEnemyCollision();
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Enemy"))
        {
            OnEnemyCollision();
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Star"))
        {
            // Handled by Star script
        }
        else if (collision.CompareTag("PowerUp"))
        {
            // Handled by PowerUpItem script
        }
    }
}
