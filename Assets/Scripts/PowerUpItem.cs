using UnityEngine;

/// <summary>
/// Individual power-up collectible item.
/// Spawns randomly and grants power-up effects when collected by the player.
/// </summary>
public class PowerUpItem : MonoBehaviour
{
    [Header("Power-Up Configuration")]
    public PowerUpType powerUpType = PowerUpType.SpeedBoost;
    public float fallSpeed = 2f;
    public float rotationSpeed = 90f;
    public float destroyYPosition = -6f;

    [Header("Visual")]
    public Sprite[] powerUpSprites;
    public Color[] powerUpColors;

    [Header("Audio")]
    public AudioClip collectSound;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _direction;

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
        // Random slight horizontal drift
        _direction = new Vector3(Random.Range(-0.3f, 0.3f), -1f, 0f).normalized;
    }

    void Update()
    {
        MovePowerUp();
        RotatePowerUp();
        CheckBounds();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    private void SetupVisuals()
    {
        if (_spriteRenderer == null) return;

        // Assign sprite and color based on power-up type
        int typeIndex = (int)powerUpType;
        if (typeIndex < powerUpSprites.Length && powerUpSprites[typeIndex] != null)
        {
            _spriteRenderer.sprite = powerUpSprites[typeIndex];
        }

        if (typeIndex < powerUpColors.Length)
        {
            _spriteRenderer.color = powerUpColors[typeIndex];
        }
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void MovePowerUp()
    {
        transform.position += _direction * fallSpeed * Time.deltaTime;
    }

    private void RotatePowerUp()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void CheckBounds()
    {
        if (transform.position.y < destroyYPosition)
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
            CollectPowerUp();
        }
    }

    private void CollectPowerUp()
    {
        // Activate the power-up
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ActivatePowerUp(powerUpType);
        }

        // Add score bonus
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPowerUpScore();
        }

        // Add coins if coin bonus power-up
        if (powerUpType == PowerUpType.CoinBonus)
        {
            if (ShipSelectionController.Instance != null)
            {
                ShipSelectionController.Instance.AddCoins(10);
            }
        }

        // Add extra life if life boost power-up
        if (powerUpType == PowerUpType.LifeBoost)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.AddLife();
            }
        }

        // Play sound
        if (collectSound != null && GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(collectSound);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.twoToneClip);
        }

        Destroy(gameObject);
    }
}
