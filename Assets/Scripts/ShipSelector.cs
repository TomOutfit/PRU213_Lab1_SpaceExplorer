using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles ship selection on the main menu.
/// Saves selected ship to PlayerPrefs and updates visual feedback.
/// </summary>
public class ShipSelector : MonoBehaviour
{
    public int shipIndex;
    public string shipName;

    // Ship colors matching the 5 ship types
    private static readonly Color[] shipColors = new Color[]
    {
        Color.cyan,      // Fighter
        Color.green,      // Speeder
        Color.yellow,    // Tank
        new Color(0.6f, 0.2f, 1f), // Sniper (purple)
        Color.red        // Bomber
    };

    private Button button;
    private Image buttonImage;
    private Image previewImage;
    private bool isSelected = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        // Find preview image
        Transform preview = transform.Find("Preview");
        if (preview != null)
        {
            previewImage = preview.GetComponent<Image>();
        }
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnShipSelected);
        }

        // Load previous selection
        int savedIndex = PlayerPrefs.GetInt("SelectedShip", 0);
        if (savedIndex == shipIndex)
        {
            SelectThisShip();
        }
    }

    /// <summary>
    /// Called when this ship button is clicked.
    /// </summary>
    public void OnShipSelected()
    {
        // Deselect all other ships
        ShipSelector[] allSelectors = FindObjectsOfType<ShipSelector>();
        foreach (ShipSelector selector in allSelectors)
        {
            if (selector != this)
            {
                selector.DeselectShip();
            }
        }

        // Select this ship
        SelectThisShip();

        // Save to PlayerPrefs
        PlayerPrefs.SetInt("SelectedShip", shipIndex);
        PlayerPrefs.SetString("SelectedShipName", shipName);
        PlayerPrefs.Save();

        Debug.Log($"[ShipSelector] Selected: {shipName} (Index: {shipIndex})");

        // Play feedback
        AudioManager audio = FindObjectOfType<AudioManager>();
        if (audio != null)
        {
            Debug.Log("[ShipSelector] Ship selected sound");
        }
    }

    /// <summary>
    /// Selects this ship visually.
    /// </summary>
    public void SelectThisShip()
    {
        isSelected = true;
        Color shipColor = GetShipColor();

        if (buttonImage != null)
        {
            buttonImage.color = new Color(shipColor.r, shipColor.g, shipColor.b, 0.9f);
        }

        if (previewImage != null)
        {
            previewImage.color = shipColor;
        }

        // Update ship name color
        Text nameText = GetComponentInChildren<Text>();
        if (nameText != null && nameText.gameObject.name == "ShipName")
        {
            nameText.color = shipColor;
        }
    }

    /// <summary>
    /// Deselects this ship visually.
    /// </summary>
    public void DeselectShip()
    {
        isSelected = false;
        if (buttonImage != null)
        {
            buttonImage.color = new Color(0.15f, 0.15f, 0.3f, 0.9f);
        }
    }

    /// <summary>
    /// Gets the color for this ship type.
    /// </summary>
    public Color GetShipColor()
    {
        if (shipIndex >= 0 && shipIndex < shipColors.Length)
        {
            return shipColors[shipIndex];
        }
        return Color.cyan;
    }

    /// <summary>
    /// Static helper to get selected ship data.
    /// </summary>
    public static int GetSelectedShipIndex()
    {
        return PlayerPrefs.GetInt("SelectedShip", 0);
    }

    public static string GetSelectedShipName()
    {
        return PlayerPrefs.GetString("SelectedShipName", "Fighter");
    }

    public static Color GetSelectedShipColor()
    {
        int index = GetSelectedShipIndex();
        if (index >= 0 && index < shipColors.Length)
        {
            return shipColors[index];
        }
        return Color.cyan;
    }
}
