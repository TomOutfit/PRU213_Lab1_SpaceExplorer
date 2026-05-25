using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages ship selection in the main menu.
/// Handles unlocking, selection, and persistence of chosen ship.
/// </summary>
public class ShipSelectionController : MonoBehaviour
{
    [Header("Ship Configuration")]
    public ShipData[] availableShips;
    public int currentShipIndex = 0;

    [Header("Ship Loading")]
    public bool loadShipsFromResources = true;
    public string shipsResourcesPath = "ScriptableObjects";

    [Header("UI References")]
    public GameObject shipSelectionPanel;
    public Image shipPreviewImage;
    public Text shipNameText;
    public Text shipDescriptionText;
    public Text shipStatsText;
    public Button[] shipButtons;
    public Button selectButton;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Text coinsText;

    [Header("Ship Stats Display")]
    public Image speedBar;
    public Image fireRateBar;
    public Image damageBar;
    public Image armorBar;

    [Header("Audio")]
    public AudioClip selectClip;
    public AudioClip confirmClip;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private int _coins = 0;
    private int _selectedIndex = 0;
    private const string COINS_KEY = "PlayerCoins";
    private const string SELECTED_SHIP_KEY = "SelectedShipIndex";
    private const string SHIP_UNLOCK_PREFIX = "ShipUnlocked_";

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public static ShipSelectionController Instance { get; private set; }
    public ShipData SelectedShip => availableShips[_selectedIndex];
    public int Coins => _coins;

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

        LoadPlayerData();
    }

    private void Start()
    {
        LoadAvailableShips();
        InitializeShips();
        UpdateUI();
    }

    private void LoadAvailableShips()
    {
        if (loadShipsFromResources)
        {
            ShipData[] loadedShips = Resources.LoadAll<ShipData>(shipsResourcesPath);
            if (loadedShips != null && loadedShips.Length > 0)
            {
                availableShips = loadedShips;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void InitializeShips()
    {
        foreach (ShipData ship in availableShips)
        {
            string key = SHIP_UNLOCK_PREFIX + ship.name;
            ship.isUnlocked = PlayerPrefs.GetInt(key, ship.isDefaultShip ? 1 : 0) == 1;
        }
    }

    private void LoadPlayerData()
    {
        _coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        _selectedIndex = PlayerPrefs.GetInt(SELECTED_SHIP_KEY, 0);
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetInt(COINS_KEY, _coins);
        PlayerPrefs.SetInt(SELECTED_SHIP_KEY, _selectedIndex);
        PlayerPrefs.Save();
    }

    // -------------------------------------------------------------------------
    // Ship Navigation
    // -------------------------------------------------------------------------

    public void NextShip()
    {
        _selectedIndex = (_selectedIndex + 1) % availableShips.Length;
        PlaySelectSound();
        UpdateUI();
    }

    public void PreviousShip()
    {
        _selectedIndex = (_selectedIndex - 1 + availableShips.Length) % availableShips.Length;
        PlaySelectSound();
        UpdateUI();
    }

    public void SelectShip(int index)
    {
        if (index < 0 || index >= availableShips.Length) return;

        ShipData ship = availableShips[index];

        if (!ship.isUnlocked)
        {
            TryUnlockShip(index);
            return;
        }

        _selectedIndex = index;
        PlaySelectSound();
        UpdateUI();
    }

    public void ConfirmSelection()
    {
        ShipData ship = availableShips[_selectedIndex];

        if (!ship.isUnlocked)
        {
            TryUnlockShip(_selectedIndex);
            return;
        }

        SavePlayerData();
        PlayConfirmSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("GamePlayScene");
        }
        else
        {
            SceneManager.LoadScene("GamePlayScene");
        }
    }

    // -------------------------------------------------------------------------
    // Unlocking System
    // -------------------------------------------------------------------------

    private void TryUnlockShip(int index)
    {
        ShipData ship = availableShips[index];

        if (_coins >= ship.unlockCost)
        {
            _coins -= ship.unlockCost;
            ship.isUnlocked = true;

            string key = SHIP_UNLOCK_PREFIX + ship.name;
            PlayerPrefs.SetInt(key, 1);

            SavePlayerData();
            UpdateUI();

            PlayConfirmSound();
        }
        else
        {
            Debug.Log("Not enough coins to unlock " + ship.shipName);
        }
    }

    public void AddCoins(int amount)
    {
        _coins += amount;
        SavePlayerData();
        UpdateUI();
    }

    // -------------------------------------------------------------------------
    // UI Updates
    // -------------------------------------------------------------------------

    private void UpdateUI()
    {
        if (availableShips == null || availableShips.Length == 0) return;

        ShipData ship = availableShips[_selectedIndex];

        // Update preview
        if (shipPreviewImage != null && ship.shipSprite != null)
        {
            shipPreviewImage.sprite = ship.shipSprite;
        }

        // Update text
        if (shipNameText != null)
        {
            shipNameText.text = ship.shipName;
        }

        if (shipDescriptionText != null)
        {
            shipDescriptionText.text = ship.description;
        }

        if (shipStatsText != null)
        {
            shipStatsText.text = $"Lives: {ship.startingLives}\n" +
                                $"Points Bonus: {ship.pointsMultiplier:F1}x\n" +
                                $"Triple Shot: {(ship.hasTripleShot ? "Yes" : "No")}\n" +
                                $"Piercing: {(ship.hasPiercingShot ? "Yes" : "No")}";
        }

        // Update stat bars
        UpdateStatBar(speedBar, ship.speed);
        UpdateStatBar(fireRateBar, ship.fireRate);
        UpdateStatBar(damageBar, ship.damage);
        UpdateStatBar(armorBar, ship.armor);

        // Update buttons
        UpdateShipButtons();
        UpdateCoinsDisplay();
    }

    private void UpdateStatBar(Image bar, float value)
    {
        if (bar != null)
        {
            bar.fillAmount = value / 10f;
        }
    }

    private void UpdateShipButtons()
    {
        if (shipButtons == null || shipButtons.Length < availableShips.Length) return;

        for (int i = 0; i < shipButtons.Length; i++)
        {
            ShipData ship = availableShips[i];
            Button btn = shipButtons[i];

            ColorBlock colors = btn.colors;
            colors.normalColor = ship.isUnlocked ? Color.green : Color.gray;
            colors.selectedColor = ship.isUnlocked ? Color.green : Color.gray;
            btn.colors = colors;

            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                if (!ship.isUnlocked)
                {
                    btnText.text = $"{ship.shipName}\n{ship.unlockCost} coins";
                }
                else
                {
                    btnText.text = ship.shipName;
                }
            }
        }
    }

    private void UpdateCoinsDisplay()
    {
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {_coins}";
        }
    }

    // -------------------------------------------------------------------------
    // Audio
    // -------------------------------------------------------------------------

    private void PlaySelectSound()
    {
        if (selectClip != null && GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(selectClip);
        }
    }

    private void PlayConfirmSound()
    {
        if (confirmClip != null && GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(confirmClip);
        }
    }

    // -------------------------------------------------------------------------
    // Panel Control
    // -------------------------------------------------------------------------

    public void ShowShipSelection()
    {
        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(true);
            UpdateUI();
        }
    }

    public void HideShipSelection()
    {
        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(false);
        }
    }

    public void ToggleShipSelection()
    {
        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(!shipSelectionPanel.activeSelf);
            if (shipSelectionPanel.activeSelf)
            {
                UpdateUI();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Getters for Gameplay
    // -------------------------------------------------------------------------

    public ShipData GetSelectedShipData()
    {
        if (availableShips == null || availableShips.Length == 0) return null;
        if (_selectedIndex < 0 || _selectedIndex >= availableShips.Length) return availableShips[0];
        return availableShips[_selectedIndex];
    }

    public int GetSelectedShipIndex()
    {
        return _selectedIndex;
    }
}
