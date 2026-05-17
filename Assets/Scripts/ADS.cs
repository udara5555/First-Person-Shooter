using UnityEngine;

public class ADS : MonoBehaviour
{
    [Header("ADS Settings")]
    public float adsZoom = 40f;
    public float normalZoom = 60f;
    public float adsSpeed = 10f;
    public Vector3 adsPosition = new Vector3(0.3f, -0.2f, 0.5f);
    public Vector3 normalPosition = Vector3.zero;

    private Camera mainCamera;
    private float targetFOV;
    private float currentFOV;
    private bool isAiming = false;

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
    }

    void Update()
    {
        HandleADS();
    }

    private void HandleADS()
    {
        isAiming = Input.GetMouseButton(1); // Right mouse button

        if (mainCamera != null)
        {
            targetFOV = isAiming ? adsZoom : normalZoom;
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, adsSpeed * Time.deltaTime);
            mainCamera.fieldOfView = currentFOV;
        }

        // Update gun position for ADS
        if (transform.parent != null)
        {
            Vector3 targetPosition = isAiming ? adsPosition : normalPosition;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, adsSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Returns true if the player is currently aiming down sights
    /// </summary>
    public bool GetIsAiming()
    {
        return isAiming;
    }

    /// <summary>
    /// Returns the current field of view
    /// </summary>
    public float GetCurrentFOV()
    {
        return currentFOV;
    }
}