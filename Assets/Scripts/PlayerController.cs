using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Dash Settings")]
    public float dashSpeedMultiplier = 2.5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.5f;

    [Header("Graze Settings")]
    public float grazeRadius = 1.3f;
    public float collisionRadius = 0.6f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float lastShotTime = -999f;
    private Camera mainCam;
    private Vector2 screenBounds;
    
    private bool hasShield = false;
    private GameObject shieldVisual;

    private AudioManager audioMgr;
    private GamePlayManager gpm;

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    private ParticleSystem thrusterParticles;

    [HideInInspector]
    public Color shipColor = Color.white;

    // Graze state
    private System.Collections.Generic.HashSet<GameObject> grazedAsteroids = new System.Collections.Generic.HashSet<GameObject>();

    public float GetDashCooldownNormalized()
    {
        if (dashCooldown <= 0f) return 0f;
        return Mathf.Clamp01(dashCooldownTimer / dashCooldown);
    }

    public float GetDashCooldownSeconds()
    {
        return Mathf.Max(0f, dashCooldownTimer);
    }

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
        audioMgr = FindAnyObjectByType<AudioManager>();
        gpm = FindAnyObjectByType<GamePlayManager>();
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
        thrusterParticles = ps;
        
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        
        // Randomize lifetime, speed, and size for organic motion
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
        main.startColor = new Color(shipColor.r, shipColor.g, shipColor.b, 0.8f);
        
        var emission = ps.emission;
        emission.rateOverTime = 55f; // More dense flame

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 8f; // Narrower cone for rocket exhaust
        shape.radius = 0.05f;
        
        // Point downwards (particles move along local Z axis)
        thruster.transform.localRotation = Quaternion.Euler(90, 0, 0); 

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 9; // Below the ship (order 10)
        renderer.sortingLayerName = "Default";
        
        // Create a custom sprite material using the universally supported Sprites/Default shader
        Material particleMat = new Material(Shader.Find("Sprites/Default"));
        renderer.material = particleMat;

        // Assign a beautiful star sprite as the particle texture via the Texture Sheet Animation module
        Sprite[] starAssets = Resources.LoadAll<Sprite>("Background/Star_3");
        if (starAssets != null && starAssets.Length > 0)
        {
            var ts = ps.textureSheetAnimation;
            ts.enabled = true;
            ts.mode = ParticleSystemAnimationMode.Sprites;
            ts.SetSprite(0, starAssets[0]);
        }

        // Enable Size over Lifetime to taper the tail to a point
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Enable Color over Lifetime to smoothly fade the flame
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorGradient);

        ps.Play();
    }

    private Vector2 GetMovementInput()
    {
        float h = 0f;
        float v = 0f;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) h -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) h += 1f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) v += 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) v -= 1f;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stick = gamepad.leftStick.ReadValue();
            if (Mathf.Abs(stick.x) > 0.19f) h += Mathf.Sign(stick.x);
            if (Mathf.Abs(stick.y) > 0.19f) v += Mathf.Sign(stick.y);

            if (gamepad.dpad.left.isPressed) h -= 1f;
            if (gamepad.dpad.right.isPressed) h += 1f;
            if (gamepad.dpad.up.isPressed) v += 1f;
            if (gamepad.dpad.down.isPressed) v -= 1f;
        }

        return new Vector2(Mathf.Clamp(h, -1f, 1f), Mathf.Clamp(v, -1f, 1f));
    }

    private void Update()
    {
        // Update timers
        dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                // Restore thruster particles to normal ship color
                if (thrusterParticles != null)
                {
                    var main = thrusterParticles.main;
                    main.startColor = new Color(shipColor.r, shipColor.g, shipColor.b, 0.8f);
                    var emission = thrusterParticles.emission;
                    emission.rateOverTime = 55f;
                }
            }
        }

        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        bool dashPressed = false;
        if (keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame) dashPressed = true;
        if (gamepad != null && gamepad.buttonWest.wasPressedThisFrame) dashPressed = true;

        // Check for Dash (Left Shift)
        if (dashPressed && dashCooldownTimer <= 0f && !isDashing && GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            Vector2 inputDir = GetMovementInput();
            Vector2 moveDir = inputDir.normalized;
            if (moveDir == Vector2.zero) moveDir = Vector2.up; // default up

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashDirection = moveDir;

            // Thruster complementary color during dash and spikes particle count
            if (thrusterParticles != null)
            {
                var main = thrusterParticles.main;
                float hc, sc, vc;
                Color.RGBToHSV(shipColor, out hc, out sc, out vc);
                Color dashColor = Color.HSVToRGB((hc + 0.5f) % 1f, 1f, 1f);
                main.startColor = new Color(dashColor.r, dashColor.g, dashColor.b, 1.0f);
                var emission = thrusterParticles.emission;
                emission.rateOverTime = 90f;
            }

            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.05f);
        }

        bool shootPressed = false;
        if (keyboard != null && keyboard.spaceKey.isPressed) shootPressed = true;
        if (gamepad != null && gamepad.buttonSouth.isPressed) shootPressed = true;

        // Shooting — uses Update (not FixedUpdate) so it respects cooldown with Time.time
        if (shootPressed && Time.time >= lastShotTime + shootCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }

        // Sprite flipping for visual direction feedback
        Vector2 currentMoveInput = GetMovementInput();
        if (currentMoveInput.x < 0f) sr.flipX = true;
        else if (currentMoveInput.x > 0f) sr.flipX = false;
    }

    /// <summary>
    /// Handles physics-based movement using arrow keys and dashing.
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 targetPos;

        if (isDashing)
        {
            // Dash movement
            targetPos = rb.position + dashDirection * (moveSpeed * dashSpeedMultiplier) * Time.fixedDeltaTime;
        }
        else
        {
            // Get input from Arrow Keys (or WASD)
            Vector2 inputDir = GetMovementInput();
            float h = inputDir.x * moveSpeed * Time.fixedDeltaTime;
            float v = inputDir.y * moveSpeed * Time.fixedDeltaTime;
            targetPos = rb.position + new Vector2(h, v);
        }

        rb.MovePosition(targetPos);

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -screenBounds.x, screenBounds.x);
        pos.y = Mathf.Clamp(pos.y, -screenBounds.y, screenBounds.y);
        transform.position = pos;

        // Perform Graze detection
        if (!isDashing && GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
            foreach (GameObject ast in asteroids)
            {
                if (ast == null) continue;
                float dist = Vector2.Distance(transform.position, ast.transform.position);
                if (dist <= grazeRadius && dist > collisionRadius)
                {
                    TriggerGraze(ast);
                }
            }
        }
    }

    private void TriggerGraze(GameObject asteroid)
    {
        if (grazedAsteroids.Contains(asteroid)) return;

        grazedAsteroids.Add(asteroid);

        // Limit size of hashset to prevent theoretical leak and clean up nulls
        if (grazedAsteroids.Count > 100)
        {
            grazedAsteroids.RemoveWhere(ast => ast == null);
            if (grazedAsteroids.Count > 100)
            {
                grazedAsteroids.Clear();
                grazedAsteroids.Add(asteroid);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10); // base graze score (will be multiplied by combo)
        }

        if (audioMgr != null) audioMgr.PlayStarCollect(); // Graze sound

        if (gpm != null) gpm.CreateCollectEffect(transform.position); // Graze effect

        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null)
        {
            ui.TriggerGrazeUI();
        }

        Debug.Log("Graze! Near asteroid " + asteroid.name);
    }

    private void Shoot()
    {
        if (laserPrefab == null) return;

        int currentCombo = GameManager.Instance != null ? GameManager.Instance.ComboMultiplier : 1;

        if (currentCombo >= 3)
        {
            // Triple Shot combo weapon upgrade!
            float[] angles = { -15f, 0f, 15f };
            foreach (float angle in angles)
            {
                Vector3 spawnPos = laserSpawnPoint != null
                    ? laserSpawnPoint.position
                    : transform.position + Vector3.up * 0.6f;

                GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                ConfigureLaser(laser);

                Laser laserScript = laser.GetComponent<Laser>();
                if (laserScript != null)
                {
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                    laserScript.SetDirection(dir);
                }
                else
                {
                    Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
                    if (laserRb != null)
                    {
                        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                        laserRb.linearVelocity = dir * laserSpeed;
                    }
                }
            }
        }
        else
        {
            // Normal Single Shot
            Vector3 spawnPos = laserSpawnPoint != null
                ? laserSpawnPoint.position
                : transform.position + Vector3.up * 0.6f;

            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
            ConfigureLaser(laser);

            Laser laserScript = laser.GetComponent<Laser>();
            if (laserScript != null)
            {
                laserScript.SetDirection(Vector2.up);
            }
            else
            {
                Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
                if (laserRb != null)
                    laserRb.linearVelocity = Vector2.up * laserSpeed;
            }
        }

        if (audioMgr != null) audioMgr.PlayLaser();
    }

    private void ConfigureLaser(GameObject laser)
    {
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
            laserSr.color = shipColor; // Tint laser to match the ship color theme!
        }
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
            if (isDashing) return; // Invincible during Dash!

            if (gpm != null)
            {
                gpm.OnAsteroidDestroyed();
                gpm.CreateExplosionEffect(collision.transform.position);
            }
            
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
