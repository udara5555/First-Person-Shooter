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

        // Setup leave button listener
        leaveButton.onClick.AddListener(LeaveGame);

        // Show menu on start
        ShowMenu();
    }

    void Update()
    {
        // Toggle menu with Esc key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMenuActive)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }

        // Hide menu on any mouse click (except on UI elements like Leave button)
        if (Input.GetMouseButtonDown(0) && isMenuActive && !IsPointerOverUIElement())
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
        Time.timeScale = 1f; // Ensure game continues (not paused)
    }

    private void HideMenu()
    {
        menuPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isMenuActive = false;
    }

    private void LeaveGame()
    {
        // Reset time scale before leaving
        Time.timeScale = 1f;

        // Navigate back to lobby
        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// Checks if pointer is over a UI element
    /// </summary>
    private bool IsPointerOverUIElement()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}