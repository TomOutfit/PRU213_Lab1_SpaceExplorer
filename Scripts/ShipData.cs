using UnityEngine;

/// <summary>
/// ScriptableObject defining a ship's statistics and properties.
/// Each ship type has unique stats affecting gameplay.
/// </summary>
[CreateAssetMenu(fileName = "NewShip", menuName = "Space Explorer/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Ship Identity")]
    public string shipName = "Default Ship";
    public Sprite shipSprite;
    public GameObject shipPrefab;
    public string description = "A balanced ship";

    [Header("Base Stats (0-10 scale)")]
    public float speed = 5f;
    public float fireRate = 1f;
    public float damage = 1f;
    public float armor = 1f;

    [Header("Gameplay Modifiers")]
    public int startingLives = 3;
    public float bulletSpeedMultiplier = 1f;
    public float pointsMultiplier = 1f;
    public float shieldDuration = 0f;
    public bool hasTripleShot = false;
    public bool hasPiercingShot = false;

    [Header("Visual Effects")]
    public Color shipColor = Color.white;
    public ParticleSystem engineParticles;
    public Sprite[] damageStates;

    [Header("Unlocking")]
    public bool isUnlocked = true;
    public int unlockCost = 0;
    public bool isDefaultShip = false;

    /// <summary>
    /// Calculates effective speed based on ship stats
    /// </summary>
    public float GetSpeed()
    {
        return speed * 1.5f;
    }

    /// <summary>
    /// Calculates effective fire rate (shots per second)
    /// </summary>
    public float GetFireRate()
    {
        return fireRate * 0.5f + 0.3f;
    }

    /// <summary>
    /// Calculates damage multiplier
    /// </summary>
    public float GetDamage()
    {
        return damage * 5f;
    }

    /// <summary>
    /// Gets the lives penalty modifier (higher armor = less penalty)
    /// </summary>
    public float GetArmorModifier()
    {
        return 1f - (armor * 0.1f);
    }
}
