using UnityEngine;

/// <summary>
/// Assigns a sprite to the SpriteRenderer based on the spriteType enum.
/// spriteType: 0 = ship, 1 = asteroid, 2 = star (diamond/silver/gold), 3 = laser
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteGenerator : MonoBehaviour
{
    public enum SpriteType { Ship = 0, Asteroid = 1, Star = 2, Laser = 3 }

    public SpriteType spriteType = SpriteType.Ship;

    [Header("Ship Sprites")]
    public Sprite[] shipSprites;

    [Header("Asteroid Sprites")]
    public Sprite[] asteroidSprites;

    [Header("Star Sprites")]
    public Sprite[] starSprites;

    [Header("Laser Sprites")]
    public Sprite[] laserSprites;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        GenerateSprite();
    }

    private void GenerateSprite()
    {
        Sprite[] sprites = null;

        switch (spriteType)
        {
            case SpriteType.Ship:
                sprites = shipSprites;
                break;
            case SpriteType.Asteroid:
                sprites = asteroidSprites;
                break;
            case SpriteType.Star:
                sprites = starSprites;
                break;
            case SpriteType.Laser:
                sprites = laserSprites;
                break;
        }

        if (sprites != null && sprites.Length > 0)
        {
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
        }
    }
}
