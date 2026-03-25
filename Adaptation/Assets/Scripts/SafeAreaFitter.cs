using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool fitOnStart = true;
    [SerializeField] private bool fitOnOrientationChange = true;
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Events")]
    public UnityEvent<Rect> OnSafeAreaChanged;

    private RectTransform rectTransform;
    private Rect currentSafeArea;
    private ScreenOrientation currentOrientation;
    private float updateTimer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        currentOrientation = Screen.orientation;
    }

    private void Start()
    {
        if (fitOnStart)
        {
            FitToSafeArea();
        }
    }

    private void Update()
    {
        if (fitOnOrientationChange)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0;
                CheckAndUpdate();
            }
        }
    }

    private void CheckAndUpdate()
    {
        bool needsUpdate = false;

        if (Screen.orientation != currentOrientation)
        {
            currentOrientation = Screen.orientation;
            needsUpdate = true;
            Debug.Log($"[SafeAreaFitter] Orientation changed to: {currentOrientation}");
        }
        if (Screen.safeArea != currentSafeArea)
        {
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            FitToSafeArea();
        }
    }

    public void FitToSafeArea()
    {
        if (rectTransform == null)
            return;

        Rect safeArea = Screen.safeArea;
        currentSafeArea = safeArea;

        Debug.Log($"[SafeAreaFitter] Screen size: {Screen.width}x{Screen.height}, Safe Area: {safeArea}");

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        OnSafeAreaChanged?.Invoke(safeArea);

        Debug.Log($"[SafeAreaFitter] Applied anchors: min={anchorMin}, max={anchorMax}");
    }

    public void ForceUpdate()
    {
        FitToSafeArea();
    }
}