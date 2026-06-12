using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that creates a full-screen canvas to fade in and out between scenes.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;
    public static SceneTransitionManager Instance { get { return instance; } }

    private CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(this.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it's on top of everything

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image image = panelObj.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeGroup = panelObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Fade Out (to black)
        fadeGroup.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeGroup.alpha = 1f;

        // Load Scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Fade In (from black)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
    }
}
