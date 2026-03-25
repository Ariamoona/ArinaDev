using UnityEngine;
using GameAnalyticsSDK;
using System.Collections.Generic;

namespace GameAnalyticsIntegration
{
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("GameAnalytics Keys")]
        [SerializeField] private string gameKey = "YOUR_GAME_KEY_HERE";
        [SerializeField] private string secretKey = "YOUR_SECRET_KEY_HERE";
        [SerializeField] private string buildVersion = "1.0.0";

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool manualSessionHandling = false;

        private bool _isInitialized = false;
        private string _currentLevelId = "level_1";

        public bool IsInitialized => _isInitialized;
        public string CurrentLevelId => _currentLevelId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeGameAnalytics();
        }

        private void InitializeGameAnalytics()
        {
            Debug.Log("[AnalyticsManager] Initializing GameAnalytics...");

            if (enableDebugLogging)
            {
                GameAnalytics.SetDebugLogEnabled(true);
                Debug.Log("[AnalyticsManager] Debug logging enabled");
            }

            GameAnalytics.SetBuildVersion(buildVersion);

            GameAnalytics.SetCustomDimension01("player_skill_beginner");
            GameAnalytics.SetCustomDimension02("game_mode_normal");

            GameAnalytics.Initialize(gameKey, secretKey);

            GameAnalytics.OnInitialized += OnAnalyticsInitialized;
        }

        private void OnAnalyticsInitialized()
        {
            _isInitialized = true;
            Debug.Log("[AnalyticsManager] GameAnalytics initialized successfully!");

            GameAnalytics.AddResourceCurrency("coins");
            GameAnalytics.AddResourceCurrency("gems");
            GameAnalytics.AddResourceItemType("collectable");
            GameAnalytics.AddResourceItemType("powerup");

            AnalyticsEvents.SendSessionStart();

            GameAnalytics.OnInitialized -= OnAnalyticsInitialized;
        }

        private void OnApplicationQuit()
        {
            if (_isInitialized)
            {
                AnalyticsEvents.SendSessionEnd();
                GameAnalytics.Shutdown();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!_isInitialized) return;

            if (pauseStatus)
            {
                AnalyticsEvents.SendSessionEnd();
            }
            else
            {
                AnalyticsEvents.SendSessionStart();
            }
        }

        public void SetCurrentLevel(string levelId)
        {
            _currentLevelId = levelId;
        }
    }
}