using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponButton : MonoBehaviour
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage; // The button's image component for color change

    private WeaponInfo weaponInfo;
    private System.Action<WeaponInfo, bool> onSelectionChanged;
    private bool isSelected = false;
    private ColorBlock originalColors;

    public void Initialize(WeaponInfo weapon, System.Action<WeaponInfo, bool> callback)
    {
        weaponInfo = weapon;
        onSelectionChanged = callback;

        weaponName.text = weapon.weaponName;
        damageText.text = $"Damage: {weapon.damage}";

        if (weaponIcon != null && weapon.weaponIcon != null)
            weaponIcon.sprite = weapon.weaponIcon;

        // Store the original button colors
        originalColors = button.colors;

        // Get the button's image component if not assigned
        if (buttonImage == null)
            buttonImage = button.GetComponent<Image>();

        // Check if this weapon is pre-selected
        isSelected = LoadoutData.IsWeaponSelected(weapon.weaponId);
        UpdateButtonColor();

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // Try to toggle selection
        bool canSelect = LoadoutData.TrySelectWeapon(weaponInfo.weaponId);

        if (isSelected)
        {
            // Was selected, now deselected
            isSelected = false;
        }
        else if (canSelect)
        {
            // Successfully selected
            isSelected = true;
        }
        else
        {
            // Can't select more weapons
            Debug.LogWarning($"Maximum {LoadoutData.GetMaxWeapons()} weapons allowed!");
            return;
        }

        UpdateButtonColor();
        onSelectionChanged?.Invoke(weaponInfo, isSelected);
    }

    private void UpdateButtonColor()
    {
        if (buttonImage == null)
            return;

        // Get the button's color block to access the selected color
        ColorBlock colors = button.colors;

        if (isSelected)
        {
            // Set to selected color (green)
            buttonImage.color = colors.selectedColor;
        }
        else
        {
            // Set to normal color (white)
            buttonImage.color = colors.normalColor;
        }
    }

    public void RefreshSelection()
    {
        // Called when panel reopens to sync with LoadoutData
        isSelected = LoadoutData.IsWeaponSelected(weaponInfo.weaponId);
        UpdateButtonColor();
    }

    public bool IsSelected() => isSelected;
    public string GetWeaponId() => weaponInfo.weaponId;
}