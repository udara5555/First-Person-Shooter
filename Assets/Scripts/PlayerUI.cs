using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Image healthBar;
    [SerializeField] private RawImage skinImage;

    [Header("Skin Textures")]
    [SerializeField] private Texture[] skinTextures;

    private Health playerHealth;

    void Start()
    {
        // Get the Health component from the local player (from ColyseusManager)
        if (ColyseusManager.Instance != null && ColyseusManager.Instance.localPlayer != null)
        {
            playerHealth = ColyseusManager.Instance.localPlayer.GetComponent<Health>();
        }

        // Fallback: try to find it on this GameObject if the above fails
        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("Health component not found on local player!");
            return;
        }

        // Display player name from LobbyData
        if (playerNameText != null)
        {
            playerNameText.text = LobbyData.PlayerName;
        }

        // Display initial health
        UpdateHealthUI();

        // Display player skin
        ApplySkinToUI();
    }

    void Update()
    {
        // Update health UI every frame
        UpdateHealthUI();
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

    private void ApplySkinToUI()
    {
        if (skinImage == null || skinTextures == null || skinTextures.Length == 0)
        {
            Debug.LogWarning("Skin image or textures not assigned!");
            return;
        }

        // Get the selected skin index
        int skinIndex = SkinData.GetSkinIndex(SkinData.SelectedSkin);

        // Apply the corresponding texture
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