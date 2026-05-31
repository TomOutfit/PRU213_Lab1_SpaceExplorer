using UnityEngine;

/// <summary>
/// Scrolling star-field background: stars move downward and wrap back to the top
/// when they pass below the camera, creating a continuous parallax space feel.
/// </summary>
public class StarField : MonoBehaviour
{
    [Header("Star Objects")]
    public Transform[] backgroundStars;

    [Header("Scrolling")]
    public float starScrollSpeed = 2f;
    public float backgroundHeight = 20f;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (mainCam == null || backgroundStars == null) return;

        float camBottom = mainCam.transform.position.y - mainCam.orthographicSize - 1f;
        float camTop = mainCam.transform.position.y + mainCam.orthographicSize + 1f;

        foreach (Transform star in backgroundStars)
        {
            if (star == null) continue;

            star.Translate(Vector3.down * starScrollSpeed * Time.deltaTime, Space.World);

            if (star.position.y < camBottom)
            {
                float newY = camTop + Random.Range(0f, 5f);
                float newX = mainCam.transform.position.x + Random.Range(-10f, 10f);
                star.position = new Vector3(newX, newY, star.position.z);
            }
        }
    }
}
