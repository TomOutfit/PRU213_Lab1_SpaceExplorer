using UnityEngine;

/// <summary>
/// ScriptableObject defining a ship type with unique stats.
/// Used for the ship selection system in Space Explorer.
/// </summary>
[CreateAssetMenu(fileName = "NewShip", menuName = "Space Explorer/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Ship Identity")]
    public string shipName = "Fighter";
    [TextArea(2, 4)]
    public string description = "A fast and agile fighter ship.";

    [Header("Movement")]
    public float speed = 8f;
    public float rotationSpeed = 10f;

    [Header("Combat")]
    public float fireRate = 0.15f;
    public float damage = 15f;
    public float laserSpeed = 15f;

    [Header("Defense")]
    public int maxLives = 3;

    [Header("Visual")]
    public float size = 0.5f;
    public Color shipColor = Color.cyan;

    [Header("Special Effects")]
    public bool hasShield = false;
    public bool hasDoubleShot = false;
    public bool hasTripleShot = false;

    /// <summary>
    /// Gets the fire rate interval based on fire rate value.
    /// </summary>
    public float GetFireInterval()
    {
        return fireRate;
    }

    /// <summary>
    /// Gets the number of projectiles per shot.
    /// </summary>
    public int GetProjectileCount()
    {
        if (hasTripleShot) return 3;
        if (hasDoubleShot) return 2;
        return 1;
    }

    /// <summary>
    /// Gets the ship type identifier for internal use.
    /// </summary>
    public ShipType GetShipType()
    {
        if (shipName == "Speeder") return ShipType.Speeder;
        if (shipName == "Tank") return ShipType.Tank;
        if (shipName == "Sniper") return ShipType.Sniper;
        if (shipName == "Bomber") return ShipType.Bomber;
        return ShipType.Fighter;
    }
}

public enum ShipType
{
    Fighter,
    Speeder,
    Tank,
    Sniper,
    Bomber
}
