using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform gunContainer; // The Gun container with all weapons
    [SerializeField] private List<GameObject> allWeaponModels; // Drag all gun models here

    private List<string> selectedWeaponIds = new List<string>();
    private int currentWeaponIndex = 0;
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

        // Deactivate all weapons on start
        foreach (GameObject model in allWeaponModels)
        {
            if (model != null)
                model.SetActive(false);
        }

        InitializeWeapons();
        EquipWeapon(0);
    }

    private void Update()
    {
        HandleWeaponSwitching();
    }

    private void InitializeWeapons()
    {
        // Get selected weapons from LobbyData
        selectedWeaponIds = LobbyData.SelectedWeapons;

        if (selectedWeaponIds.Count == 0)
        {
            Debug.LogError("No weapons selected! Using default AK-47");
            selectedWeaponIds.Add("ak47");
        }

        Debug.Log($"Initialized with {selectedWeaponIds.Count} weapons: {string.Join(", ", selectedWeaponIds)}");
    }

    private void HandleWeaponSwitching()
    {
        // Press 1 to switch to first weapon
        if (Input.GetKeyDown(KeyCode.Alpha1) && selectedWeaponIds.Count >= 1)
        {
            EquipWeapon(0);
        }

        // Press 2 to switch to second weapon
        if (Input.GetKeyDown(KeyCode.Alpha2) && selectedWeaponIds.Count >= 2)
        {
            EquipWeapon(1);
        }
    }

    private void EquipWeapon(int index)
    {
        if (index >= selectedWeaponIds.Count)
        {
            Debug.LogWarning($"Weapon index {index} out of range");
            return;
        }

        if (index == currentWeaponIndex)
            return; // Already equipped

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
            Debug.LogError($"Weapon ID {weaponId} not found in map!");
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

        weaponModel.SetActive(true);
        Debug.Log($"Activated weapon: {weaponId} at index {gunIndex}");

        // Get Gun script from the weapon model
        currentGunScript = weaponModel.GetComponent<Gun>();
        if (currentGunScript == null)
        {
            currentGunScript = weaponModel.GetComponentInChildren<Gun>();
        }

        // Update FPS controller with new gun
        if (fpsController != null && currentGunScript != null)
        {
            fpsController.gun = currentGunScript;
            Debug.Log($"Equipped weapon: {weaponId}");
        }
        else
        {
            Debug.LogWarning($"Could not find Gun script on weapon model!");
        }
    }

    public int GetCurrentWeaponIndex() => currentWeaponIndex;
    public string GetCurrentWeaponId() => selectedWeaponIds[currentWeaponIndex];
    public Gun GetCurrentGun() => currentGunScript;
}