using UnityEngine;
using System.Collections.Generic;

public class TestWeaponConfig : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool testOnStart = true;
    [SerializeField] private float testInterval = 5f;

    private RemoteConfigLoader configLoader;
    private float timer;

    private void Start()
    {
        configLoader = GetComponent<RemoteConfigLoader>();

        if (configLoader == null)
        {
            Debug.LogError("RemoteConfigLoader not found!");
            return;
        }

        if (testOnStart)
        {
            TestConfig();
        }
    }

    private void Update()
    {
        if (testInterval > 0)
        {
            timer += Time.deltaTime;
            if (timer >= testInterval)
            {
                timer = 0;
                TestConfig();
            }
        }
    }

    private void TestConfig()
    {
        Debug.Log("=== Testing Weapon Configuration ===");

        var allConfigs = configLoader.GetAllConfigs();
        Debug.Log($"Loaded {allConfigs.Count} weapon configs");

        foreach (var config in allConfigs)
        {
            Debug.Log($"Weapon ID: {config.id}, Damage: {config.damage}, Cooldown: {config.cooldown}");
        }

        Debug.Log("=== End Test ===");
    }
}