using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HUDPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button leftActionButton;
    [SerializeField] private Button rightActionButton;
    [SerializeField] private Text topBarText;
    [SerializeField] private Image topBarLogo;

    [Header("Events")]
    public UnityEvent OnLeftActionPressed;
    public UnityEvent OnRightActionPressed;

    [Header("Settings")]
    [SerializeField] private string topBarTitle = "Game Title";
    [SerializeField] private Sprite topBarLogoSprite;

    private void Start()
    {
        SetupButtons();
        SetupTopBar();
    }

    private void SetupButtons()
    {
        if (leftActionButton != null)
        {
            leftActionButton.onClick.AddListener(() =>
            {
                Debug.Log("[HUD] Left Action Button Pressed");
                OnLeftActionPressed?.Invoke();
            });
        }

        if (rightActionButton != null)
        {
            rightActionButton.onClick.AddListener(() =>
            {
                Debug.Log("[HUD] Right Action Button Pressed");
                OnRightActionPressed?.Invoke();
            });
        }
    }

    private void SetupTopBar()
    {
        if (topBarText != null)
        {
            topBarText.text = topBarTitle;
        }

        if (topBarLogo != null && topBarLogoSprite != null)
        {
            topBarLogo.sprite = topBarLogoSprite;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[HUD] Space pressed - simulating Left Action");
            OnLeftActionPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("[HUD] Enter pressed - simulating Right Action");
            OnRightActionPressed?.Invoke();
        }
    }
}