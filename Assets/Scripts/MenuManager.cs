using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button leaveButton;

    private bool isMenuActive = true;
    private ColyseusManager colyseusManager;
    private FPSController fpsController;

    void Start()
    {
        // Validate UI references
        if (menuPanel == null)
        {
            Debug.LogError("MenuPanel is not assigned in MenuManager!");
            return;
        }

        if (leaveButton == null)
        {
            Debug.LogError("LeaveButton is not assigned in MenuManager!");
            return;
        }

        // Get ColyseusManager instance
        colyseusManager = ColyseusManager.Instance;

        // Get FPSController from the local player (stored in ColyseusManager)
        if (colyseusManager != null && colyseusManager.localPlayer != null)
        {
            fpsController = colyseusManager.localPlayer.GetComponent<FPSController>();

            if (fpsController == null)
                Debug.LogError("FPSController not found on local player!");
        }
        else
        {
            Debug.LogError("ColyseusManager or localPlayer not found!");
        }

        // Setup leave button listener
        leaveButton.onClick.AddListener(LeaveGame);

        // Show menu on start
        ShowMenu();
    }

    void Update()
    {
        // ESC key - only open menu if not already open
        if (Input.GetKeyDown(KeyCode.Escape) && !isMenuActive)
        {
            ShowMenu();
        }

        // Left mouse click - only close menu if open (except on Leave button)
        if (Input.GetMouseButtonDown(0) && isMenuActive && !IsPointerOverLeaveButton())
        {
            HideMenu();
        }
    }

    private void ShowMenu()
    {
        menuPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isMenuActive = true;
        Time.timeScale = 1f;

        // Disable player movement
        if (fpsController != null)
            fpsController.enabled = false;
        else
            Debug.LogWarning("FPSController is null, cannot disable movement!");
    }

    private void HideMenu()
    {
        menuPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isMenuActive = false;

        // Enable player movement
        if (fpsController != null)
            fpsController.enabled = true;
        else
            Debug.LogWarning("FPSController is null, cannot enable movement!");
    }

    private async void LeaveGame()
    {
        // Reset time scale before leaving
        Time.timeScale = 1f;

        // Leave the Colyseus room
        if (colyseusManager != null)
        {
            var room = colyseusManager.GetRoom();
            if (room != null && room.State != null)
            {
                await room.Leave();
                Debug.Log("Left the game room");
            }

            // Destroy the old ColyseusManager instance so a new one can be created
            Destroy(colyseusManager.gameObject);
            Debug.Log("Destroyed ColyseusManager for new room connection");
        }

        // Navigate back to lobby
        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// Checks if pointer is over the Leave button specifically
    /// </summary>
    private bool IsPointerOverLeaveButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == leaveButton.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}