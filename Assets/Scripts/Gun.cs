using System.Collections.Generic;
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

            // Check if target has Health component (local player)
            var health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Local player hit for {damage} damage");
            }
            else
            {
                // Check if it's a remote player
                var remoteTag = hit.collider.tag;
                if (remoteTag == "RemotePlayer" || hit.collider.name.Contains("Remote"))
                {
                    SendDamageToRemotePlayer(hit.collider.gameObject);
                    Debug.Log($"Remote player hit, sending damage event");
                }
            }

            // Flash crosshair on hit
            if (crosshair != null)
                crosshair.OnHit();
        }
        else
        {
            Debug.Log("Raycast missed!");
        }
    }

    void SendDamageToRemotePlayer(GameObject targetPlayer)
    {
        if (ColyseusManager.Instance == null || ColyseusManager.Instance.GetRoom() == null)
        {
            Debug.LogError("ColyseusManager not available!");
            return;
        }

        // Send damage message to server
        ColyseusManager.Instance.GetRoom().Send("damage", new Dictionary<string, object>
        {
            { "targetPlayerId", GetRemotePlayerSessionId(targetPlayer) },
            { "damage", damage }
        });
    }

    string GetRemotePlayerSessionId(GameObject targetPlayer)
    {
        // This assumes you store sessionId on the remote player GameObject
        var remoteComponent = targetPlayer.GetComponent<RemotePlayerComponent>();
        if (remoteComponent != null)
            return remoteComponent.SessionId;

        // Fallback: Search in remotes dictionary (you'll need to expose this)
        // Or store sessionId as a tag/name
        return "unknown";
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
}