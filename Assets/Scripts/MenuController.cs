using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu scene: handles Play, Instructions, and Quit button clicks,
/// and manages the visibility of the Instructions panel.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button guideButton;
    public Button closeGuideButton;
    public Button quitButton;
    public GameObject guideCanva;

    [Header("Ship Selection (Optional Visual Variety)")]
    public Sprite[] menuBackgroundShipSprites;

    private void Start()
    {
        // Instantiate beautiful dynamic space background
        if (FindAnyObjectByType<SpaceBackgroundEffects>() == null)
        {
            GameObject bgObj = new GameObject("SpaceBackgroundEffects");
            bgObj.AddComponent<SpaceBackgroundEffects>();
        }

        // Hide guide panel at start
        if (guideCanva != null)
            guideCanva.SetActive(false);

        // Wire up button listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (guideButton != null)
            guideButton.onClick.AddListener(OnGuideClicked);

        if (closeGuideButton != null)
            closeGuideButton.onClick.AddListener(OnCloseGuideClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
            
        // Add UI Button Animation dynamically to all buttons
        Button[] allButtons = { playButton, guideButton, closeGuideButton, quitButton };
        foreach (Button btn in allButtons)
        {
            if (btn != null && btn.gameObject.GetComponent<UIButtonAnimation>() == null)
            {
                btn.gameObject.AddComponent<UIButtonAnimation>();
            }
        }
    }

    public void OnPlayClicked()
    {
        Debug.Log("[MenuController] Play clicked — loading Play scene.");
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
    }

    public void OnGuideClicked()
    {
        Debug.Log("[MenuController] Guide panel opened.");
        if (guideCanva != null)
            guideCanva.SetActive(true);
    }

    public void OnCloseGuideClicked()
    {
        Debug.Log("[MenuController] Guide panel closed.");
        if (guideCanva != null)
            guideCanva.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Debug.Log("[MenuController] Quit clicked.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
        if (guideButton != null)
            guideButton.onClick.RemoveListener(OnGuideClicked);
        if (closeGuideButton != null)
            closeGuideButton.onClick.RemoveListener(OnCloseGuideClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
}
