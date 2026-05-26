using UnityEngine;

/// <summary>
/// Controls star behavior: rotation, bobbing animation, and collection.
/// Stars are collectible items that add points to the player's score.
/// </summary>
public class Star : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Visual Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minPulse = 0.8f;
    [SerializeField] private float maxPulse = 1.2f;

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private float timeOffset;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameActive) return;
        AnimateStar();
    }

    /// <summary>
    /// Animates the star with bobbing motion and pulsing scale.
    /// </summary>
    private void AnimateStar()
    {
        // Bobbing motion
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed + timeOffset) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Rotation
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Pulsing scale
        float pulse = Mathf.Lerp(minPulse, maxPulse, (Mathf.Sin(Time.time * pulseSpeed + timeOffset) + 1) / 2);
        transform.localScale = Vector3.one * pulse;
    }

    /// <summary>
    /// Called when player collects the star.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance?.PlayStarCollect();
            GameManager.Instance.CollectStar();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when star goes off screen. Respawns at a new random position.
    /// </summary>
    private void OnBecameInvisible()
    {
        RespawnAtRandomPosition();
    }

    /// <summary>
    /// Respawns the star at a new random position within the screen.
    /// </summary>
    public void RespawnAtRandomPosition()
    {
        if (!gameObject.activeInHierarchy) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 minPos = cam.ViewportToWorldPoint(new Vector3(0.1f, 0.1f, 0));
        Vector3 maxPos = cam.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, 0));

        float randomX = Random.Range(minPos.x, maxPos.x);
        float randomY = Random.Range(minPos.y, maxPos.y);

        transform.position = new Vector3(randomX, randomY, 0);
        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
}
