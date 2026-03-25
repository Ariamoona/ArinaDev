using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BackgroundScaler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool scaleToFillScreen = true;
    [SerializeField] private bool preserveAspectRatio = true;
    [SerializeField] private bool updateOnOrientationChange = true;

    private RectTransform rectTransform;
    private RawImage rawImage;
    private Image image;
    private ScreenOrientation currentOrientation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
        image = GetComponent<Image>();
        currentOrientation = Screen.orientation;
    }

    private void Start()
    {
        if (scaleToFillScreen)
        {
            ScaleToScreen();
        }
    }

    private void Update()
    {
        if (updateOnOrientationChange && Screen.orientation != currentOrientation)
        {
            currentOrientation = Screen.orientation;
            ScaleToScreen();
        }
    }

    public void ScaleToScreen()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        if (preserveAspectRatio && (rawImage != null || image != null))
        {
            AdjustTextureAspect();
        }

        Debug.Log($"[BackgroundScaler] Scaled background to screen: {Screen.width}x{Screen.height}");
    }

    private void AdjustTextureAspect()
    {
        Texture texture = null;

        if (rawImage != null && rawImage.texture != null)
            texture = rawImage.texture;
        else if (image != null && image.sprite != null)
            texture = image.sprite.texture;

        if (texture == null)
            return;

        float textureAspect = (float)texture.width / texture.height;
        float screenAspect = (float)Screen.width / Screen.height;

        if (rectTransform != null)
        {
            if (screenAspect > textureAspect)
            {
                float scale = screenAspect / textureAspect;
                rectTransform.localScale = new Vector3(scale, 1f, 1f);
            }
            else
            {
                float scale = textureAspect / screenAspect;
                rectTransform.localScale = new Vector3(1f, scale, 1f);
            }
        }
    }
}