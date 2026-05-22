using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform gunContainer;
    [SerializeField] private List<GameObject> allWeaponModels;

    private List<string> selectedWeaponIds = new List<string>();
    private int currentWeaponIndex = -1;
    private Gun currentGunScript;
    private FPSController fpsController;

    // Map weapon IDs to gun pack indices
    private Dictionary<string, int> weaponIndexMap = new Dictionary<string, int>
    {
        { "mp5", 0 },      // Gun 1
        { "shotgun", 1 },  // Gun 2
        { "smg", 2 },      // Gun 3
        { "uzi", 3 },      // Gun 4
        { "m16", 4 },      // Gun 5
        { "sniper", 5 },   // Gun 6
        { "magnum", 6 },   // Gun 7
        { "ak47", 7 },     // Gun 8
        { "lmg", 8 }       // Gun 9 (if you have it)
    };

    private void Start()
    {
        fpsController = GetComponent<FPSController>();

        if (allWeaponModels == null || allWeaponModels.Count == 0)
        {
            Debug.LogError("WeaponManager: No weapon models assigned!");
            return;
        }

        // Deactivate all weapons on start
        foreach (GameObject model in allWeaponModels)
        {
            if (model != null)
                model.SetActive(false);
        }

        InitializeWeapons();

        // Equip first weapon immediately
        if (selectedWeaponIds.Count > 0)
        {
            EquipWeapon(0);
        }
        else
        {
            Debug.LogError("WeaponManager: No weapons selected!");
        }
    }

    private void InitializeWeapons()
    {
        // Get selected weapons from LobbyData
        selectedWeaponIds = new List<string>(LobbyData.SelectedWeapons);

        if (selectedWeaponIds.Count == 0)
        {
            Debug.LogError("No weapons selected! Using default AK-47");
            selectedWeaponIds.Add("ak47");
        }
    }

    // MOVED FROM Update() - now called explicitly from FPSController
    public void EquipWeapon(int index)
    {
        if (selectedWeaponIds.Count == 0)
        {
            Debug.LogError("EquipWeapon: No weapons selected!");
            return;
        }

        if (index >= selectedWeaponIds.Count)
        {
            Debug.LogWarning($"EquipWeapon: Weapon index {index} out of range");
            return;
        }

        if (index == currentWeaponIndex)
            return;

        currentWeaponIndex = index;
        string weaponId = selectedWeaponIds[index];

        // Deactivate all weapon models
        foreach (GameObject model in allWeaponModels)
        {
            if (model != null)
                model.SetActive(false);
        }

        // Activate the selected weapon model
        ActivateWeapon(weaponId);
    }

    private void ActivateWeapon(string weaponId)
    {
        if (!weaponIndexMap.ContainsKey(weaponId))
        {
            Debug.LogError($"Weapon ID '{weaponId}' not found!");
            return;
        }

        int gunIndex = weaponIndexMap[weaponId];

        if (gunIndex >= allWeaponModels.Count)
        {
            Debug.LogError($"Gun index {gunIndex} out of range!");
            return;
        }

        GameObject weaponModel = allWeaponModels[gunIndex];
        if (weaponModel == null)
        {
            Debug.LogError($"Weapon model at index {gunIndex} is null!");
            return;
        }

        // Activate the weapon
        weaponModel.SetActive(true);

        // Get Gun script from the weapon model
        currentGunScript = weaponModel.GetComponent<Gun>();
        if (currentGunScript == null)
        {
            currentGunScript = weaponModel.GetComponentInChildren<Gun>();
        }

        Debug.Log($"Equipped weapon: {weaponId}");
    }

    /// <summary>
    /// Fires the currently equipped gun
    /// </summary>
    public void Shoot()
    {
        if (currentGunScript != null)
        {
            currentGunScript.Shoot();
        }
    }

    /// <summary>
    /// Reloads the currently equipped gun
    /// </summary>
    public void Reload()
    {
        if (currentGunScript != null)
        {
            currentGunScript.Reload();
        }
    }

    public int GetCurrentWeaponIndex() => currentWeaponIndex;
    public Gun GetCurrentGun() => currentGunScript;

    public string GetCurrentWeaponId()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < selectedWeaponIds.Count)
        {
            return selectedWeaponIds[currentWeaponIndex];
        }
        return "ak47"; // Default fallback
    }
}