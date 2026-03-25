using System;
using UnityEngine;

namespace GamePushIntegration
{
    [Serializable]
    public class PlayerData
    {
        public int score;
        public int level;
        public int coins;
        public DateTime lastSaveTime;
        public string playerId;
        public string playerName;

        public PlayerData()
        {
            score = 0;
            level = 1;
            coins = 0;
            lastSaveTime = DateTime.Now;
            playerId = "";
            playerName = "Player";
        }
    }

    [Serializable]
    public class LocalCacheData
    {
        public PlayerData playerData;
        public bool hasPendingSync;
        public string pendingSyncData;
        public DateTime lastSyncAttempt;

        public LocalCacheData()
        {
            playerData = new PlayerData();
            hasPendingSync = false;
            pendingSyncData = "";
            lastSyncAttempt = DateTime.Now;
        }
    }
}