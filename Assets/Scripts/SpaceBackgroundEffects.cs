using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A beautiful, high-performance, dynamic parallax space background system.
/// Automatically disables static background images in the scene, scales to fit any resolution,
/// manages dual nebula layers (with hue shifting), twinkling starfields, and shooting stars.
/// </summary>
public class SpaceBackgroundEffects : MonoBehaviour
{
    public static SpaceBackgroundEffects Instance { get; private set; }

    [Header("Scrolling Speeds")]
    public float baseNebulaSpeed = 0.04f;
    public float glowNebulaSpeed = 0.08f;
    public float hueShiftSpeed = 0.05f;

    [Header("Star Settings")]
    public int totalStars = 50;
    public float farStarsSpeed = 0.12f;
    public float midStarsSpeed = 0.25f;
    public float nearStarsSpeed = 0.5f;

    [Header("Shooting Stars")]
    public float minSpawnInterval = 6f;
    public float maxSpawnInterval = 12f;
    public float shootingStarSpeed = 25f;

    // Static speeds for reactivity
    public static float ScrollSpeedMultiplier = 1f;
    public static float TargetScrollSpeedMultiplier = 1f;

    private Camera mainCam;
    private float camHeight;
    private float camWidth;

    // Sprite resources loaded at runtime
    private Sprite baseNebulaSprite;
    private Sprite glowNebulaSprite;
    private Sprite[] starSprites;

    // Background layer instances
    private List<Transform> baseNebulaTransforms = new List<Transform>();
    private List<Transform> glowNebulaTransforms = new List<Transform>();
    private List<SpriteRenderer> glowNebulaRenderers = new List<SpriteRenderer>();

    private float baseNebulaWorldHeight;
    private float glowNebulaWorldHeight;
    private float baseHue;

    // Twinkling stars class
    private class TwinkleStar
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public float baseSpeed;
        public float baseScale;
        public float twinkleSpeed;
        public float phaseOffset;
        public float minAlpha;
        public float maxAlpha;
    }
    private List<TwinkleStar> twinklingStars = new List<TwinkleStar>();

    // Shooting star timer
    private float shootingStarTimer;
    private float nextShootingStarTime;
    private PlayerController player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        baseHue = Random.value; // Randomize background color palette on start

        mainCam = Camera.main;
        if (mainCam == null) mainCam = Camera.main;

        // Initialize camera bounds first so they are correct when setting up sprites and starfield
        UpdateCameraBounds();

        // Clean up static background sprites in the scene to avoid overlap
        DisableExistingSceneBackgrounds();

        // Load Sprites from Resources
        LoadSprites();

        // Initialize background layers
        if (baseNebulaSprite != null) SetupBaseNebula();
        if (glowNebulaSprite != null) SetupGlowNebula();

        // Initialize twinkling stars
        if (starSprites != null && starSprites.Length > 0) SetupStarfield();

        // Shooting stars setup
        ResetShootingStarTimer();
    }

    private void Start()
    {
        // Smoothly set initial scale
        UpdateCameraBounds();
        RepositionNebulae();
    }

    private void LateUpdate()
    {
        // Dynamically find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
        }

        // Calculate target scroll speed multiplier
        float targetMult = 1f;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            targetMult = 0.15f; // Gentle drift on game over
        }
        else if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Speed up when moving up (positive Y velocity), slow down when moving down
                float playerVelY = playerRb.linearVelocity.y;
                targetMult = Mathf.Clamp(1f + playerVelY * 0.15f, 0.3f, 3f);
            }
        }
        else
        {
            // Menu scene: slow drift
            targetMult = 0.4f;
        }
        TargetScrollSpeedMultiplier = targetMult;

        // Smoothly lerp the speed multiplier based on target
        ScrollSpeedMultiplier = Mathf.Lerp(ScrollSpeedMultiplier, TargetScrollSpeedMultiplier, Time.deltaTime * 3f);

        UpdateCameraBounds();

        // 1. Scroll Nebulae
        ScrollNebulaLayer(baseNebulaTransforms, baseNebulaSpeed * ScrollSpeedMultiplier, baseNebulaWorldHeight);
        ScrollNebulaLayer(glowNebulaTransforms, glowNebulaSpeed * ScrollSpeedMultiplier, glowNebulaWorldHeight);

        // 2. Animate Glow Nebula Hue & Saturation
        AnimateGlowNebulaColor();

        // 3. Scroll and Twinkle Starfield
        ScrollAndTwinkleStars();

        // 4. Shooting Stars
        UpdateShootingStars();
    }

    private void LoadSprites()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        bool isPlayScene = sceneName.Contains("play") || sceneName.Contains("game");

        string baseNebulaPath;
        string glowNebulaPath;

        if (isPlayScene)
        {
            // Play scene uses a different combination of base & glow layers
            baseNebulaPath = "Background/yuri_b-space-1548139_1920";
            glowNebulaPath = "Background/geralt-abstract-5286683_1920";
        }
        else
        {
            // Menu scene uses the new menu-background
            baseNebulaPath = "Background/menu-background";
            glowNebulaPath = "Background/Background";
        }

        // Load the base nebula (Multiple sprite sheet)
        Sprite[] baseSheets = Resources.LoadAll<Sprite>(baseNebulaPath);
        if (baseSheets != null && baseSheets.Length > 0)
            baseNebulaSprite = baseSheets[0];

        // Load the glow nebula (Multiple sprite sheet)
        Sprite[] glowSheets = Resources.LoadAll<Sprite>(glowNebulaPath);
        if (glowSheets != null && glowSheets.Length > 0)
            glowNebulaSprite = glowSheets[0];

        // Load star sprites
        List<Sprite> loadedStars = new List<Sprite>();
        for (int i = 1; i <= 3; i++)
        {
            Sprite[] starSheet = Resources.LoadAll<Sprite>($"Background/Star_{i}");
            if (starSheet != null && starSheet.Length > 0)
            {
                loadedStars.Add(starSheet[0]);
            }
        }
        starSprites = loadedStars.ToArray();
    }

    private void DisableExistingSceneBackgrounds()
    {
        // Find sprite renderers and check if they belong to background sprites
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            if (sr.gameObject == gameObject) continue;
            
            string objName = sr.gameObject.name.ToLower();
            string spriteName = sr.sprite != null ? sr.sprite.name.ToLower() : "";

            if (objName.Contains("universe") || objName.Contains("background") ||
                spriteName.Contains("universe") || spriteName.Contains("background"))
            {
                // Disable SpriteRenderer to hide it, but keep the object active just in case
                sr.enabled = false;
                Debug.Log($"[SpaceBackgroundEffects] Disabled existing background sprite renderer: {sr.gameObject.name}");
            }
        }
    }

    private void UpdateCameraBounds()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        float newHeight = mainCam.orthographicSize * 2f;
        float newWidth = newHeight * mainCam.aspect;

        if (Mathf.Abs(newHeight - camHeight) > 0.01f || Mathf.Abs(newWidth - camWidth) > 0.01f)
        {
            camHeight = newHeight;
            camWidth = newWidth;
            RescaleNebulae();
        }
    }

    private void RescaleNebulae()
    {
        if (baseNebulaSprite == null || baseNebulaTransforms.Count < 2) return;

        float spriteWidth = baseNebulaSprite.bounds.size.x;
        float spriteHeight = baseNebulaSprite.bounds.size.y;
        float scaleX = (camWidth + 2f) / spriteWidth;
        float scaleY = scaleX;
        baseNebulaWorldHeight = spriteHeight * scaleY;

        foreach (var t in baseNebulaTransforms)
        {
            if (t != null) t.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        if (glowNebulaSprite != null && glowNebulaTransforms.Count >= 2)
        {
            float glowWidth = glowNebulaSprite.bounds.size.x;
            float glowHeight = glowNebulaSprite.bounds.size.y;
            float glowScaleX = (camWidth + 2f) / glowWidth;
            float glowScaleY = glowScaleX;
            glowNebulaWorldHeight = glowHeight * glowScaleY;

            foreach (var t in glowNebulaTransforms)
            {
                if (t != null) t.localScale = new Vector3(glowScaleX, glowScaleY, 1f);
            }
        }
    }

    private void SetupBaseNebula()
    {
        // Create 2 scrolling base nebula planes for wrapping
        for (int i = 0; i < 2; i++)
        {
            GameObject plane = new GameObject($"BaseNebula_{i}");
            plane.transform.SetParent(this.transform);
            
            SpriteRenderer sr = plane.AddComponent<SpriteRenderer>();
            sr.sprite = baseNebulaSprite;
            sr.sortingOrder = -100; // Deepest layer
            // Dynamic subtle tint that matches the playthrough's baseHue theme!
            Color baseColor = Color.HSVToRGB(baseHue, 0.15f, 0.75f);
            baseColor.a = 1f;
            sr.color = baseColor;

            // Mirror the second plane vertically to make scrolling seamless
            if (i == 1)
            {
                sr.flipY = true;
            }

            // Calculate world height based on camera width to preserve aspect ratio
            float spriteWidth = baseNebulaSprite.bounds.size.x;
            float spriteHeight = baseNebulaSprite.bounds.size.y;
            
            // Scale to fit screen width plus extra margin for scrolling/safety
            float scaleX = (camWidth + 2f) / spriteWidth;
            float scaleY = scaleX; // Keep proportional
            plane.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            baseNebulaWorldHeight = spriteHeight * scaleY;
            baseNebulaTransforms.Add(plane.transform);
        }
    }

    private void SetupGlowNebula()
    {
        // Create 2 scrolling overlay glow nebula planes
        for (int i = 0; i < 2; i++)
        {
            GameObject plane = new GameObject($"GlowNebula_{i}");
            plane.transform.SetParent(this.transform);
            
            SpriteRenderer sr = plane.AddComponent<SpriteRenderer>();
            sr.sprite = glowNebulaSprite;
            sr.sortingOrder = -95; // Just in front of base nebula
            sr.color = new Color(1f, 1f, 1f, 0.25f); // 25% opacity

            // Mirror the second plane vertically to make scrolling seamless
            if (i == 1)
            {
                sr.flipY = true;
            }

            // Scale to fit screen width
            float spriteWidth = glowNebulaSprite.bounds.size.x;
            float spriteHeight = glowNebulaSprite.bounds.size.y;
            
            float scaleX = (camWidth + 2f) / spriteWidth;
            float scaleY = scaleX;
            plane.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            glowNebulaWorldHeight = spriteHeight * scaleY;
            glowNebulaTransforms.Add(plane.transform);
            glowNebulaRenderers.Add(sr);
        }
    }

    private void RepositionNebulae()
    {
        // Position base nebulae
        if (baseNebulaTransforms.Count == 2)
        {
            baseNebulaTransforms[0].position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 10f);
            baseNebulaTransforms[1].position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y + baseNebulaWorldHeight, 10f);
        }

        // Position glow nebulae
        if (glowNebulaTransforms.Count == 2)
        {
            glowNebulaTransforms[0].position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 9.5f);
            glowNebulaTransforms[1].position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y + glowNebulaWorldHeight, 9.5f);
        }
    }

    private void ScrollNebulaLayer(List<Transform> layers, float speed, float worldHeight)
    {
        if (layers.Count < 2 || mainCam == null) return;

        float camY = mainCam.transform.position.y;

        for (int i = 0; i < layers.Count; i++)
        {
            Transform layer = layers[i];
            // Scroll down
            layer.Translate(Vector3.down * speed * Time.deltaTime, Space.World);

            // Keep X aligned with camera
            layer.position = new Vector3(mainCam.transform.position.x, layer.position.y, layer.position.z);

            // Wrap when it gets fully below the screen
            if (layer.position.y < camY - worldHeight)
            {
                // Find the other layer to place this one exactly on top of it
                Transform otherLayer = layers[i == 0 ? 1 : 0];
                layer.position = new Vector3(layer.position.x, otherLayer.position.y + worldHeight - 0.005f, layer.position.z);
            }
        }
    }

    private void AnimateGlowNebulaColor()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        bool isPlayScene = sceneName.Contains("play") || sceneName.Contains("game");

        float baseOpacity = isPlayScene ? 0.18f : 0.28f; // slightly lower in play scene for gameplay visibility

        // Gaseous shifting color tint (Hue cycles over time around the randomized playthrough theme baseHue)
        float shiftAmount = Mathf.Sin(Time.time * hueShiftSpeed) * 0.12f;
        float mappedHue = (baseHue + shiftAmount) % 1.0f;
        if (mappedHue < 0f) mappedHue += 1.0f;
        
        Color cosmicColor = Color.HSVToRGB(mappedHue, 0.55f, 0.9f);
        cosmicColor.a = baseOpacity; // Transparency

        foreach (var renderer in glowNebulaRenderers)
        {
            if (renderer != null)
            {
                renderer.color = cosmicColor;
            }
        }
    }

    private void SetupStarfield()
    {
        // Spawn stars across the screen area
        for (int i = 0; i < totalStars; i++)
        {
            GameObject starObj = new GameObject($"BackgroundStar_{i}");
            starObj.transform.SetParent(this.transform);

            SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
            sr.sprite = starSprites[Random.Range(0, starSprites.Length)];

            // Randomize position within screen bounds initially
            float startX = Random.Range(mainCam.transform.position.x - camWidth/2f - 2f, mainCam.transform.position.x + camWidth/2f + 2f);
            float startY = Random.Range(mainCam.transform.position.y - camHeight/2f - 2f, mainCam.transform.position.y + camHeight/2f + 2f);
            starObj.transform.position = new Vector3(startX, startY, 5f);

            TwinkleStar star = new TwinkleStar
            {
                transform = starObj.transform,
                renderer = sr,
                twinkleSpeed = Random.Range(1.5f, 5f),
                phaseOffset = Random.Range(0f, Mathf.PI * 2f)
            };

            // Distribute stars into Far (60%), Mid (30%), Near (10%) layers
            float randVal = Random.value;
            if (randVal < 0.6f)
            {
                // Far stars
                sr.sortingOrder = -90;
                star.baseSpeed = farStarsSpeed;
                star.baseScale = Random.Range(0.12f, 0.22f);
                star.minAlpha = 0.15f;
                star.maxAlpha = 0.5f;
                sr.color = new Color(0.7f, 0.8f, 1f, Random.Range(0.2f, 0.5f));
            }
            else if (randVal < 0.9f)
            {
                // Mid stars
                sr.sortingOrder = -85;
                star.baseSpeed = midStarsSpeed;
                star.baseScale = Random.Range(0.28f, 0.45f);
                star.minAlpha = 0.3f;
                star.maxAlpha = 0.8f;
                sr.color = new Color(1f, 1f, 0.9f, Random.Range(0.4f, 0.8f));
            }
            else
            {
                // Near stars (larger, brighter)
                sr.sortingOrder = -80;
                star.baseSpeed = nearStarsSpeed;
                star.baseScale = Random.Range(0.55f, 0.75f);
                star.minAlpha = 0.5f;
                star.maxAlpha = 1.0f;
                sr.color = new Color(1f, 0.95f, 0.95f, Random.Range(0.6f, 1f));
            }

            // Apply base scale
            starObj.transform.localScale = Vector3.one * star.baseScale;
            twinklingStars.Add(star);
        }
    }

    private void ScrollAndTwinkleStars()
    {
        if (mainCam == null) return;

        float camY = mainCam.transform.position.y;
        float bottomBoundary = camY - camHeight / 2f - 2f;
        float topBoundary = camY + camHeight / 2f + 2f;
        float leftBoundary = mainCam.transform.position.x - camWidth / 2f - 2f;
        float rightBoundary = mainCam.transform.position.x + camWidth / 2f + 2f;

        foreach (var star in twinklingStars)
        {
            if (star == null || star.transform == null) continue;

            // 1. Scroll
            float actualSpeed = star.baseSpeed * ScrollSpeedMultiplier;
            star.transform.Translate(Vector3.down * actualSpeed * Time.deltaTime, Space.World);

            // 2. Twinkle
            float twinkleFactor = Mathf.Sin(Time.time * star.twinkleSpeed + star.phaseOffset);
            float alpha = Mathf.Lerp(star.minAlpha, star.maxAlpha, (twinkleFactor + 1f) * 0.5f);
            Color col = star.renderer.color;
            col.a = alpha;
            star.renderer.color = col;

            // Dynamic scale pulse
            float scaleMult = Mathf.Lerp(0.85f, 1.2f, (twinkleFactor + 1f) * 0.5f);
            star.transform.localScale = Vector3.one * (star.baseScale * scaleMult);

            // 3. Wrap around bottom to top
            if (star.transform.position.y < bottomBoundary)
            {
                float newX = Random.Range(leftBoundary, rightBoundary);
                float newY = topBoundary + Random.Range(0f, 2f);
                star.transform.position = new Vector3(newX, newY, star.transform.position.z);
            }
        }
    }

    private void ResetShootingStarTimer()
    {
        shootingStarTimer = 0f;
        nextShootingStarTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void UpdateShootingStars()
    {
        shootingStarTimer += Time.deltaTime;
        if (shootingStarTimer >= nextShootingStarTime)
        {
            SpawnShootingStar();
            ResetShootingStarTimer();
        }
    }

    private void SpawnShootingStar()
    {
        if (mainCam == null || starSprites == null || starSprites.Length == 0) return;

        GameObject ssObj = new GameObject("ShootingStar");
        ssObj.transform.SetParent(this.transform);

        SpriteRenderer sr = ssObj.AddComponent<SpriteRenderer>();
        // Choose a star sprite
        sr.sprite = starSprites[Random.Range(0, starSprites.Length)];
        sr.sortingOrder = -75; // In front of standard stars
        
        // Pure bright cyan/white shooting star
        Color starColor = Random.value > 0.5f ? new Color(0.6f, 0.9f, 1f, 1f) : new Color(1f, 1f, 1f, 1f);
        sr.color = starColor;

        // Position and direction
        // Start from top-left or top-right quadrant and slide down diagonally
        bool fromRight = Random.value > 0.5f;
        float startX = mainCam.transform.position.x + (fromRight ? (camWidth / 2f) : (-camWidth / 2f));
        float startY = mainCam.transform.position.y + Random.Range(0f, camHeight / 2f);
        ssObj.transform.position = new Vector3(startX, startY, 4.5f);

        // Stretched scale to look like a streak/streak line
        float streakLength = Random.Range(1.8f, 3f);
        float streakWidth = Random.Range(0.12f, 0.2f);
        ssObj.transform.localScale = new Vector3(streakWidth, streakLength, 1f);

        // Rotate in direction of motion (moving down and diagonally)
        float angleX = fromRight ? -45f : 45f; // tilt diagonally
        ssObj.transform.rotation = Quaternion.Euler(0f, 0f, angleX + 180f); // point downwards along trajectory

        Vector3 moveDirection = new Vector3(fromRight ? -1f : 1f, -1.2f, 0f).normalized;

        // Add a self-contained movement & fade script
        ShootingStarMover mover = ssObj.AddComponent<ShootingStarMover>();
        mover.direction = moveDirection;
        mover.speed = shootingStarSpeed * Random.Range(0.85f, 1.2f);
        mover.color = starColor;
    }
}

/// <summary>
/// Helper component added to shooting stars to move and fade them out.
/// </summary>
public class ShootingStarMover : MonoBehaviour
{
    public Vector3 direction;
    public float speed;
    public Color color;

    private SpriteRenderer sr;
    private float lifeTime = 0.8f;
    private float elapsed = 0f;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifeTime + 0.1f);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        elapsed += Time.deltaTime;
        float t = elapsed / lifeTime;

        // Fade color alpha
        if (sr != null)
        {
            Color c = color;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;
        }

        // Stretches out slightly as it gets faster, then fades
        transform.localScale = new Vector3(
            transform.localScale.x * (1f - Time.deltaTime * 0.5f),
            transform.localScale.y * (1f + Time.deltaTime * 0.8f),
            1f
        );
    }
}
