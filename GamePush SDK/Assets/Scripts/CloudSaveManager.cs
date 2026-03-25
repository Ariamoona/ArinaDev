using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace GamePushIntegration
{
    public class CloudSaveManager : MonoBehaviour
    {
        private static CloudSaveManager _instance;
        public static CloudSaveManager Instance => _instance;

        [Header("Settings")]
        [SerializeField] private float syncRetryDelay = 30f; 
        [SerializeField] private int maxSyncAttempts = 3; 

        private PlayerData _currentPlayerData;
        private LocalCacheData _localCache;
        private bool _isSyncing = false;
        private int _syncAttempts = 0;
        private Coroutine _syncRetryCoroutine;

        public PlayerData CurrentPlayerData => _currentPlayerData;

        public object GamePush { get; private set; }

        public event Action<PlayerData> OnDataLoaded;
        public event Action OnDataSaved;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLocalCache();
        }

        private void LoadLocalCache()
        {
            string cacheKey = "GamePush_LocalCache";
            string cachedData = PlayerPrefs.GetString(cacheKey, "");

            if (!string.IsNullOrEmpty(cachedData))
            {
                try
                {
                    _localCache = JsonConvert.DeserializeObject<LocalCacheData>(cachedData);
                    _currentPlayerData = _localCache.playerData;
                    Debug.Log("[CloudSave] Local cache loaded");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CloudSave] Failed to load cache: {e.Message}");
                    CreateNewCache();
                }
            }
            else
            {
                CreateNewCache();
            }
        }

        private void CreateNewCache()
        {
            _localCache = new LocalCacheData();
            _currentPlayerData = _localCache.playerData;
            SaveLocalCache();
        }

        private void SaveLocalCache()
        {
            _localCache.playerData = _currentPlayerData;
            string cacheData = JsonConvert.SerializeObject(_localCache);
            PlayerPrefs.SetString("GamePush_LocalCache", cacheData);
            PlayerPrefs.Save();
        }

        public void LoadGameData()
        {
            if (!GamePushManager.Instance.IsPlayerAuthorized)
            {
                Debug.Log("[CloudSave] Player not authorized, loading from cache");
                LoadFromCache();
                return;
            }

            Debug.Log("[CloudSave] Loading game data from cloud...");

            GamePush.GP_Cloud.OnCloudGetData += OnCloudDataReceived;
            GamePush.GP_Cloud.OnCloudGetDataError += OnCloudDataError;

            GamePush.GP_Cloud.GetData();
        }

        private void OnCloudDataReceived(string data)
        {
            GamePush.GP_Cloud.OnCloudGetData -= OnCloudDataReceived;
            GamePush.GP_Cloud.OnCloudGetDataError -= OnCloudDataError;

            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var cloudData = JsonConvert.DeserializeObject<PlayerData>(data);
                    if (cloudData != null)
                    {
                        if (cloudData.lastSaveTime > _currentPlayerData.lastSaveTime)
                        {
                            _currentPlayerData = cloudData;
                            Debug.Log("[CloudSave] Cloud data loaded, it's newer");
                        }
                        else
                        {
                            Debug.Log("[CloudSave] Local data is newer, will sync to cloud");
                            SaveGameData();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CloudSave] Failed to parse cloud data: {e.Message}");
                    LoadFromCache();
                }
            }
            else
            {
                LoadFromCache();
                SaveGameData();
            }

            UpdateLocalCache();
            OnDataLoaded?.Invoke(_currentPlayerData);
        }

        private void OnCloudDataError(string error)
        {
            GamePush.GP_Cloud.OnCloudGetData -= OnCloudDataReceived;
            GamePush.GP_Cloud.OnCloudGetDataError -= OnCloudDataError;

            Debug.LogError($"[CloudSave] Failed to load cloud data: {error}");
            LoadFromCache();
        }

        public void LoadFromCache()
        {
            Debug.Log("[CloudSave] Loading from local cache");
            _currentPlayerData = _localCache.playerData;
            OnDataLoaded?.Invoke(_currentPlayerData);
        }

        public void SaveGameData()
        {
            if (!GamePushManager.Instance.IsPlayerAuthorized)
            {
                Debug.Log("[CloudSave] Player not authorized, saving to cache only");
                UpdateLocalCache();

                _localCache.hasPendingSync = true;
                _localCache.pendingSyncData = JsonConvert.SerializeObject(_currentPlayerData);
                SaveLocalCache();

                StartSyncRetry();
                return;
            }

            if (_isSyncing)
            {
                Debug.Log("[CloudSave] Already syncing, queuing save");
                _localCache.hasPendingSync = true;
                _localCache.pendingSyncData = JsonConvert.SerializeObject(_currentPlayerData);
                SaveLocalCache();
                return;
            }

            _isSyncing = true;
            Debug.Log("[CloudSave] Saving game data to cloud...");

            string saveData = JsonConvert.SerializeObject(_currentPlayerData);

            GamePush.GP_Cloud.OnCloudSetData += OnCloudDataSaved;
            GamePush.GP_Cloud.OnCloudSetDataError += OnCloudSaveError;
            GamePush.GP_Cloud.SetData(saveData);
        }

        private void OnCloudDataSaved()
        {
            GamePush.GP_Cloud.OnCloudSetData -= OnCloudDataSaved;
            GamePush.GP_Cloud.OnCloudSetDataError -= OnCloudSaveError;

            _isSyncing = false;
            _syncAttempts = 0;
            _localCache.hasPendingSync = false;
            _localCache.pendingSyncData = "";
            _localCache.lastSyncAttempt = DateTime.Now;
            SaveLocalCache();

            Debug.Log("[CloudSave] Data saved to cloud successfully!");
            OnDataSaved?.Invoke();
        }

        private void OnCloudSaveError(string error)
        {
            GamePush.GP_Cloud.OnCloudSetData -= OnCloudDataSaved;
            GamePush.GP_Cloud.OnCloudSetDataError -= OnCloudSaveError;

            _isSyncing = false;
            Debug.LogError($"[CloudSave] Failed to save to cloud: {error}");

            _localCache.hasPendingSync = true;
            _localCache.pendingSyncData = JsonConvert.SerializeObject(_currentPlayerData);
            SaveLocalCache();

            StartSyncRetry();
        }

        private void StartSyncRetry()
        {
            if (_syncRetryCoroutine != null)
                StopCoroutine(_syncRetryCoroutine);

            _syncRetryCoroutine = StartCoroutine(RetrySyncRoutine());
        }

        private IEnumerator RetrySyncRoutine()
        {
            yield return new WaitForSeconds(syncRetryDelay);

            if (_localCache.hasPendingSync && _syncAttempts < maxSyncAttempts)
            {
                _syncAttempts++;
                Debug.Log($"[CloudSave] Retrying sync (attempt {_syncAttempts}/{maxSyncAttempts})");

                if (!string.IsNullOrEmpty(_localCache.pendingSyncData))
                {
                    try
                    {
                        _currentPlayerData = JsonConvert.DeserializeObject<PlayerData>(_localCache.pendingSyncData);
                        SaveGameData();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[CloudSave] Failed to parse pending data: {e.Message}");
                    }
                }
            }
        }

        private void UpdateLocalCache()
        {
            _currentPlayerData.lastSaveTime = DateTime.Now;
            SaveLocalCache();
        }

        public void UpdatePlayerProgress(int newScore, int newLevel, int newCoins)
        {
            _currentPlayerData.score = newScore;
            _currentPlayerData.level = newLevel;
            _currentPlayerData.coins = newCoins;
            _currentPlayerData.lastSaveTime = DateTime.Now;

            SaveGameData();
        }

        private void OnApplicationQuit()
        {
            if (_currentPlayerData != null)
            {
                UpdateLocalCache();
                if (_localCache.hasPendingSync)
                {
                    SaveGameData();
                }
            }
        }
    }
}