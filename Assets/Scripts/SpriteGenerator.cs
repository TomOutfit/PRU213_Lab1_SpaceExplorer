using UnityEngine;

/// <summary>
/// Generates procedural 2D sprites for the game: spaceship, asteroid, and star.
/// Uses MeshRenderer and polygon colliders for crisp, scalable graphics.
/// </summary>
public class SpriteGenerator : MonoBehaviour
{
    public enum SpriteType { Spaceship, Asteroid, Star, Laser }

    [SerializeField] private SpriteType spriteType = SpriteType.Spaceship;

    private void Start()
    {
        GenerateSprite();
    }

    /// <summary>
    /// Generates the appropriate sprite based on the selected type.
    /// </summary>
    private void GenerateSprite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        switch (spriteType)
        {
            case SpriteType.Spaceship:
                sr.sprite = CreateSpaceshipSprite();
                sr.color = new Color(0.3f, 0.8f, 1f);
                break;
            case SpriteType.Asteroid:
                sr.sprite = CreateAsteroidSprite();
                sr.color = Color.gray;
                break;
            case SpriteType.Star:
                sr.sprite = CreateStarSprite();
                sr.color = new Color(1f, 0.9f, 0.2f);
                break;
            case SpriteType.Laser:
                sr.sprite = CreateLaserSprite();
                sr.color = new Color(1f, 0.2f, 0.2f);
                break;
        }
    }

    /// <summary>
    /// Creates a spaceship sprite using a triangle shape.
    /// </summary>
    private Sprite CreateSpaceshipSprite()
    {
        int width = 64;
        int height = 64;
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        Vector2 center = new Vector2(width / 2f, height / 2f);

        // Ship points (triangle pointing up)
        Vector2 tip = new Vector2(width / 2f, height - 5);
        Vector2 leftWing = new Vector2(5, 10);
        Vector2 rightWing = new Vector2(width - 5, 10);
        Vector2 bottom = new Vector2(width / 2f, 20);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (IsPointInTriangle(pos, tip, leftWing, rightWing) ||
                    IsPointInTriangle(pos, leftWing, bottom, rightWing))
                {
                    colors[y * width + x] = Color.white;
                }
                else
                {
                    colors[y * width + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Creates an asteroid sprite using an irregular polygon.
    /// </summary>
    private Sprite CreateAsteroidSprite()
    {
        int width = 64;
        int height = 64;
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        // Generate random asteroid shape points
        System.Random rng = new System.Random();
        Vector2 center = new Vector2(width / 2f, height / 2f);
        Vector2[] points = new Vector2[8];

        for (int i = 0; i < 8; i++)
        {
            float angle = (i / 8f) * 360f;
            float radius = 20f + rng.Next(5, 12);
            points[i] = center + new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (IsPointInPolygon(pos, points))
                {
                    // Add some texture variation
                    float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.3f;
                    colors[y * width + x] = Color.Lerp(new Color(0.6f, 0.6f, 0.6f), new Color(0.8f, 0.8f, 0.8f), noise);
                }
                else
                {
                    colors[y * width + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Creates a star sprite with a 5-pointed star shape.
    /// </summary>
    private Sprite CreateStarSprite()
    {
        int width = 64;
        int height = 64;
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        Vector2 center = new Vector2(width / 2f, height / 2f);

        // Create 5-pointed star
        Vector2[] starPoints = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = (i * 36f - 90f) * Mathf.Deg2Rad;
            float radius = (i % 2 == 0) ? 28f : 12f;
            starPoints[i] = center + new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (IsPointInPolygon(pos, starPoints))
                {
                    colors[y * width + x] = Color.white;
                }
                else
                {
                    colors[y * width + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Creates a laser beam sprite.
    /// </summary>
    private Sprite CreateLaserSprite()
    {
        int width = 8;
        int height = 32;
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float centerDist = Mathf.Abs(x - width / 2f) / (width / 2f);
                if (centerDist < 0.5f)
                {
                    float gradient = 1f - centerDist * 2f;
                    colors[y * width + x] = Color.Lerp(Color.red, Color.yellow, gradient);
                }
                else
                {
                    colors[y * width + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Checks if a point is inside a triangle using barycentric coordinates.
    /// </summary>
    private bool IsPointInTriangle(Vector2 p, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float dX = p.x - v3.x;
        float dY = p.y - v3.y;
        float dX21 = v3.x - v2.x;
        float dY12 = v2.y - v3.y;
        float D = dY12 * (v1.x - v3.x) + dX21 * (v1.y - v3.y);
        float s = dY12 * dX + dX21 * dY;
        float t = (v3.y - v1.y) * dX + (v1.x - v3.x) * dY;

        if (D < 0)
            return (s <= 0 && t <= 0 && s + t >= D);
        return (s >= 0 && t >= 0 && s + t <= D);
    }

    /// <summary>
    /// Checks if a point is inside a polygon using ray casting.
    /// </summary>
    private bool IsPointInPolygon(Vector2 p, Vector2[] vertices)
    {
        float minX = vertices[0].x;
        float maxX = vertices[0].x;
        float minY = vertices[0].y;
        float maxY = vertices[0].y;

        for (int i = 1; i < vertices.Length; i++)
        {
            minX = Mathf.Min(minX, vertices[i].x);
            maxX = Mathf.Max(maxX, vertices[i].x);
            minY = Mathf.Min(minY, vertices[i].y);
            maxY = Mathf.Max(maxY, vertices[i].y);
        }

        if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY)
            return false;

        int j = vertices.Length - 1;
        bool inside = false;

        for (int i = 0; i < vertices.Length; j = i++)
        {
            if ((vertices[i].y > p.y) != (vertices[j].y > p.y) &&
                p.x < (vertices[j].x - vertices[i].x) * (p.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
