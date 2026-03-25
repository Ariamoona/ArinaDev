using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePushIntegration
{
    public class UIController : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI authStatusText;

        [Header("Game Stats")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI coinsText;

        [Header("Buttons")]
        [SerializeField] private Button authorizeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button rewardedAdButton;
        [SerializeField] private Button interstitialAdButton;
        [SerializeField] private Button addScoreButton;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;

        private int _currentScore = 0;
        private int _currentLevel = 1;
        private int _currentCoins = 0;

        private void Start()
        {
            SetupButtons();
            SubscribeToEvents();
            UpdateUI();
        }

        private void SetupButtons()
        {
            authorizeButton.onClick.AddListener(OnAuthorizeClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            rewardedAdButton.onClick.AddListener(OnRewardedAdClicked);
            interstitialAdButton.onClick.AddListener(OnInterstitialAdClicked);
            addScoreButton.onClick.AddListener(OnAddScoreClicked);

            SetButtonsInteractable(false);
        }

        private void SubscribeToEvents()
        {
            GamePushManager.Instance.OnSDKInitialized.AddListener(OnSDKReady);
            GamePushManager.Instance.OnPlayerAuthorized.AddListener(OnPlayerAuthorized);
            GamePushManager.Instance.OnPlayerAuthFailed.AddListener(OnPlayerAuthFailed);

            CloudSaveManager.Instance.OnDataLoaded += OnDataLoaded;
            CloudSaveManager.Instance.OnDataSaved += OnDataSaved;

            AdManager.Instance.OnRewardReceived += OnRewardReceived;
            AdManager.Instance.OnAdError += OnAdError;
        }

        private void OnSDKReady()
        {
            SetButtonsInteractable(true);
            UpdateStatus("SDK Ready. Authorizing...");
        }

        private void OnAuthorizeClicked()
        {
            GamePushManager.Instance.AuthorizePlayer();
            UpdateStatus("Authorizing...");
        }

        private void OnPlayerAuthorized(string playerId)
        {
            playerIdText.text = $"ID: {playerId}";
            playerNameText.text = $"Name: {GamePush.GP_Player.GetName()}";
            authStatusText.text = "Authorized";
            authStatusText.color = Color.green;
            UpdateStatus("Authorized successfully!");
        }

        private void OnPlayerAuthFailed(string error)
        {
            authStatusText.text = "Failed";
            authStatusText.color = Color.red;
            UpdateStatus($"Auth failed: {error}");
        }

        private void OnDataLoaded(PlayerData data)
        {
            _currentScore = data.score;
            _currentLevel = data.level;
            _currentCoins = data.coins;
            UpdateUI();
            UpdateStatus("Game data loaded!");
        }

        private void OnDataSaved()
        {
            UpdateStatus("Game data saved to cloud!");
        }

        private void OnSaveClicked()
        {
            CloudSaveManager.Instance.UpdatePlayerProgress(_currentScore, _currentLevel, _currentCoins);
            UpdateStatus("Saving data...");
        }

        private void OnLeaderboardClicked()
        {
            LeaderboardManager.Instance.SubmitScore(_currentScore);
            LeaderboardManager.Instance.ShowLeaderboard();
            UpdateStatus("Opening leaderboard...");
        }

        private void OnRewardedAdClicked()
        {
            if (AdManager.Instance.IsRewardedReady)
            {
                AdManager.Instance.ShowRewardedAd();
                UpdateStatus("Showing rewarded ad...");
            }
            else
            {
                UpdateStatus("Rewarded ad not ready, please wait...");
            }
        }

        private void OnInterstitialAdClicked()
        {
            if (AdManager.Instance.IsInterstitialReady)
            {
                AdManager.Instance.ShowInterstitialAd();
                UpdateStatus("Showing interstitial ad...");
            }
            else
            {
                UpdateStatus("Interstitial ad not ready, please wait...");
            }
        }

        private void OnAddScoreClicked()
        {
            _currentScore += 100;
            _currentCoins += 50;
            UpdateUI();
            UpdateStatus($"Added points! New score: {_currentScore}");
        }

        private void OnRewardReceived()
        {
            _currentCoins += 200;
            UpdateUI();
            UpdateStatus("Reward received! +200 coins!");
        }

        private void OnAdError(string error)
        {
            UpdateStatus($"Ad error: {error}");
        }

        private void UpdateUI()
        {
            scoreText.text = $"Score: {_currentScore}";
            levelText.text = $"Level: {_currentLevel}";
            coinsText.text = $"Coins: {_currentCoins}";
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{DateTime.Now:HH:mm:ss}] {message}";
                Debug.Log($"[UI] {message}");
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            authorizeButton.interactable = interactable;
            saveButton.interactable = interactable;
            leaderboardButton.interactable = interactable;
            rewardedAdButton.interactable = interactable;
            interstitialAdButton.interactable = interactable;
            addScoreButton.interactable = interactable;
        }

        private void OnDestroy()
        {
            if (GamePushManager.Instance != null)
            {
                GamePushManager.Instance.OnSDKInitialized.RemoveListener(OnSDKReady);
                GamePushManager.Instance.OnPlayerAuthorized.RemoveListener(OnPlayerAuthorized);
                GamePushManager.Instance.OnPlayerAuthFailed.RemoveListener(OnPlayerAuthFailed);
            }

            if (CloudSaveManager.Instance != null)
            {
                CloudSaveManager.Instance.OnDataLoaded -= OnDataLoaded;
                CloudSaveManager.Instance.OnDataSaved -= OnDataSaved;
            }

            if (AdManager.Instance != null)
            {
                AdManager.Instance.OnRewardReceived -= OnRewardReceived;
                AdManager.Instance.OnAdError -= OnAdError;
            }
        }
    }
}