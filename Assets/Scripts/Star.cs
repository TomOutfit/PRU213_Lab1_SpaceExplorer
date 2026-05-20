using UnityEngine;

/// <summary>
/// Enhanced star collectible with multiple types and visual indicators.
/// Different star types provide different point values.
/// </summary>
public class Star : MonoBehaviour
{
    [Header("Star Configuration")]
    public StarType starType = StarType.Blue;
    public float speed = 3f;
    public float destroyYPosition = -6f;

    [Header("Visual Effects")]
    public float rotationSpeed = 90f;
    public bool useGlowEffect = true;
    public Color glowColor = Color.yellow;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private SpriteRenderer _spriteRenderer;
    private Vector3 _direction;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        SetupStar();
    }

    void Start()
    {
        // Slight random horizontal drift
        _direction = new Vector3(Random.Range(-0.2f, 0.2f), -1f, 0f).normalized;
    }

    void Update()
    {
        MoveStar();
        RotateStar();
        CheckBounds();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    private void SetupStar()
    {
        // Set color based on star type
        if (_spriteRenderer != null)
        {
            Color color = starType switch
            {
                StarType.Blue => new Color(0.3f, 0.5f, 1f),     // Light blue
                StarType.Silver => new Color(0.75f, 0.75f, 0.75f), // Silver
                StarType.Gold => new Color(1f, 0.84f, 0f),      // Gold
                _ => Color.white
            };

            _spriteRenderer.color = color;

            // Add glow effect
            if (useGlowEffect)
            {
                _spriteRenderer.material = new Material(_spriteRenderer.material);
                _spriteRenderer.material.SetColor("_GlowColor", glowColor);
            }
        }

        // Set points value (visual only - actual scoring is done by ScoreManager)
        gameObject.tag = "Star";
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void MoveStar()
    {
        transform.position += _direction * speed * Time.deltaTime;
    }

    private void RotateStar()
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
            CollectStar();
        }
    }

    private void CollectStar()
    {
        // Award points based on star type
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddStarScore(starType);
        }

        // Play collection sound
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.twoToneClip);
        }

        // Create collection particle effect
        CreateCollectEffect();

        Destroy(gameObject);
    }

    private void CreateCollectEffect()
    {
        // Simple visual feedback - can be enhanced with particle system
        GameObject effect = new GameObject("StarCollectEffect");
        effect.transform.position = transform.position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.sprite = _spriteRenderer.sprite;
        sr.color = _spriteRenderer.color;
        sr.sortingOrder = 100;

        // Animate and destroy
        StartCoroutine(AnimateCollectEffect(effect));
    }

    private System.Collections.IEnumerator AnimateCollectEffect(GameObject effect)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 1.5f;
        Color startColor = effect.GetComponent<SpriteRenderer>().color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            effect.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            Color c = Color.Lerp(startColor, endColor, t);
            effect.GetComponent<SpriteRenderer>().color = c;

            yield return null;
        }

        Destroy(effect);
    }

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Set the star type and update visuals accordingly
    /// </summary>
    public void SetStarType(StarType type)
    {
        starType = type;
        SetupStar();
    }

    /// <summary>
    /// Get the point value for this star type
    /// </summary>
    public int GetPointsValue()
    {
        return starType switch
        {
            StarType.Blue => 10,
            StarType.Silver => 20,
            StarType.Gold => 30,
            _ => 10
        };
    }
}
