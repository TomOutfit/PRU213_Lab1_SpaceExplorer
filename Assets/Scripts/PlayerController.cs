using UnityEngine;

/// <summary>
/// Controls the player spaceship: movement via arrow keys, shooting lasers with Space,
/// and collision responses with asteroids and stars.
/// This script handles the core "Spaceship movement and shooting" requirement.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotateSpeed = 300f;

    [Header("Shooting")]
    public GameObject laserPrefab;
    public Transform laserSpawnPoint;
    public float shootCooldown = 0.2f;
    public float laserSpeed = 12f;

    [Header("Ship Appearance")]
    public Sprite[] shipSprites;

    [Header("Weapons")]
    public Sprite[] weaponSprites;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float lastShotTime = -999f;
    private Camera mainCam;
    private Vector2 screenBounds;
    
    private bool hasShield = false;
    private GameObject shieldVisual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) 
        {
            sr.sortingOrder = 10;
            sr.enabled = true;
        }

        // Load dynamically from Resources if the array is empty or not fully populated
        if (shipSprites == null || shipSprites.Length == 0)
        {
            shipSprites = Resources.LoadAll<Sprite>("Ship");
        }

        if (shipSprites != null && shipSprites.Length > 0)
        {
            sr.sprite = shipSprites[Random.Range(0, shipSprites.Length)];
        }
    }

    private void Start()
    {
        mainCam = Camera.main;
        UpdateScreenBounds();
        CreateEngineThruster();
    }

    private void UpdateScreenBounds()
    {
        if (mainCam == null) mainCam = Camera.main;
        float vertExtent = mainCam.orthographicSize;
        float horzExtent = vertExtent * mainCam.aspect;
        screenBounds = new Vector2(horzExtent + 1f, vertExtent + 1f);
    }

    private void CreateEngineThruster()
    {
        GameObject thruster = new GameObject("EngineThruster");
        thruster.transform.SetParent(this.transform);
        // Position at the bottom of the ship
        thruster.transform.localPosition = new Vector3(0, -0.6f, 0); 
        
        ParticleSystem ps = thruster.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.2f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange fire
        
        var emission = ps.emission;
        emission.rateOverTime = 30;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;
        // Point downwards (particles move along local Z axis)
        thruster.transform.localRotation = Quaternion.Euler(90, 0, 0); 

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 9; // Below the ship (order 10)
        renderer.sortingLayerName = "Default";

        ps.Play();
    }

    private void Update()
    {
        // Shooting — uses Update (not FixedUpdate) so it respects cooldown with Time.time
        if (Input.GetKey(KeyCode.Space) && Time.time >= lastShotTime + shootCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }

        // Sprite flipping for visual direction feedback
        float h = Input.GetAxisRaw("Horizontal");
        if (h < 0f) sr.flipX = true;
        else if (h > 0f) sr.flipX = false;
    }

    /// <summary>
    /// Handles physics-based movement using arrow keys.
    /// </summary>
    private void FixedUpdate()
    {
        // Get input from Arrow Keys (or WASD)
        float h = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.fixedDeltaTime;
        float v = Input.GetAxisRaw("Vertical") * moveSpeed * Time.fixedDeltaTime;

        // Apply movement
        Vector2 targetPos = rb.position + new Vector2(h, v);
        rb.MovePosition(targetPos);

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -screenBounds.x, screenBounds.x);
        pos.y = Mathf.Clamp(pos.y, -screenBounds.y, screenBounds.y);
        transform.position = pos;
    }

    private void Shoot()
    {
        if (laserPrefab == null) return;

        Vector3 spawnPos = laserSpawnPoint != null
            ? laserSpawnPoint.position
            : transform.position + Vector3.up * 0.6f;

        GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer laserSr = laser.GetComponent<SpriteRenderer>();
        if (laserSr == null) laserSr = laser.GetComponentInChildren<SpriteRenderer>();
        if (laserSr != null)
        {
#if UNITY_EDITOR
            if (weaponSprites == null || weaponSprites.Length == 0)
            {
                string[] weaponPaths = new string[] {
                    "Assets/Sprites/Weapons/Laser_Blue.png",
                    "Assets/Sprites/Weapons/Laser_Green.png",
                    "Assets/Sprites/Weapons/Laser_Orange.png",
                    "Assets/Sprites/Weapons/Laser_Purple.png",
                    "Assets/Sprites/Weapons/Laser_Red.png"
                };
                weaponSprites = new Sprite[weaponPaths.Length];
                for (int i = 0; i < weaponPaths.Length; i++)
                {
                    weaponSprites[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(weaponPaths[i]);
                }
            }
#endif
            if (weaponSprites != null && weaponSprites.Length > 0)
            {
                Sprite chosenWeapon = weaponSprites[Random.Range(0, weaponSprites.Length)];
                if (chosenWeapon != null)
                {
                    laserSr.sprite = chosenWeapon;
                }
            }
        }

        Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
        if (laserRb != null)
            laserRb.linearVelocity = Vector2.up * laserSpeed;
            
        AudioManager audioMgr = FindAnyObjectByType<AudioManager>();
        if (audioMgr != null) audioMgr.PlayLaser();
    }

    public void ActivateShield()
    {
        hasShield = true;
        // Basic visual feedback for shield (change color temporarily)
        sr.color = new Color(0.5f, 0.8f, 1f, 1f);
        Debug.Log("Shield Activated!");
    }

    public void ActivateRapidFire()
    {
        StartCoroutine(RapidFireRoutine());
        Debug.Log("Rapid Fire Activated!");
    }

    private System.Collections.IEnumerator RapidFireRoutine()
    {
        float originalCooldown = shootCooldown;
        shootCooldown = 0.05f; // extremely fast shooting
        yield return new WaitForSeconds(5f);
        shootCooldown = originalCooldown;
        Debug.Log("Rapid Fire Deactivated!");
    }

    /// <summary>
    /// Handles collision logic between the spaceship, asteroids, and stars.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        if (collision.gameObject.CompareTag("Asteroid"))
        {
            GamePlayManager gpm = FindAnyObjectByType<GamePlayManager>();
            if (gpm != null)
            {
                gpm.OnAsteroidDestroyed();
                gpm.CreateExplosionEffect(collision.transform.position);
            }
            
            AudioManager audioMgr = FindAnyObjectByType<AudioManager>();

            if (hasShield)
            {
                // Shield breaks, asteroid is destroyed, player takes no damage
                hasShield = false;
                sr.color = Color.white; // Reset color
                Destroy(collision.gameObject);
                
                if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.1f);
                
                // Play sound
                if (audioMgr != null) audioMgr.PlayAsteroidDestroy();
                
                Debug.Log("Shield absorbed the hit!");
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(-GameManager.Instance.penaltyPerAsteroid);
                GameManager.Instance.LoseLife();
            }

            if (audioMgr != null) audioMgr.PlayAsteroidDestroy();
            Destroy(collision.gameObject);
            
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.3f);

            // Brief flash before respawning
            StartCoroutine(RespawnAfterHit());
        }
        // PowerUp collision is handled by PowerUp.cs using OnTriggerEnter2D
    }

    private System.Collections.IEnumerator RespawnAfterHit()
    {
        // Flash red effect
        for (int i = 0; i < 4; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            sr.enabled = true;
            sr.color = Color.white;
        }
    }
}
