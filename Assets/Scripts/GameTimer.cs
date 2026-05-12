using Colyseus;
using Colyseus.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Callbacks")]
    [SerializeField] private float warningThresholdSeconds = 60f;

    private float timeRemaining = -1f;
    private bool isGameActive = false;
    private bool hasWarned = false;
    private Room<MyRoomState> room;
    private bool initialized = false;

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("Timer Text (TextMeshProUGUI) not assigned!");
            return;
        }

        // Try to initialize immediately, but retry in Update if it fails
        InitializeRoom();
    }

    void Update()
    {
        // Retry initialization if it failed before
        if (!initialized)
        {
            InitializeRoom();
            return;
        }

        if (room == null || !isGameActive || timeRemaining < 0) return;

        // Warning at threshold
        if (timeRemaining <= warningThresholdSeconds && !hasWarned)
        {
            hasWarned = true;
            OnWarningThreshold();
        }

        // Game over
        if (timeRemaining <= 0)
        {
            EndGame();
        }
    }

    private void InitializeRoom()
    {
        if (ColyseusManager.Instance != null)
        {
            room = ColyseusManager.Instance.GetRoom();
            if (room != null)
            {
                initialized = true;

                // Subscribe to timeRemaining changes from server
                var cb = Callbacks.Get(room);
                cb.OnChange(room.State, () =>
                {
                    timeRemaining = room.State.timeRemaining;
                    isGameActive = room.State.isGameActive;

                    // Update display immediately when timer changes
                    UpdateTimerDisplay();
                });
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            //Debug.LogError("timerText is null!");
            return;
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"{minutes:D2}:{seconds:D2}";

        if (timeRemaining <= 30f)
        {
            timerText.color = Color.red;
        }
        else if (timeRemaining <= warningThresholdSeconds)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    private void OnWarningThreshold()
    {
        Debug.Log($"WARNING: {warningThresholdSeconds} seconds remaining!");
    }

    private void EndGame()
    {
        isGameActive = false;
        Debug.Log("GAME TIME OVER! Returning all players to lobby...");

        if (ColyseusManager.Instance != null)
        {
            ColyseusManager.Instance.OnLocalPlayerDied();
        }
    }
}