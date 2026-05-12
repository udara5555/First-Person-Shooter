using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float gameDurationSeconds = 300f; // 5 minutes default
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Callbacks")]
    [SerializeField] private float warningThresholdSeconds = 60f; // Show warning at 1 minute

    private float timeRemaining;
    private bool isGameActive = true;
    private bool hasWarned = false;

    void Start()
    {
        timeRemaining = gameDurationSeconds;

        if (timerText == null)
        {
            Debug.LogError("Timer Text (TextMeshProUGUI) not assigned!");
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;

        // Update UI
        UpdateTimerDisplay();

        // Warning at threshold
        if (timeRemaining <= warningThresholdSeconds && !hasWarned)
        {
            hasWarned = true;
            OnWarningThreshold();
        }

        // Game over
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"{minutes:D2}:{seconds:D2}";

        // Optional: Change color based on time remaining
        if (timeRemaining <= 30f)
        {
            timerText.color = Color.red; // Red for critical time
        }
        else if (timeRemaining <= warningThresholdSeconds)
        {
            timerText.color = Color.yellow; // Yellow for warning
        }
        else
        {
            timerText.color = Color.white; // White for normal
        }
    }

    private void OnWarningThreshold()
    {
        Debug.Log($" WARNING: {warningThresholdSeconds} seconds remaining!");
        // You can add sound effects or visual effects here
    }

    private void EndGame()
    {
        isGameActive = false;
        Debug.Log("GAME TIME OVER! Returning all players to lobby...");

        // Notify all players through ColyseusManager
        if (ColyseusManager.Instance != null)
        {
            var room = ColyseusManager.Instance.GetRoom();
            if (room != null)
            {
                // Send a message to the server that time is up
                room.Send("gameEnded", new { reason = "timeUp" });
            }

            // Return local player to lobby
            ColyseusManager.Instance.OnLocalPlayerDied();
        }
    }

    // Public method to pause/resume timer
    public void PauseTimer()
    {
        isGameActive = false;
    }

    public void ResumeTimer()
    {
        isGameActive = true;
    }

    // Public method to get time remaining
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    // Public method to set custom game duration
    public void SetGameDuration(float seconds)
    {
        gameDurationSeconds = seconds;
        timeRemaining = seconds;
    }
}