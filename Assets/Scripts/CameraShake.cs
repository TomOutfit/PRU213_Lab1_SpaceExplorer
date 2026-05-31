using System.Collections;
using UnityEngine;

/// <summary>
/// Handles screen shake effects for impacts and explosions.
/// Add this to the Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    public static CameraShake Instance { get { return instance; } }

    private Vector3 originalPos;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
