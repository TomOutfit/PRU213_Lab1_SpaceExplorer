using UnityEngine;

/// <summary>
/// Collectible powerup: drops downwards from the top of the screen.
/// Grants bonus points and an optional shield.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { ScoreBonus, Shield }
    
    [Header("Settings")]
    public PowerUpType type = PowerUpType.ScoreBonus;
    public float fallSpeed = 3f;
    public float rotationSpeed = 50f;
    public int bonusPoints = 50;

    private Camera mainCam;
    private Vector3 moveDirection;

    private void Start()
    {
        mainCam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 100;
            sr.color = Color.white;
            sr.enabled = true;
        }
        
        float angle = Random.Range(-45f, 45f);
        moveDirection = Quaternion.Euler(0, 0, angle) * Vector3.down;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
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
            if (pc != null && type == PowerUpType.Shield)
            {
                pc.ActivateShield();
            }

            GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
            if (gpm != null) gpm.CreateCollectEffect(transform.position);

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
        if (gpm != null) gpm.OnStarCollected();
    }
}
