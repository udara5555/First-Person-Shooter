using UnityEngine;
using UnityEngine.UI;

public class RemotePlayerComponent : MonoBehaviour
{
    public string SessionId { get; set; }
    
    private Canvas healthBarCanvas;
    private Image healthBarFill;
    private float maxHealth = 100f;
    private AudioSource audioSource;

    void Start()
    {
        CreateHealthBar();

        // Add AudioSource for remote shoot sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // Full 3D sound
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 40f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    /// <summary>
    /// Plays muzzle flash and shoot sound on this remote player's currently active weapon.
    /// Called by ColyseusManager when a "playerShoot" message arrives.
    /// </summary>
    public void PlayShootEffects()
    {
        // Find the active Gun script on the remote player's weapon
        Gun activeGun = GetActiveGun();
        if (activeGun == null)
        {
            Debug.LogWarning($"[RemoteShoot] No active Gun found on remote player {SessionId}");
            return;
        }

        // Play muzzle flash
        if (activeGun.muzzleFlash != null && activeGun.shootPoint != null)
        {
            var flash = Instantiate(activeGun.muzzleFlash, activeGun.shootPoint.position, activeGun.shootPoint.rotation);
            Destroy(flash, 0.1f);
        }

        // Play shoot sound
        if (activeGun.shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activeGun.shootSound);
        }

        // Trigger shoot animation
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            try { animator.SetTrigger("Shoot"); }
            catch (System.Exception) { /* Parameter doesn't exist, ignore */ }
        }
    }

    /// <summary>
    /// Finds the currently active Gun script on this remote player.
    /// Searches through the Gun container for an active weapon with a Gun component.
    /// </summary>
    private Gun GetActiveGun()
    {
        // Find Gun container (search hierarchy)
        Transform gunContainer = FindGunContainer();
        if (gunContainer == null) return null;

        // Find the active child with a Gun component
        for (int i = 0; i < gunContainer.childCount; i++)
        {
            var child = gunContainer.GetChild(i);
            if (child.gameObject.activeInHierarchy)
            {
                var gun = child.GetComponent<Gun>();
                if (gun == null)
                    gun = child.GetComponentInChildren<Gun>();
                if (gun != null)
                    return gun;
            }
        }

        return null;
    }

    private Transform FindGunContainer()
    {
        // Try direct child
        Transform gunContainer = transform.Find("Gun");
        if (gunContainer != null) return gunContainer;

        // Try under Spine1
        Transform spine1 = transform.Find("Spine1");
        if (spine1 != null)
        {
            gunContainer = spine1.Find("Gun");
            if (gunContainer != null) return gunContainer;
        }

        // Recursive search as last resort
        Transform[] allTransforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.name == "Gun")
                return t;
        }

        return null;
    }

    void CreateHealthBar()
    {
        // Create canvas for health bar
        var canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = new Vector3(0, 2.5f, 0);

        healthBarCanvas = canvasGO.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;

        var canvasRectTransform = canvasGO.GetComponent<RectTransform>();
        canvasRectTransform.sizeDelta = new Vector2(1, 0.2f);

        // Create background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform);
        bgGO.transform.localPosition = Vector3.zero;
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = Color.black;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(1, 0.2f);

        // Create health fill
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform);
        fillGO.transform.localPosition = Vector3.zero;
        healthBarFill = fillGO.AddComponent<Image>();
        healthBarFill.color = Color.green;
        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(1, 0.2f);
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        this.maxHealth = maxHealth;
        if (healthBarFill != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercent;
            
            // Change color based on health
            if (healthPercent > 0.5f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.25f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }
}