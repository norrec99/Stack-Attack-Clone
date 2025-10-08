using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private float scrollSpeed = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (rawImage == null) return;

        Rect uv = rawImage.uvRect;
        uv.y += scrollSpeed * Time.deltaTime;

        if (uv.y > 1f) uv.y -= Mathf.Floor(uv.y);
        if (uv.y < 0f) uv.y += Mathf.Ceil(-uv.y);

        rawImage.uvRect = uv;
    }
}
