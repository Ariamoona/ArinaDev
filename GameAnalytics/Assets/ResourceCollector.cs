using UnityEngine;
using GameAnalyticsIntegration;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] private int resourceValue = 10;
    [SerializeField] private string resourceType = "coin";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.AddScore(resourceValue);

            AnalyticsEvents.SendDesignEvent($"enemy:kill:{resourceType}", resourceValue);

            Destroy(gameObject);
        }
    }
}