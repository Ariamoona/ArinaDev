using UnityEngine;
using System;

namespace GamePushIntegration
{
    public class LeaderboardManager : MonoBehaviour
    {
        private static LeaderboardManager _instance;
        public static LeaderboardManager Instance => _instance;

        public object GamePush { get; private set; }

        [Header("Settings")]
        [SerializeField] private string leaderboardId = "score_leaderboard";

        public event Action<int> OnScoreSubmitted;
        public event Action<string> OnLeaderboardError;

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

        public void SubmitScore(int score)
        {
            if (!GamePushManager.Instance.IsPlayerAuthorized)
            {
                Debug.LogWarning("[Leaderboard] Player not authorized, cannot submit score");
                OnLeaderboardError?.Invoke("Player not authorized");
                return;
            }

            Debug.Log($"[Leaderboard] Submitting score: {score}");

            GamePush.GP_Leaderboard.OnLeaderboardSetScore += OnScoreSubmittedSuccess;
            GamePush.GP_Leaderboard.OnLeaderboardSetScoreError += OnScoreSubmittedError;

            GamePush.GP_Leaderboard.SetScore(leaderboardId, score);
        }

        private void OnScoreSubmittedSuccess()
        {
            GamePush.GP_Leaderboard.OnLeaderboardSetScore -= OnScoreSubmittedSuccess;
            GamePush.GP_Leaderboard.OnLeaderboardSetScoreError -= OnScoreSubmittedError;

            Debug.Log("[Leaderboard] Score submitted successfully!");
            OnScoreSubmitted?.Invoke(GamePush.GP_Player.GetScore(leaderboardId));
        }

        private void OnScoreSubmittedError(string error)
        {
            GamePush.GP_Leaderboard.OnLeaderboardSetScore -= OnScoreSubmittedSuccess;
            GamePush.GP_Leaderboard.OnLeaderboardSetScoreError -= OnScoreSubmittedError;

            Debug.LogError($"[Leaderboard] Failed to submit score: {error}");
            OnLeaderboardError?.Invoke(error);
        }

        public void ShowLeaderboard()
        {
            if (!GamePushManager.Instance.IsPlayerAuthorized)
            {
                Debug.LogWarning("[Leaderboard] Player not authorized");
                return;
            }

            GamePush.GP_Leaderboard.Open(leaderboardId);
        }

        public void FetchTopScores(int limit = 10, Action<string[]> callback = null)
        {
            GamePush.GP_Leaderboard.OnLeaderboardFetch += (data) => {
                callback?.Invoke(new string[] { data });
            };
            GamePush.GP_Leaderboard.Fetch(leaderboardId, limit);
        }
    }
}