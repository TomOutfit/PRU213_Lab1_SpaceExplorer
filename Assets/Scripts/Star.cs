using UnityEngine;

/// <summary>
/// Collectible star: rotates, bobs up and down, and pulses in scale.
/// Destroyed on collision with the player; triggers score gain.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Star : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    public float pulseSpeed = 2f;
    public float minPulse = 0.8f;
    public float maxPulse = 1.2f;

    private Vector3 startPos;
    private float timeOffset;
    private Color starColor = Color.white;

    public Color StarColor => starColor;

    private void Awake()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // Randomize star color out of: Gold (Yellow), Cyan (Blue), Magenta (Pink), Lime (Green)
        Color[] possibleColors = {
            new Color(1f, 0.9f, 0.2f),  // Gold
            new Color(0f, 0.8f, 1f),   // Cyan
            new Color(1f, 0.2f, 0.6f),  // Magenta
            new Color(0.2f, 1f, 0.4f)   // Lime
        };
        starColor = possibleColors[Random.Range(0, possibleColors.Length)];

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = starColor;
        }
    }

    private void Update()
    {
        // Gentle rotation
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float bob = Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;
        transform.position = startPos + Vector3.up * bob;

        // Pulse scale
        float pulse = Mathf.Lerp(minPulse, maxPulse,
            (Mathf.Sin((Time.time + timeOffset) * pulseSpeed) + 1f) * 0.5f);
        transform.localScale = Vector3.one * pulse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(GameManager.Instance.pointsPerStar);

            GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
            if (gpm != null)
            {
                gpm.OnStarCollected();
                gpm.CreateCollectEffect(transform.position, starColor);
            }

            Destroy(gameObject);
        }
    }
}
