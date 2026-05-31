using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Adds a hover scale animation to UI Buttons dynamically.
/// </summary>
public class UIButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float hoverScaleMultiplier = 1.1f;
    public float animationSpeed = 10f;
    
    private bool isHovering = false;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        Vector3 targetScale = isHovering ? originalScale * hoverScaleMultiplier : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}
