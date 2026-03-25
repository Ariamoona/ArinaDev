using System;
using UnityEngine;
using UnityEngine.Events;

namespace GamePushIntegration
{
    public class GamePushManager : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent OnSDKInitialized;
        public UnityEvent<string> OnPlayerAuthorized;
        public UnityEvent<string> OnPlayerAuthFailed;

        [Header("Settings")]
        [SerializeField] private int gameId = 1;
        [SerializeField] private string secretKey = "your_secret_key"; 

        private static GamePushManager _instance;
        public static GamePushManager Instance => _instance;

        private bool _isInitialized = false;
        private bool _isPlayerAuthorized = false;

        public bool IsInitialized => _isInitialized;
        public bool IsPlayerAuthorized => _isPlayerAuthorized;

        public object GamePush { get; private set; }

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
            InitializeSDK();
        }

        private void InitializeSDK()
        {
            Debug.Log("[GamePush] Initializing SDK...");

            GamePush.GP_Init.OnInitComplete += OnSDKInitComplete;
            GamePush.GP_Init.OnInitError += OnSDKInitError;

            GamePush.GP_Init.Init(gameId, secretKey);
        }

        private void OnSDKInitComplete()
        {
            _isInitialized = true;
            Debug.Log("[GamePush] SDK initialized successfully!");

            GamePush.GP_Player.OnPlayerAuthorized += OnPlayerAuthSuccess;
            GamePush.GP_Player.OnPlayerAuthorizeError += OnPlayerAuthError;
            GamePush.GP_Player.OnPlayerFetchData += OnPlayerDataFetched;

            OnSDKInitialized?.Invoke();

            AuthorizePlayer();
        }

        private void OnSDKInitError(string error)
        {
            Debug.LogError($"[GamePush] SDK initialization error: {error}");
            OnPlayerAuthFailed?.Invoke($"SDK Init Error: {error}");
        }

        public void AuthorizePlayer()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[GamePush] SDK not initialized yet!");
                return;
            }

            Debug.Log("[GamePush] Authorizing player...");
            GamePush.GP_Player.Authorize();
        }

        private void OnPlayerAuthSuccess()
        {
            _isPlayerAuthorized = true;
            string playerId = GamePush.GP_Player.GetID();
            string playerName = GamePush.GP_Player.GetName();

            Debug.Log($"[GamePush] Player authorized! ID: {playerId}, Name: {playerName}");
            OnPlayerAuthorized?.Invoke(playerId);

            CloudSaveManager.Instance?.LoadGameData();
        }

        private void OnPlayerAuthError(string error)
        {
            Debug.LogError($"[GamePush] Player authorization error: {error}");
            OnPlayerAuthFailed?.Invoke(error);

            CloudSaveManager.Instance?.LoadFromCache();
        }

        private void OnPlayerDataFetched()
        {
            Debug.Log("[GamePush] Player data fetched from platform");
        }

        private void OnDestroy()
        {
            if (GamePush.GP_Init.OnInitComplete != null)
                GamePush.GP_Init.OnInitComplete -= OnSDKInitComplete;
            if (GamePush.GP_Init.OnInitError != null)
                GamePush.GP_Init.OnInitError -= OnSDKInitError;
            if (GamePush.GP_Player.OnPlayerAuthorized != null)
                GamePush.GP_Player.OnPlayerAuthorized -= OnPlayerAuthSuccess;
            if (GamePush.GP_Player.OnPlayerAuthorizeError != null)
                GamePush.GP_Player.OnPlayerAuthorizeError -= OnPlayerAuthError;
            if (GamePush.GP_Player.OnPlayerFetchData != null)
                GamePush.GP_Player.OnPlayerFetchData -= OnPlayerDataFetched;
        }
    }
}