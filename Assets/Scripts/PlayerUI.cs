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