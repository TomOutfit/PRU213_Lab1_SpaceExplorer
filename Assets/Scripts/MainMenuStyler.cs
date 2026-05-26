using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles all visual styling and effects for the Main Menu.
/// Provides animated title, styled buttons, panel glow, and background effects.
/// </summary>
public class MainMenuStyler : MonoBehaviour
{
    [Header("Title Settings")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text subtitleText;
    [SerializeField] private float titleHue = 0f;
    [SerializeField] private float colorCycleSpeed = 0.08f;
    [SerializeField] private float titleBobSpeed = 1.5f;
    [SerializeField] private float titleBobAmount = 8f;

    [Header("Button Settings")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private float buttonHoverScale = 1.08f;
    [SerializeField] private float buttonClickScale = 0.95f;
    [SerializeField] private float buttonAnimSpeed = 8f;

    [Header("Panel Settings")]
    [SerializeField] private Image shipSelectionPanel;
    [SerializeField] private float panelPulseSpeed = 1.2f;
    [SerializeField] private float panelPulseAmount = 0.04f;

    [Header("Ship Selection")]
    [SerializeField] private ShipSelector[] shipSelectors;

    [Header("Background Effects")]
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private float overlayPulseSpeed = 0.5f;

    // Internal state
    private float time = 0f;
    private Vector3 titleBasePos;
    private bool instructionsShowing = false;

    // Button animation state
    private enum ButtonAnimState { Idle, Hover, Click }
    private ButtonAnimState[] buttonStates;
    private float[] buttonScales;

    private void Awake()
    {
        buttonStates = new ButtonAnimState[3];
        buttonScales = new float[3] { 1f, 1f, 1f };
    }

    private void Start()
    {
        SetupTitle();
        SetupButtons();
        SetupPanel();
        SetupBackground();
        SetupShipSelectors();

        time = 0f;
    }

    private void Update()
    {
        time += Time.deltaTime;
        AnimateTitle();
        AnimateButtons();
        AnimatePanel();
        AnimateBackground();
    }

    // ─── TITLE ────────────────────────────────────────────────────────────

    private void SetupTitle()
    {
        if (titleText == null) return;

        // Add shadow and outline for depth
        AddOutlineToText(titleText, 4f, new Color(0f, 0f, 0f, 0.8f));
        AddShadowToText(titleText, new Vector2(3f, -3f), new Color(0f, 0.3f, 0.8f, 0.7f), 8f);

        // Set font and style
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;

        titleBasePos = titleText.rectTransform.anchoredPosition3D;
    }

    private void AnimateTitle()
    {
        if (titleText == null) return;

        // Color cycling through space-themed colors (cool blues, purples, cyans)
        titleHue += Time.deltaTime * colorCycleSpeed;
        if (titleHue > 1f) titleHue -= 1f;

        float adjustedHue = Mathf.Repeat(titleHue * 0.4f + 0.55f, 1f);
        Color titleColor = UnityEngine.Color.HSVToRGB(adjustedHue, 0.9f, 1f);
        titleText.color = titleColor;

        // Subtle bob animation (smooth sine wave)
        float bob = Mathf.Sin(time * titleBobSpeed) * titleBobAmount;
        titleText.rectTransform.anchoredPosition3D = titleBasePos + new Vector3(0f, bob, 0f);

        // Subtitle fade/pulse
        if (subtitleText != null)
        {
            float alpha = 0.7f + Mathf.Sin(time * 1.5f) * 0.25f;
            subtitleText.color = new Color(0.6f, 0.85f, 1f, alpha);
        }
    }

    // ─── BUTTONS ─────────────────────────────────────────────────────────

    private void SetupButtons()
    {
        SetupButton(playButton, 0, new Color(0.15f, 0.75f, 0.3f, 1f), OnPlayClicked);
        SetupButton(instructionsButton, 1, new Color(0.25f, 0.45f, 0.9f, 1f), OnInstructionsClicked);
        SetupButton(quitButton, 2, new Color(0.85f, 0.2f, 0.2f, 1f), OnQuitClicked);
    }

    private void SetupButton(Button btn, int index, Color baseColor, UnityEngine.Events.UnityAction callback)
    {
        if (btn == null) return;

        btn.onClick.AddListener(callback);

        // Style the button's image
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = baseColor;
        }

        // Add Outline component for glow border
        Outline outline = btn.GetComponent<Outline>();
        if (outline == null)
            outline = btn.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(1f, 1f, 1f, 0.25f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Ensure button has a Transition for ColorTint
        ColorBlock colors = btn.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = new Color(baseColor.r * 1.35f, baseColor.g * 1.35f, baseColor.b * 1.35f);
        colors.pressedColor = new Color(baseColor.r * 0.75f, baseColor.g * 0.75f, baseColor.b * 0.75f);
        colors.disabledColor = new Color(baseColor.r * 0.35f, baseColor.g * 0.35f, baseColor.b * 0.35f, 0.5f);
        colors.colorMultiplier = 1f;
        btn.colors = colors;

        btn.transition = Selectable.Transition.ColorTint;
    }

    private void AnimateButtons()
    {
        AnimateButton(playButton, 0);
        AnimateButton(instructionsButton, 1);
        AnimateButton(quitButton, 2);
    }

    private void AnimateButton(Button btn, int index)
    {
        if (btn == null) return;

        float targetScale = 1f;

        // Hover detection (simplified — actual hover is handled by EventSystem)
        // We do a subtle idle pulse
        targetScale = 1f + Mathf.Sin(time * buttonAnimSpeed * 0.5f + index * 1.2f) * 0.012f;

        buttonScales[index] = Mathf.Lerp(buttonScales[index], targetScale, Time.deltaTime * buttonAnimSpeed);
        btn.transform.localScale = Vector3.one * buttonScales[index];
    }

    // ─── PANEL ───────────────────────────────────────────────────────────

    private void SetupPanel()
    {
        if (shipSelectionPanel == null) return;

        // Add outline to panel for glowing frame
        Outline panelOutline = shipSelectionPanel.GetComponent<Outline>();
        if (panelOutline == null)
            panelOutline = shipSelectionPanel.gameObject.AddComponent<Outline>();

        panelOutline.effectColor = new Color(0.25f, 0.55f, 1f, 0.6f);
        panelOutline.effectDistance = new Vector2(3f, -3f);

        // Panel base color
        shipSelectionPanel.color = new Color(0.02f, 0.04f, 0.14f, 0.95f);
    }

    private void AnimatePanel()
    {
        if (shipSelectionPanel == null) return;

        // Pulse alpha and outline glow together
        float pulse = 0.88f + Mathf.Sin(time * panelPulseSpeed) * panelPulseAmount;
        Color c = shipSelectionPanel.color;
        shipSelectionPanel.color = new Color(c.r, c.g, c.b, pulse);

        // Pulse the outline glow
        Outline outline = shipSelectionPanel.GetComponent<Outline>();
        if (outline != null)
        {
            float glowPulse = 0.5f + Mathf.Sin(time * panelPulseSpeed * 0.8f) * 0.15f;
            outline.effectColor = new Color(0.25f, 0.55f, 1f, glowPulse);
        }
    }

    // ─── BACKGROUND ──────────────────────────────────────────────────────

    private void SetupBackground()
    {
        if (backgroundOverlay == null) return;
        backgroundOverlay.color = new Color(0f, 0f, 0.02f, 0f);
    }

    private void AnimateBackground()
    {
        if (backgroundOverlay == null) return;

        float pulse = Mathf.Sin(time * overlayPulseSpeed) * 0.01f;
        backgroundOverlay.color = new Color(0f, 0f, 0.05f + pulse, 0.15f);
    }

    // ─── SHIP SELECTORS ─────────────────────────────────────────────────

    private void SetupShipSelectors()
    {
        if (shipSelectors == null || shipSelectors.Length == 0)
            shipSelectors = FindObjectsOfType<ShipSelector>();

        foreach (var selector in shipSelectors)
        {
            if (selector == null) continue;

            // Add outline to ship buttons
            Outline outline = selector.GetComponent<Outline>();
            if (outline == null)
                outline = selector.gameObject.AddComponent<Outline>();

            outline.effectColor = new Color(0.5f, 0.8f, 1f, 0.2f);
            outline.effectDistance = new Vector2(2f, 2f);
        }
    }

    // ─── CLICK HANDLERS ─────────────────────────────────────────────────

    private void OnPlayClicked()
    {
        AudioManager.Instance?.StopBackgroundMusic();
        GameManager.Instance?.StartGame();
    }

    private void OnInstructionsClicked()
    {
        instructionsShowing = !instructionsShowing;
        // Toggle instructions panel visibility
        GameObject instructionsPanel = GameObject.Find("InstructionsPanel");
        if (instructionsPanel != null)
            instructionsPanel.SetActive(instructionsShowing);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenuStyler] Quit game requested.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ─── UTILITY ─────────────────────────────────────────────────────────

    private void AddOutlineToText(Text txt, float spread, Color color)
    {
        if (txt == null) return;
        Outline outline = txt.GetComponent<Outline>();
        if (outline == null)
            outline = txt.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(spread, -spread);
        outline.useGraphicAlpha = true;
    }

    private void AddShadowToText(Text txt, Vector2 offset, Color color, float blur)
    {
        if (txt == null) return;
        Shadow shadow = txt.GetComponent<Shadow>();
        if (shadow == null)
            shadow = txt.gameObject.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = offset;
        shadow.useGraphicAlpha = true;
    }
}
