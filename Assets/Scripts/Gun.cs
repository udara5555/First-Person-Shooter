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

    [Header("Animation")]
    public Animator animator;

    private float shootCooldown = 0f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();
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
            animator.SetTrigger("Shoot");

        // Play muzzle flash
        if (muzzleFlash != null)
        {
            var flash = Instantiate(muzzleFlash, shootPoint.position, shootPoint.rotation);
            Destroy(flash, 0.1f);
        }

        // Play sound
        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);

        // Raycast for hit
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, shootRange))
        {
            // Deal damage if target has health
            var health = hit.collider.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);

            // Create bullet hole/impact effect
            Debug.Log($"Shot hit: {hit.collider.gameObject.name}");
        }
    }
}