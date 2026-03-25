using UnityEngine;
using GameAnalyticsIntegration;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int scorePerResource = 10;
    [SerializeField] private int startingLives = 3;

    private int _currentScore = 0;
    private int _currentLives;
    private int _currentLevel = 1;
    private string _currentWorld = "world_1";
    private float _levelStartTime;
    private bool _isLevelActive = false;

    public int CurrentScore => _currentScore;
    public int CurrentLives => _currentLives;
    public int CurrentLevel => _currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _currentLives = startingLives;
        StartLevel();
    }

    public void StartLevel()
    {
        _currentScore = 0;
        _levelStartTime = Time.time;
        _isLevelActive = true;

        string levelId = $"{_currentWorld}:level_{_currentLevel}";
        AnalyticsManager.Instance?.SetCurrentLevel(levelId);

        AnalyticsEvents.SendProgressionStart(_currentWorld, $"level_{_currentLevel}");

        Debug.Log($"Level {_currentLevel} started!");
    }

    public void AddScore(int amount)
    {
        if (!_isLevelActive) return;

        _currentScore += amount;

        AnalyticsEvents.SendResourceEarned("coins", amount, "collectable", "coin");

        AnalyticsEvents.SendDesignEvent($"collectable:coin:pickup", _currentScore);

        Debug.Log($"Score: {_currentScore}, +{amount} coins");
    }

    public void CompleteLevel()
    {
        if (!_isLevelActive) return;

        _isLevelActive = false;
        float timeElapsed = Time.time - _levelStartTime;

        AnalyticsEvents.SendProgressionComplete(_currentWorld, $"level_{_currentLevel}", _currentScore, timeElapsed);

        Debug.Log($"Level {_currentLevel} completed! Score: {_currentScore}, Time: {timeElapsed:F2}s");

        _currentLevel++;
        StartLevel();
    }

    public void FailLevel(string reason)
    {
        if (!_isLevelActive) return;

        _isLevelActive = false;
        _currentLives--;

        AnalyticsEvents.SendProgressionFail(_currentWorld, $"level_{_currentLevel}", reason);

        Debug.Log($"Level {_currentLevel} failed! Reason: {reason}. Lives left: {_currentLives}");

        if (_currentLives > 0)
        {
            Invoke(nameof(RestartLevel), 1f);
        }
        else
        {
            GameOver();
        }
    }

    private void RestartLevel()
    {
        StartLevel();
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        AnalyticsEvents.SendErrorEvent("critical", "Game Over - No lives remaining", "GameManager.FailLevel");
    }

    public void PurchaseItem(string itemId, int cost)
    {
        if (_currentScore >= cost)
        {
            _currentScore -= cost;

            AnalyticsEvents.SendResourceSpent("coins", cost, "consumable", itemId);

            AnalyticsEvents.SendBusinessEvent("USD", 99, "consumable", itemId, "in_game_shop");

            AnalyticsEvents.SendDesignEvent($"shop:purchase:{itemId}", cost);

            Debug.Log($"Purchased {itemId} for {cost} coins");
        }
    }
}