using UnityEngine;
using UnityEngine.UI;

public class SniperADS : MonoBehaviour
{
    [Header("Scope Settings")]
    public float scopeZoom = 20f;
    public float normalZoom = 60f;
    public float zoomSpeed = 15f;

    [Header("Scope Overlay")]
    public RawImage scopeOverlay;

    [Header("Scope Vignette")]
    public Image scopeVignette;
    public Color vignetteColor = new Color(0, 0, 0, 0.7f);

    private Camera mainCamera;
    private float targetFOV;
    private float currentFOV;
    private bool isScoped = false;

    void OnEnable()
    {
        // Hide scope overlay when weapon is equipped
        DisableScopeUI();
    }

    void Start()
    {
        mainCamera = GetComponentInParent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            targetFOV = normalZoom;
            currentFOV = targetFOV;
            mainCamera.fieldOfView = currentFOV;
        }

        // Setup scope UI if references are provided
        if (scopeOverlay != null)
        {
            scopeOverlay.gameObject.SetActive(false);
        }

        if (scopeVignette != null)
        {
            scopeVignette.gameObject.SetActive(false);
            scopeVignette.color = vignetteColor;
        }
    }

    void Update()
    {
        HandleScopeZoom();
    }

    private void HandleScopeZoom()
    {
        isScoped = Input.GetMouseButton(1); // Right mouse button

        if (mainCamera != null)
        {
            targetFOV = isScoped ? scopeZoom : normalZoom;
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, zoomSpeed * Time.deltaTime);
            mainCamera.fieldOfView = currentFOV;
        }

        // Show/hide scope UI elements
        if (scopeOverlay != null)
            scopeOverlay.gameObject.SetActive(isScoped);

        if (scopeVignette != null)
            scopeVignette.gameObject.SetActive(isScoped);
    }

    /// <summary>
    /// Returns true if player is currently scoped in
    /// </summary>
    public bool GetIsScoped()
    {
        return isScoped;
    }

    /// <summary>
    /// Returns the current field of view
    /// </summary>
    public float GetCurrentFOV()
    {
        return currentFOV;
    }

    /// <summary>
    /// Disable the scope UI temporarily
    /// </summary>
    public void DisableScopeUI()
    {
        if (scopeOverlay != null)
            scopeOverlay.gameObject.SetActive(false);

        if (scopeVignette != null)
            scopeVignette.gameObject.SetActive(false);

        isScoped = false;
    }
}