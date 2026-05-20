using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Defines the types of power-ups available in the game
/// </summary>
public enum PowerUpType
{
    None,
    Shield,           // Temporary invincibility
    SpeedBoost,      // Increased movement speed
    TripleShot,      // Fire 3 bullets at once
    RapidFire,       // Increased fire rate
    PiercingShot,    // Bullets pass through enemies
    ScoreBonus,      // Double points for duration
    LifeBoost,       // Gain extra life
    CoinBonus        // Gain extra coins
}

/// <summary>
/// Manages all active power-ups on the player.
/// Handles activation, duration, stacking, and visual feedback.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static PowerUpManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    [Header("Power-Up Durations (seconds)")]
    public float shieldDuration = 5f;
    public float speedBoostDuration = 8f;
    public float tripleShotDuration = 10f;
    public float rapidFireDuration = 8f;
    public float piercingShotDuration = 8f;
    public float scoreBonusDuration = 15f;

    [Header("Power-Up Intensities")]
    public float speedBoostMultiplier = 1.5f;
    public float fireRateBoostMultiplier = 2f;
    public float scoreBonusMultiplier = 2f;

    // -------------------------------------------------------------------------
    // Active Power-Up State
    // -------------------------------------------------------------------------

    [Serializable]
    public class ActivePowerUp
    {
        public PowerUpType type;
        public float remainingTime;
        public float totalDuration;
        public int stackCount;

        public float GetProgress()
        {
            if (totalDuration <= 0) return 1f;
            return 1f - (remainingTime / totalDuration);
        }
    }

    private Dictionary<PowerUpType, ActivePowerUp> _activePowerUps = new Dictionary<PowerUpType, ActivePowerUp>();
    private List<PowerUpType> _newlyActivated = new List<PowerUpType>();
    private List<PowerUpType> _expiredPowerUps = new List<PowerUpType>();

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    public event Action<PowerUpType, float> OnPowerUpActivated;
    public event Action<PowerUpType> OnPowerUpDeactivated;
    public event Action<PowerUpType, float> OnPowerUpTimeUpdated;
    public event Action<PowerUpType, int> OnPowerUpStacked;

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public bool IsShieldActive => IsPowerUpActive(PowerUpType.Shield);
    public bool IsSpeedBoostActive => IsPowerUpActive(PowerUpType.SpeedBoost);
    public bool IsTripleShotActive => IsPowerUpActive(PowerUpType.TripleShot);
    public bool IsRapidFireActive => IsPowerUpActive(PowerUpType.RapidFire);
    public bool IsPiercingShotActive => IsPowerUpActive(PowerUpType.PiercingShot);
    public bool IsScoreBonusActive => IsPowerUpActive(PowerUpType.ScoreBonus);

    public float SpeedMultiplier => IsSpeedBoostActive ? speedBoostMultiplier : 1f;
    public float FireRateMultiplier => IsRapidFireActive ? fireRateBoostMultiplier : 1f;
    public float ScoreMultiplier => IsScoreBonusActive ? scoreBonusMultiplier : 1f;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        UpdatePowerUpTimers();
    }

    // -------------------------------------------------------------------------
    // Core Methods
    // -------------------------------------------------------------------------

    private void UpdatePowerUpTimers()
    {
        _expiredPowerUps.Clear();
        _newlyActivated.Clear();

        float deltaTime = Time.deltaTime;

        foreach (var kvp in _activePowerUps)
        {
            ActivePowerUp powerUp = kvp.Value;
            powerUp.remainingTime -= deltaTime;

            OnPowerUpTimeUpdated?.Invoke(kvp.Key, powerUp.remainingTime);

            if (powerUp.remainingTime <= 0)
            {
                _expiredPowerUps.Add(kvp.Key);
            }
        }

        // Deactivate expired power-ups
        foreach (PowerUpType type in _expiredPowerUps)
        {
            DeactivatePowerUp(type);
        }
    }

    /// <summary>
    /// Activates a power-up. If already active, extends duration and increases stack.
    /// </summary>
    public void ActivatePowerUp(PowerUpType type, float customDuration = -1f)
    {
        if (type == PowerUpType.None) return;

        float duration = GetDuration(type, customDuration);

        if (_activePowerUps.TryGetValue(type, out ActivePowerUp existing))
        {
            // Extend duration and stack
            existing.remainingTime = Mathf.Max(existing.remainingTime, duration);
            existing.totalDuration = duration;
            existing.stackCount++;

            OnPowerUpStacked?.Invoke(type, existing.stackCount);
            OnPowerUpTimeUpdated?.Invoke(type, existing.remainingTime);
        }
        else
        {
            // New power-up
            ActivePowerUp newPowerUp = new ActivePowerUp
            {
                type = type,
                remainingTime = duration,
                totalDuration = duration,
                stackCount = 1
            };

            _activePowerUps[type] = newPowerUp;
            _newlyActivated.Add(type);

            OnPowerUpActivated?.Invoke(type, duration);
            ApplyPowerUpEffects(type);
        }
    }

    /// <summary>
    /// Deactivates a power-up and removes its effects
    /// </summary>
    public void DeactivatePowerUp(PowerUpType type)
    {
        if (_activePowerUps.ContainsKey(type))
        {
            RemovePowerUpEffects(type);
            _activePowerUps.Remove(type);
            OnPowerUpDeactivated?.Invoke(type);
        }
    }

    /// <summary>
    /// Deactivates all power-ups
    /// </summary>
    public void DeactivateAllPowerUps()
    {
        foreach (var kvp in _activePowerUps)
        {
            RemovePowerUpEffects(kvp.Key);
            OnPowerUpDeactivated?.Invoke(kvp.Key);
        }
        _activePowerUps.Clear();
    }

    // -------------------------------------------------------------------------
    // Duration Helpers
    // -------------------------------------------------------------------------

    private float GetDuration(PowerUpType type, float customDuration)
    {
        if (customDuration > 0) return customDuration;

        return type switch
        {
            PowerUpType.Shield => shieldDuration,
            PowerUpType.SpeedBoost => speedBoostDuration,
            PowerUpType.TripleShot => tripleShotDuration,
            PowerUpType.RapidFire => rapidFireDuration,
            PowerUpType.PiercingShot => piercingShotDuration,
            PowerUpType.ScoreBonus => scoreBonusDuration,
            _ => 5f
        };
    }

    // -------------------------------------------------------------------------
    // Effect Application
    // -------------------------------------------------------------------------

    private void ApplyPowerUpEffects(PowerUpType type)
    {
        // Effects are applied through properties that the player checks
        // This allows for dynamic effects based on active power-ups
    }

    private void RemovePowerUpEffects(PowerUpType type)
    {
        // Effects are removed automatically when the property checks return false
    }

    // -------------------------------------------------------------------------
    // Status Checks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Checks if a specific power-up is currently active
    /// </summary>
    public bool IsPowerUpActive(PowerUpType type)
    {
        return _activePowerUps.ContainsKey(type) && _activePowerUps[type].remainingTime > 0;
    }

    /// <summary>
    /// Gets the remaining time for a power-up
    /// </summary>
    public float GetPowerUpRemainingTime(PowerUpType type)
    {
        if (_activePowerUps.TryGetValue(type, out ActivePowerUp powerUp))
        {
            return powerUp.remainingTime;
        }
        return 0f;
    }

    /// <summary>
    /// Gets the progress (0-1) of a power-up's duration
    /// </summary>
    public float GetPowerUpProgress(PowerUpType type)
    {
        if (_activePowerUps.TryGetValue(type, out ActivePowerUp powerUp))
        {
            return powerUp.GetProgress();
        }
        return 1f;
    }

    /// <summary>
    /// Gets a list of all currently active power-ups
    /// </summary>
    public List<PowerUpType> GetActivePowerUps()
    {
        List<PowerUpType> activeList = new List<PowerUpType>();
        foreach (var kvp in _activePowerUps)
        {
            if (kvp.Value.remainingTime > 0)
            {
                activeList.Add(kvp.Key);
            }
        }
        return activeList;
    }

    /// <summary>
    /// Gets the number of active power-ups
    /// </summary>
    public int GetActivePowerUpCount()
    {
        int count = 0;
        foreach (var kvp in _activePowerUps)
        {
            if (kvp.Value.remainingTime > 0)
            {
                count++;
            }
        }
        return count;
    }

    // -------------------------------------------------------------------------
    // Debug
    // -------------------------------------------------------------------------

    public string GetActivePowerUpsDebugString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Active Power-Ups:");

        foreach (var kvp in _activePowerUps)
        {
            if (kvp.Value.remainingTime > 0)
            {
                sb.AppendLine($"  {kvp.Value.type}: {kvp.Value.remainingTime:F1}s (x{kvp.Value.stackCount})");
            }
        }

        return sb.ToString();
    }

#if UNITY_EDITOR
    [ContextMenu("Test Activate All Power-Ups")]
    private void TestActivateAll()
    {
        ActivatePowerUp(PowerUpType.Shield);
        ActivatePowerUp(PowerUpType.SpeedBoost);
        ActivatePowerUp(PowerUpType.TripleShot);
        ActivatePowerUp(PowerUpType.RapidFire);
        ActivatePowerUp(PowerUpType.PiercingShot);
        ActivatePowerUp(PowerUpType.ScoreBonus);
    }

    [ContextMenu("Test Deactivate All")]
    private void TestDeactivateAll()
    {
        DeactivateAllPowerUps();
    }
#endif
}
