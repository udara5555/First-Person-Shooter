using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Shooting")]
    public float fireRate = 0.1f;
    public float damage = 10f;
    public float bulletSpeed = 50f;
    public float shootRange = 100f;

    [Header("Effects")]
    public Transform shootPoint; // Where bullets spawn from
    public GameObject muzzleFlash;
    public AudioClip shootSound;
    public GameObject bulletImpactPrefab; // Impact mark prefab
    public float impactMarkScale = 2f;

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public Crosshair crosshair;

    [Header("Camera")]
    public Camera mainCamera;

    private float shootCooldown = 0f;
    private AudioSource audioSource;

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

        // Debug check
        if (bulletImpactPrefab == null)
            Debug.LogWarning("bulletImpactPrefab is not assigned in Gun script!");
    }

    void Update()
    {
        shootCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && shootCooldown <= 0)
        {
            Shoot();
            shootCooldown = fireRate;
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
        // impactMarkScale of 2 = 0.02 world units
        impact.transform.localScale = Vector3.one * impactMarkScale;

        Debug.Log($"Impact created at {impactPosition} on {hit.collider.gameObject.name}");

        // Add BulletImpact component if it doesn't exist
        if (impact.GetComponent<BulletImpact>() == null)
            impact.AddComponent<BulletImpact>();
    }
}