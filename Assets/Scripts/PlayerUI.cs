using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Image healthBar;
    [SerializeField] private RawImage skinImage;
    [SerializeField] private TextMeshProUGUI gunNameText;
    [SerializeField] private TextMeshProUGUI ammoCountText;
    [SerializeField] private GameObject damagePanel;

    [Header("Damage Panel Settings")]
    [SerializeField] private float damagePanelDisplayDuration = 0.5f;
    [SerializeField] private float damagePanelFadeDuration = 0.3f;

    [Header("Skin Textures")]
    [SerializeField] private Texture[] skinTextures;

    [Header("Gun Names")]
    [SerializeField]
    private List<string> gunNames = new List<string>
    {
        "MP5", "Shotgun", "SMG", "UZI", "M16", "Sniper", "Magnum", "AK-47", "LMG"
    };

    private Health playerHealth;
    private WeaponManager weaponManager;
    private Gun currentGun;
    private float damagePanelTimer = 0f;
    private CanvasGroup damagePanelCanvasGroup;

    void Start()
    {
        // Get the Health component from the local player
        if (ColyseusManager.Instance != null && ColyseusManager.Instance.localPlayer != null)
        {
            playerHealth = ColyseusManager.Instance.localPlayer.GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("Health component not found on local player!");
            return;
        }

        // Get WeaponManager
        weaponManager = GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager not found!");
        }

        // Setup damage panel
        if (damagePanel != null)
        {
            damagePanelCanvasGroup = damagePanel.GetComponent<CanvasGroup>();
            if (damagePanelCanvasGroup == null)
            {
                damagePanelCanvasGroup = damagePanel.AddComponent<CanvasGroup>();
            }

            // Ensure panel is initially disabled
            damagePanel.SetActive(false);
            damagePanelCanvasGroup.alpha = 0f;
            Debug.Log("DamagePanel initialized!");
        }
        else
        {
            Debug.LogWarning("DamagePanel not assigned in PlayerUI!");
        }

        // Display player name
        if (playerNameText != null)
        {
            playerNameText.text = LobbyData.PlayerName;
        }

        // Display initial health
        UpdateHealthUI();

        // Display player skin
        ApplySkinToUI();

        // Initialize gun UI
        UpdateGunUI();
    }

    void Update()
    {
        // Update health UI every frame
        UpdateHealthUI();

        // Update gun UI every frame
        UpdateGunUI();

        // Update damage panel fade
        UpdateDamagePanel();
    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        float currentHealth = playerHealth.GetHealth();
        float maxHealth = playerHealth.GetMaxHealth();

        // Update health text
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{currentHealth}/{maxHealth} HP";
        }

        // Update health bar fill
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    private void UpdateGunUI()
    {
        // Get the current gun from WeaponManager
        if (weaponManager == null) return;

        // Get current gun script
        Gun gun = weaponManager.GetCurrentGun();

        if (gun == null) return;

        // Update gun name based on the actual gun model index, not the selected weapons index
        if (gunNameText != null)
        {
            string currentWeaponId = weaponManager.GetCurrentWeaponId();
            int gunModelIndex = weaponManager.GetGunModelIndexFromId(currentWeaponId);

            if (gunModelIndex >= 0 && gunModelIndex < gunNames.Count)
            {
                gunNameText.text = gunNames[gunModelIndex];
            }
        }

        // Update ammo count
        if (ammoCountText != null)
        {
            int currentAmmo = gun.GetCurrentAmmo();
            int maxAmmo = gun.GetMagazineSize();
            ammoCountText.text = $"{currentAmmo}/{maxAmmo}";
        }
    }

    private void UpdateDamagePanel()
    {
        if (damagePanelCanvasGroup == null || damagePanel == null) return;

        if (damagePanelTimer > 0)
        {
            damagePanelTimer -= Time.deltaTime;

            // Fade out after display duration
            if (damagePanelTimer < damagePanelFadeDuration)
            {
                damagePanelCanvasGroup.alpha = damagePanelTimer / damagePanelFadeDuration;
            }
        }
        else
        {
            damagePanelCanvasGroup.alpha = 0f;
            damagePanel.SetActive(false);
        }
    }

    public void ShowDamagePanel()
    {
        if (damagePanel == null || damagePanelCanvasGroup == null)
        {
            Debug.LogWarning("Damage panel is not assigned!");
            return;
        }

        // Enable the panel
        damagePanel.SetActive(true);
        damagePanelCanvasGroup.alpha = 1f;
        damagePanelTimer = damagePanelDisplayDuration + damagePanelFadeDuration;
    }

    private void ApplySkinToUI()
    {
        if (skinImage == null || skinTextures == null || skinTextures.Length == 0)
        {
            Debug.LogWarning("Skin image or textures not assigned!");
            return;
        }

        int skinIndex = SkinData.GetSkinIndex(SkinData.SelectedSkin);

        if (skinIndex >= 0 && skinIndex < skinTextures.Length)
        {
            skinImage.texture = skinTextures[skinIndex];
        }
        else
        {
            Debug.LogWarning($"Skin index {skinIndex} is out of range!");
        }
    }
}