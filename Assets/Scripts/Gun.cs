using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Shooting")]
    public float fireRate = 0.1f;
    public float damage = 10f;
    public float bulletSpeed = 50f;
    public float shootRange = 100f;

    [Header("Effects")]
    public Transform shootPoint;
    public GameObject muzzleFlash;
    public AudioClip shootSound;
    public GameObject bulletImpactPrefab;
    public float impactMarkScale = 2f;

    [Header("ADS (Aim Down Sight)")]
    public float adsZoom = 40f; // Zoomed FOV
    public float normalZoom = 60f; // Normal FOV
    public float adsSpeed = 10f; // How fast to zoom
    public Vector3 adsPosition = new Vector3(0.3f, -0.2f, 0.5f); // Gun position when ADS
    public Vector3 normalPosition = Vector3.zero; // Gun position when not ADS

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public Crosshair crosshair;

    [Header("Camera")]
    public Camera mainCamera;

    private float shootCooldown = 0f;
    private AudioSource audioSource;
    private bool isAiming = false;
    private float targetFOV;
    private float currentFOV;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (crosshair == null)
            crosshair = Object.FindAnyObjectByType<Crosshair>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (bulletImpactPrefab == null)
            Debug.LogWarning("bulletImpactPrefab is not assigned in Gun script!");

        // Set initial FOV
        currentFOV = normalZoom;
        targetFOV = normalZoom;
        if (mainCamera != null)
            mainCamera.fieldOfView = currentFOV;
    }

    void Update()
    {
        // Handle ADS
        HandleADS();

        shootCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && shootCooldown <= 0)
        {
            Shoot();
            shootCooldown = fireRate;
        }
    }

    void HandleADS()
    {
        isAiming = Input.GetMouseButton(1); // Right mouse button

        if (isAiming)
        {
            targetFOV = adsZoom;
        }
        else
        {
            targetFOV = normalZoom;
        }

        // Smoothly transition FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * adsSpeed);
        if (mainCamera != null)
            mainCamera.fieldOfView = currentFOV;

        // Smoothly move gun position
        Vector3 targetPosition = isAiming ? adsPosition : normalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * adsSpeed);

        // Update crosshair visibility
        if (crosshair != null)
        {
            // You can make crosshair smaller when aiming
            crosshair.SetAiming(isAiming);
        }
    }

    void Shoot()
    {
        // Play shoot animation
        if (animator != null)
        {
            try
            {
                animator.SetTrigger("Shoot");
            }
            catch (System.Exception)
            {
                // Parameter doesn't exist, ignore
            }
        }

        // Play muzzle flash
        if (muzzleFlash != null)
        {
            var flash = Instantiate(muzzleFlash, shootPoint.position, shootPoint.rotation);
            Destroy(flash, 0.1f);
        }

        // Play sound
        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);

        // Raycast from camera center (crosshair position)
        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 rayDirection = mainCamera.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, shootRange))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name} at {hit.point}");

            // Create bullet impact mark
            CreateImpactMark(hit);

            // Deal damage if target has health
            var health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);

                // Flash crosshair on hit
                if (crosshair != null)
                    crosshair.OnHit();
            }
        }
        else
        {
            Debug.Log("Raycast missed!");
        }
    }

    void CreateImpactMark(RaycastHit hit)
    {
        if (bulletImpactPrefab == null)
        {
            Debug.LogError("bulletImpactPrefab is NULL! Cannot create impact mark.");
            return;
        }

        // Position the impact at the hit point (slightly offset from surface)
        Vector3 impactPosition = hit.point + hit.normal * 0.01f;

        // Rotate to face the surface (plane faces outward)
        Quaternion impactRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        // Create the impact mark in world space
        var impact = Instantiate(bulletImpactPrefab, impactPosition, impactRotation);

        // Set scale - the prefab is 0.01, so we scale from that base
        impact.transform.localScale = Vector3.one * impactMarkScale;

        Debug.Log($"Impact created at {impactPosition} on {hit.collider.gameObject.name}");

        // Add BulletImpact component if it doesn't exist
        if (impact.GetComponent<BulletImpact>() == null)
            impact.AddComponent<BulletImpact>();
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}