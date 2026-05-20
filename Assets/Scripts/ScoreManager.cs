// =============================================================================
// ScoreManager.cs
// Purpose : Singleton MonoBehaviour that owns the player's score for the
//           current gameplay session. Includes advanced scoring system with:
//           - Combo multiplier (increases with consecutive hits)
//           - Streak tracking (for consecutive kills without damage)
//           - Different point values per collectible type
//           - Persistent across scene loads via DontDestroyOnLoad
// Author  : Student
// Date    : 2025-07-14
// Updated : 2026-05-18 - Added combo, multiplier, streak system
// =============================================================================

using UnityEngine;
using System;

/// <summary>
/// Manages the player's score with advanced scoring mechanics:
/// - Base score from collecting items and destroying enemies
/// - Combo multiplier that increases with consecutive actions
/// - Streak bonus for consecutive successful actions
/// - High score tracking for the session
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton instance
    // -------------------------------------------------------------------------

    public static ScoreManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Scoring Configuration
    // -------------------------------------------------------------------------

    [Header("Base Point Values")]
    public int asteroidDestroyPoints = 5;
    public int starBluePoints = 10;
    public int starSilverPoints = 20;
    public int starGoldPoints = 30;
    public int enemyShipPoints = 15;
    public int ufoPoints = 25;
    public int powerUpBonusPoints = 50;

    [Header("Combo System")]
    public float comboExpireTime = 3f;
    public float comboMultiplierIncrease = 0.1f;
    public float maxComboMultiplier = 5f;

    [Header("Streak System")]
    public int streakThresholdForBonus = 5;
    public int streakBonusPoints = 25;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int _score = 0;
    private int _highScore = 0;
    private float _comboTimer = 0f;
    private float _currentMultiplier = 1f;
    private int _comboCount = 0;
    private int _streakCount = 0;
    private int _totalAsteroidsDestroyed = 0;
    private int _totalStarsCollected = 0;
    private int _totalEnemiesDestroyed = 0;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action<int> OnStreakChanged;
    public event Action<float> OnMultiplierChanged;
    public event Action<int> OnHighScoreChanged;
    public event Action<ScoreType, int> OnPointsEarned;

    public enum ScoreType
    {
        AsteroidDestroy,
        StarCollect,
        EnemyDestroy,
        UFODestroy,
        PowerUpCollect,
        StreakBonus,
        ComboBonus
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public int Score => _score;
    public int HighScore => _highScore;
    public float CurrentMultiplier => _currentMultiplier;
    public int ComboCount => _comboCount;
    public int StreakCount => _streakCount;
    public int TotalAsteroidsDestroyed => _totalAsteroidsDestroyed;
    public int TotalStarsCollected => _totalStarsCollected;
    public int TotalEnemiesDestroyed => _totalEnemiesDestroyed;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        UpdateComboTimer();
    }

    // -------------------------------------------------------------------------
    // Combo System
    // -------------------------------------------------------------------------

    private void UpdateComboTimer()
    {
        if (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    private void ResetCombo()
    {
        _comboCount = 0;
        _currentMultiplier = 1f;
        _comboTimer = 0f;
        OnComboChanged?.Invoke(_comboCount);
        OnMultiplierChanged?.Invoke(_currentMultiplier);
    }

    private void IncrementCombo()
    {
        _comboTimer = comboExpireTime;
        _comboCount++;
        _currentMultiplier = Mathf.Min(1f + (_comboCount * comboMultiplierIncrease), maxComboMultiplier);
        OnComboChanged?.Invoke(_comboCount);
        OnMultiplierChanged?.Invoke(_currentMultiplier);
    }

    // -------------------------------------------------------------------------
    // Streak System
    // -------------------------------------------------------------------------

    private void IncrementStreak()
    {
        _streakCount++;

        if (_streakCount > 0 && _streakCount % streakThresholdForBonus == 0)
        {
            int bonus = streakBonusPoints * (_streakCount / streakThresholdForBonus);
            AddScoreInternal(bonus, ScoreType.StreakBonus);
        }

        OnStreakChanged?.Invoke(_streakCount);
    }

    private void ResetStreak()
    {
        _streakCount = 0;
        OnStreakChanged?.Invoke(_streakCount);
    }

    // -------------------------------------------------------------------------
    // Core Scoring Methods
    // -------------------------------------------------------------------------

    private void AddScoreInternal(int basePoints, ScoreType type)
    {
        int multipliedPoints = Mathf.RoundToInt(basePoints * _currentMultiplier);
        _score += multipliedPoints;

        if (_score > _highScore)
        {
            _highScore = _score;
            OnHighScoreChanged?.Invoke(_highScore);
        }

        OnScoreChanged?.Invoke(_score);
        OnPointsEarned?.Invoke(type, multipliedPoints);
    }

    /// <summary>
    /// Adds asteroid destruction score with combo system
    /// </summary>
    public void AddAsteroidDestroyScore()
    {
        IncrementCombo();
        IncrementStreak();
        _totalAsteroidsDestroyed++;
        AddScoreInternal(asteroidDestroyPoints, ScoreType.AsteroidDestroy);
    }

    /// <summary>
    /// Adds star collection score based on star type
    /// </summary>
    public void AddStarScore(StarType type)
    {
        IncrementCombo();
        IncrementStreak();
        _totalStarsCollected++;

        int points = type switch
        {
            StarType.Blue => starBluePoints,
            StarType.Silver => starSilverPoints,
            StarType.Gold => starGoldPoints,
            _ => starBluePoints
        };

        AddScoreInternal(points, ScoreType.StarCollect);
    }

    /// <summary>
    /// Adds enemy ship destruction score
    /// </summary>
    public void AddEnemyDestroyScore()
    {
        IncrementCombo();
        IncrementStreak();
        _totalEnemiesDestroyed++;
        AddScoreInternal(enemyShipPoints, ScoreType.EnemyDestroy);
    }

    /// <summary>
    /// Adds UFO destruction score (highest base points)
    /// </summary>
    public void AddUFODestroyScore()
    {
        IncrementCombo();
        IncrementStreak();
        AddScoreInternal(ufoPoints, ScoreType.UFODestroy);
    }

    /// <summary>
    /// Adds power-up collection bonus
    /// </summary>
    public void AddPowerUpScore()
    {
        AddScoreInternal(powerUpBonusPoints, ScoreType.PowerUpCollect);
    }

    /// <summary>
    /// Deducts score and resets combo/streak on damage
    /// </summary>
    public void OnPlayerDamaged(int damagePoints = 20)
    {
        _score = Mathf.Max(0, _score - damagePoints);
        ResetCombo();
        ResetStreak();
        OnScoreChanged?.Invoke(_score);
    }

    /// <summary>
    /// Resets the score to zero for a new game session
    /// </summary>
    public void ResetScore()
    {
        _score = 0;
        _comboCount = 0;
        _currentMultiplier = 1f;
        _comboTimer = 0f;
        _streakCount = 0;
        _totalAsteroidsDestroyed = 0;
        _totalStarsCollected = 0;
        _totalEnemiesDestroyed = 0;
        OnScoreChanged?.Invoke(_score);
        OnComboChanged?.Invoke(_comboCount);
        OnMultiplierChanged?.Invoke(_currentMultiplier);
        OnStreakChanged?.Invoke(_streakCount);
    }

    /// <summary>
    /// Returns the current score value
    /// </summary>
    public int GetScore()
    {
        return _score;
    }

    /// <summary>
    /// Returns formatted score string with multiplier display
    /// </summary>
    public string GetScoreDisplay()
    {
        string multiplierText = _currentMultiplier > 1f ? $" x{_currentMultiplier:F1}" : "";
        return $"Score: {_score}{multiplierText}";
    }

    /// <summary>
    /// Returns combo display string
    /// </summary>
    public string GetComboDisplay()
    {
        if (_comboCount < 2) return "";
        return $"COMBO x{_comboCount}";
    }
}

/// <summary>
/// Star type enumeration for different point values
/// </summary>
public enum StarType
{
    Blue,
    Silver,
    Gold
}
