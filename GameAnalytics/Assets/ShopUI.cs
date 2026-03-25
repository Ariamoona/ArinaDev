using UnityEngine;
using UnityEngine.UI;
using GameAnalyticsIntegration;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button buyHealthButton;
    [SerializeField] private Button buyPowerupButton;

    private void Start()
    {
        openShopButton.onClick.AddListener(OnShopOpened);
        buyHealthButton.onClick.AddListener(() => BuyItem("health_potion", 50));
        buyPowerupButton.onClick.AddListener(() => BuyItem("damage_boost", 100));
    }

    private void OnShopOpened()
    {
        AnalyticsEvents.SendDesignEvent("shop:open", 1);
        Debug.Log("[Analytics] Shop opened");
    }

    private void BuyItem(string itemId, int cost)
    {
        GameManager.Instance?.PurchaseItem(itemId, cost);
    }
}