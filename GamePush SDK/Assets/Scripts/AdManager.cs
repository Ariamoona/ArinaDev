using UnityEngine;
using System;

namespace GamePushIntegration
{
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        public static AdManager Instance => _instance;

        public event Action OnRewardReceived;
        public event Action OnAdClosed;
        public event Action<string> OnAdError;

        private bool _isRewardedReady = false;
        private bool _isInterstitialReady = false;

        public bool IsRewardedReady => _isRewardedReady;
        public bool IsInterstitialReady => _isInterstitialReady;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GamePush.GP_Ads.OnRewardedReady += OnRewardedReady;
            GamePush.GP_Ads.OnRewardedOpen += OnRewardedOpen;
            GamePush.GP_Ads.OnRewardedReward += OnRewardedReward;
            GamePush.GP_Ads.OnRewardedClose += OnRewardedClose;
            GamePush.GP_Ads.OnRewardedError += OnRewardedError;

            GamePush.GP_Ads.OnInterstitialReady += OnInterstitialReady;
            GamePush.GP_Ads.OnInterstitialOpen += OnInterstitialOpen;
            GamePush.GP_Ads.OnInterstitialClose += OnInterstitialClose;
            GamePush.GP_Ads.OnInterstitialError += OnInterstitialError;

            LoadRewardedAd();
            LoadInterstitialAd();
        }

        public void LoadRewardedAd()
        {
            if (GamePushManager.Instance.IsInitialized)
            {
                GamePush.GP_Ads.LoadRewarded();
            }
        }

        public void LoadInterstitialAd()
        {
            if (GamePushManager.Instance.IsInitialized)
            {
                GamePush.GP_Ads.LoadInterstitial();
            }
        }

        public void ShowRewardedAd()
        {
            if (_isRewardedReady)
            {
                GamePush.GP_Ads.ShowRewarded();
            }
            else
            {
                Debug.LogWarning("[Ads] Rewarded ad not ready");
                OnAdError?.Invoke("Rewarded ad not ready");
            }
        }

        public void ShowInterstitialAd()
        {
            if (_isInterstitialReady)
            {
                GamePush.GP_Ads.ShowInterstitial();
            }
            else
            {
                Debug.LogWarning("[Ads] Interstitial ad not ready");
                OnAdError?.Invoke("Interstitial ad not ready");
            }
        }

        private void OnRewardedReady()
        {
            _isRewardedReady = true;
            Debug.Log("[Ads] Rewarded ad ready");
        }

        private void OnRewardedOpen()
        {
            Debug.Log("[Ads] Rewarded ad opened");
        }

        private void OnRewardedReward()
        {
            Debug.Log("[Ads] Reward granted!");
            OnRewardReceived?.Invoke();
        }

        private void OnRewardedClose()
        {
            _isRewardedReady = false;
            Debug.Log("[Ads] Rewarded ad closed");
            OnAdClosed?.Invoke();
            LoadRewardedAd();
        }

        private void OnRewardedError(string error)
        {
            _isRewardedReady = false;
            Debug.LogError($"[Ads] Rewarded ad error: {error}");
            OnAdError?.Invoke(error);
        }

        private void OnInterstitialReady()
        {
            _isInterstitialReady = true;
            Debug.Log("[Ads] Interstitial ad ready");
        }

        private void OnInterstitialOpen()
        {
            Debug.Log("[Ads] Interstitial ad opened");
        }

        private void OnInterstitialClose()
        {
            _isInterstitialReady = false;
            Debug.Log("[Ads] Interstitial ad closed");
            OnAdClosed?.Invoke();
            LoadInterstitialAd();
        }

        private void OnInterstitialError(string error)
        {
            _isInterstitialReady = false;
            Debug.LogError($"[Ads] Interstitial ad error: {error}");
            OnAdError?.Invoke(error);
        }
    }
}