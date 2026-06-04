using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Shooting")]
    public float fireRate = 0.1f;
    public float damage = 10f;
    public float bulletSpeed = 50f;
    public float shootRange = 100f;

    [Header("Magazine")]
    public int magazineSize = 30;
    public float reloadTime = 2f;
    public bool autoReload = true;

    [Header("Effects")]
    public Transform shootPoint;
    public GameObject muzzleFlash;
    public AudioClip shootSound;
    public GameObject bulletImpactPrefab;
    public float impactMarkScale = 2f;

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public Crosshair crosshair;

    [Header("Camera")]
    public Camera mainCamera;

    private float shootCooldown = 0f;
    private float reloadCooldown = 0f;
    private int currentAmmo;
    private AudioSource audioSource;
    private bool isReloading = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (crosshair == null)
            crosshair = Object.FindAnyObjectByType<Crosshair>();

        // Get camera - try multiple methods
        if (mainCamera == null)
        {
            // First try: Find parent camera (since gun is child of camera)
            mainCamera = GetComponentInParent<Camera>();
        }

        if (mainCamera == null)
        {
            // Second try: Find camera by tag
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            // Third try: Search in scene
            //mainCamera = FindObjectOfType<Camera>();
        }

        if (mainCamera == null)
            Debug.LogError("Could not find main camera! Gun will not shoot properly.");

        if (bulletImpactPrefab == null)
            Debug.LogWarning("bulletImpactPrefab is not assigned in Gun script!");

        // Initialize ammo
        currentAmmo = magazineSize;
    }

    void Update()
    {
        // Decrement cooldown (important for fire rate)
        shootCooldown -= Time.deltaTime;

        // Update reload cooldown
        if (isReloading)
        {
            reloadCooldown -= Time.deltaTime;
            if (reloadCooldown <= 0)
            {
                CompleteReload();
            }
        }

        // Auto-reload if magazine is empty and not already reloading
        if (autoReload && currentAmmo == 0 && !isReloading)
        {
            Reload();
        }
    }

    public void Shoot()
    {
        // Check if reloading
        if (isReloading)
            return;

        // Check if out of ammo
        if (currentAmmo <= 0)
            return;

        // Check cooldown
        if (shootCooldown > 0)
            return;

        shootCooldown = fireRate;
        currentAmmo--;

        // Verify camera is assigned
        if (mainCamera == null)
        {
            mainCamera = GetComponentInParent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (mainCamera == null)
            {
                Debug.LogError("Cannot shoot - main camera not found!");
                return;
            }
        }

        // Verify shoot point exists
        if (shootPoint == null)
        {
            Debug.LogError("Shoot point not assigned on " + gameObject.name);
            return;
        }

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

            // Check if we hit a remote player
            var remotePlayerComponent = hit.collider.GetComponent<RemotePlayerComponent>();
            if (remotePlayerComponent != null)
            {
                // Send damage to server for remote player
                var room = ColyseusManager.Instance?.GetRoom();
                if (room != null)
                {
                    room.Send("damage", new { targetPlayerId = remotePlayerComponent.SessionId, damage = damage });
                    Debug.Log($"Sent damage message to server: target={remotePlayerComponent.SessionId}, damage={damage}");
                }

                // Also apply damage locally for immediate feedback
                var remoteHealth = hit.collider.GetComponent<RemotePlayerHealth>();
                if (remoteHealth != null)
                {
                    remoteHealth.TakeDamage(damage);
                }
            }
            else
            {
                // Hit local player (self-damage)
                var health = hit.collider.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    // Send to server so other players see you taking damage
                    var room = ColyseusManager.Instance?.GetRoom();
                    if (room != null)
                    {
                        room.Send("damage", new { targetPlayerId = room.SessionId, damage = damage });
                        Debug.Log($"Sent damage message to server for self-damage: damage={damage}");
                    }
                }
            }
        }
    }

    public void Reload()
    {
        // Cannot reload if already reloading
        if (isReloading)
            return;

        // Cannot reload if magazine is full
        if (currentAmmo == magazineSize)
            return;

        isReloading = true;
        reloadCooldown = reloadTime;

        // Play reload animation
        if (animator != null)
        {
            try
            {
                animator.SetBool("isReloading", true);
            }
            catch (System.Exception)
            {
                // Parameter doesn't exist, ignore
            }
        }

        Debug.Log($"Reloading {gameObject.name}... ({reloadTime}s)");
    }

    private void CompleteReload()
    {
        isReloading = false;
        currentAmmo = magazineSize;

        // Stop reload animation
        if (animator != null)
        {
            try
            {
                animator.SetBool("isReloading", false);
            }
            catch (System.Exception)
            {
                // Parameter doesn't exist, ignore
            }
        }

        Debug.Log($"Reload complete! Ammo: {currentAmmo}/{magazineSize}");
    }

    void CreateImpactMark(RaycastHit hit)
    {
        if (bulletImpactPrefab == null)
        {
            Debug.LogError("bulletImpactPrefab is NULL! Cannot create impact mark.");
            return;
        }

        Vector3 impactPosition = hit.point + hit.normal * 0.01f;
        Quaternion impactRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        var impact = Instantiate(bulletImpactPrefab, impactPosition, impactRotation);
        impact.transform.localScale = Vector3.one * impactMarkScale;

        Debug.Log($"Impact created at {impactPosition} on {hit.collider.gameObject.name}");

        if (impact.GetComponent<BulletImpact>() == null)
            impact.AddComponent<BulletImpact>();
    }

    /// <summary>
    /// Returns current ammo count
    /// </summary>
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    /// <summary>
    /// Returns magazine size
    /// </summary>
    public int GetMagazineSize()
    {
        return magazineSize;
    }

    /// <summary>
    /// Returns whether gun is currently reloading
    /// </summary>
    public bool IsReloading()
    {
        return isReloading;
    }
}