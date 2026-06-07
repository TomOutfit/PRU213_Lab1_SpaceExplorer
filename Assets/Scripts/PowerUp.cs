using UnityEngine;

/// <summary>
/// Collectible powerup: drops downwards from the top of the screen.
/// Grants bonus points and an optional shield.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { ScoreBonus, Shield, RapidFire }
    
    [Header("Settings")]
    public PowerUpType type = PowerUpType.ScoreBonus;
    public float fallSpeed = 3f;
    public float rotationSpeed = 50f;
    public int bonusPoints = 50;

    private Camera mainCam;
    private Vector3 moveDirection;
    private Vector3 initialScale;

    private void Start()
    {
        mainCam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 100;
            
            // Tint based on power-up type for instant readability!
            if (type == PowerUpType.Shield)
                sr.color = new Color(0f, 0.8f, 1f); // Shield is Cool Cyan
            else if (type == PowerUpType.RapidFire)
                sr.color = new Color(1f, 0.2f, 0.4f); // Rapid Fire is Hot Magenta
            else
                sr.color = new Color(1f, 0.9f, 0.2f); // Score Bonus is Golden Yellow
                
            sr.enabled = true;
        }
        
        float angle = Random.Range(-45f, 45f);
        moveDirection = Quaternion.Euler(0, 0, angle) * Vector3.down;
        initialScale = transform.localScale;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        
        // Thêm hiệu ứng nhấp nháy (phóng to thu nhỏ) để dễ nhìn hơn
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.25f;
        transform.localScale = initialScale * pulse;
    }

    private void FixedUpdate()
    {
        transform.Translate(moveDirection * fallSpeed * Time.fixedDeltaTime, Space.World);
        
        CheckOffScreen();
    }

    private void CheckOffScreen()
    {
        if (mainCam == null) return;
        
        float camBottom = mainCam.transform.position.y - mainCam.orthographicSize - 2f;
        float camSide = mainCam.orthographicSize * mainCam.aspect + 2f;
        
        if (transform.position.y < camBottom || Mathf.Abs(transform.position.x - mainCam.transform.position.x) > camSide)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(bonusPoints);

            AudioManager audioMgr = FindAnyObjectByType<AudioManager>();
            if (audioMgr != null) audioMgr.PlayStarCollect();

            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                if (type == PowerUpType.Shield)
                {
                    pc.ActivateShield();
                }
                else if (type == PowerUpType.RapidFire)
                {
                    pc.ActivateRapidFire();
                }
            }

            GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
            if (gpm != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Color collectColor = sr != null ? sr.color : Color.yellow;
                gpm.CreateCollectEffect(transform.position, collectColor);
            }

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
        if (gpm != null) gpm.OnStarCollected();
    }
}
