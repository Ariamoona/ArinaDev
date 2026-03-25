using UnityEngine;
using UnityEngine.Advertisements;
using System;

namespace UnityAdsIntegration
{
    public class AdService : MonoBehaviour, IUnityAdsInitializationListener,
                                   IUnityAdsLoadListener, IUnityAdsShowListener
    {
        private static AdService _instance;
        public static AdService Instance => _instance;

        [Header("Unity Ads IDs")]
        [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
        [SerializeField] private string iOSGameId = "YOUR_IOS_GAME_ID";

        [Header("Ad Units")]
        [SerializeField] private string androidRewardedId = "Rewarded_Android";
        [SerializeField] private string iOSRewardedId = "Rewarded_iOS";
        [SerializeField] private string androidBannerId = "Banner_Android";
        [SerializeField] private string iOSBannerId = "Banner_iOS";

        [Header("Banner Settings")]
        [SerializeField] private BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

        private string _gameId;
        private string _rewardedAdUnitId;
        private string _bannerAdUnitId;

        private bool _isInitialized = false;
        private bool _isRewardedAdLoaded = false;
        private Action _onRewardGranted;
        private Action<string> _onAdError;

        public bool IsInitialized => _isInitialized;
        public bool IsRewardedAdReady => _isRewardedAdLoaded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePlatformSettings();
        }

        private void InitializePlatformSettings()
        {
#if UNITY_IOS
                _gameId = iOSGameId;
                _rewardedAdUnitId = iOSRewardedId;
                _bannerAdUnitId = iOSBannerId;
#elif UNITY_ANDROID
            _gameId = androidGameId;
            _rewardedAdUnitId = androidRewardedId;
            _bannerAdUnitId = androidBannerId;
#else
                _gameId = androidGameId;
                _rewardedAdUnitId = androidRewardedId;
                _bannerAdUnitId = androidBannerId;
                Debug.LogWarning("Unity Ads: Running in editor, using Android Game ID for testing");
#endif
        }

        private void Start()
        {
            InitializeAds();
        }

        public void InitializeAds()
        {
            if (_isInitialized)
            {
                Debug.Log("Unity Ads: Already initialized");
                return;
            }

            if (string.IsNullOrEmpty(_gameId))
            {
                Debug.LogError("Unity Ads: Game ID is not set!");
                return;
            }

            Debug.Log($"Unity Ads: Initializing with Game ID: {_gameId}");
            Advertisement.Initialize(_gameId, true, this);
        }

        public void OnInitializationComplete()
        {
            _isInitialized = true;
            Debug.Log("Unity Ads: Initialization complete!");

            LoadRewardedAd();
        }
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            _isInitialized = false;
            Debug.LogError($"Unity Ads: Initialization failed - {error}: {message}");
        }

        public void LoadRewardedAd()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Unity Ads: Not initialized yet, cannot load rewarded ad");
                return;
            }

            Debug.Log($"Unity Ads: Loading rewarded ad for placement: {_rewardedAdUnitId}");
            Advertisement.Load(_rewardedAdUnitId, this);
        }

        public void ShowRewardedAd(Action onRewardGranted, Action<string> onError = null)
        {
            if (!_isRewardedAdLoaded)
            {
                string errorMsg = "Rewarded ad not loaded yet";
                Debug.LogWarning($"Unity Ads: {errorMsg}");
                onError?.Invoke(errorMsg);

                LoadRewardedAd();
                return;
            }

            _onRewardGranted = onRewardGranted;
            _onAdError = onError;

            Debug.Log("Unity Ads: Showing rewarded ad");
            Advertisement.Show(_rewardedAdUnitId, this);
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == _rewardedAdUnitId)
            {
                _isRewardedAdLoaded = true;
                Debug.Log($"Unity Ads: Rewarded ad loaded for placement: {placementId}");
            }
            else if (placementId == _bannerAdUnitId)
            {
                Debug.Log($"Unity Ads: Banner ad loaded for placement: {placementId}");
            }
        }
        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            if (placementId == _rewardedAdUnitId)
            {
                _isRewardedAdLoaded = false;
                Debug.LogError($"Unity Ads: Failed to load rewarded ad - {error}: {message}");
            }
            else if (placementId == _bannerAdUnitId)
            {
                Debug.LogError($"Unity Ads: Failed to load banner ad - {error}: {message}");
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log($"Unity Ads: Ad started for placement: {placementId}");
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log($"Unity Ads: Ad completed for placement: {placementId}, state: {showCompletionState}");

            if (placementId == _rewardedAdUnitId)
            {
                _isRewardedAdLoaded = false;

                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                {
                    Debug.Log("Unity Ads: Reward granted!");
                    _onRewardGranted?.Invoke();
                }
                else
                {
                    Debug.Log("Unity Ads: User did not complete the ad, reward not granted");
                }

                LoadRewardedAd();
            }
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"Unity Ads: Show failed for {placementId} - {error}: {message}");

            if (placementId == _rewardedAdUnitId)
            {
                _isRewardedAdLoaded = false;
                _onAdError?.Invoke($"{error}: {message}");
                LoadRewardedAd();
            }
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log($"Unity Ads: Ad clicked for placement: {placementId}");
        }

        public void ShowBanner()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Unity Ads: Not initialized yet, cannot show banner");
                return;
            }

            Debug.Log($"Unity Ads: Showing banner at {bannerPosition}");

            Advertisement.Banner.SetPosition(bannerPosition);

            Advertisement.Banner.Load(_bannerAdUnitId, new BannerLoadOptions
            {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerLoadError
            });
        }

        private void OnBannerLoaded()
        {
            Debug.Log("Unity Ads: Banner loaded, showing...");
            Advertisement.Banner.Show(_bannerAdUnitId);
        }

        private void OnBannerLoadError(string message)
        {
            Debug.LogError($"Unity Ads: Banner load error: {message}");
        }

        public void HideBanner()
        {
            Debug.Log("Unity Ads: Hiding banner");
            Advertisement.Banner.Hide();
        }

        public void DestroyBanner()
        {
            Debug.Log("Unity Ads: Destroying banner");
            Advertisement.Banner.Hide();
        }
    }
}