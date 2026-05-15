using System.Collections.Generic;
using UnityEngine;

public class WeaponInfo
{
    public string weaponName;
    public string weaponId;
    public float damage;
    public Sprite weaponIcon;
}

public static class LoadoutData
{
    private static List<WeaponInfo> availableWeapons = new List<WeaponInfo>();
    private static List<string> selectedWeapons = new List<string>();
    private const int MAX_WEAPONS = 2;

    public static void InitializeWeapons()
    {
        availableWeapons = new List<WeaponInfo>
        {
            new WeaponInfo { weaponName = "MP5", weaponId = "mp5", damage = 6f, weaponIcon = WeaponIconManager.Instance.mp5Icon },
            new WeaponInfo { weaponName = "Shotgun", weaponId = "shotgun", damage = 15f, weaponIcon = WeaponIconManager.Instance.shotgunIcon },
            new WeaponInfo { weaponName = "SMG", weaponId = "smg", damage = 5f, weaponIcon = WeaponIconManager.Instance.smgIcon },
            new WeaponInfo { weaponName = "Uzi", weaponId = "uzi", damage = 6f, weaponIcon = WeaponIconManager.Instance.uziIcon },
            new WeaponInfo { weaponName = "M16", weaponId = "m16", damage = 10f, weaponIcon = WeaponIconManager.Instance.m16Icon },
            new WeaponInfo { weaponName = "Sniper", weaponId = "sniper", damage = 13f, weaponIcon = WeaponIconManager.Instance.sniperIcon },
            new WeaponInfo { weaponName = ".44", weaponId = "magnum", damage = 10f, weaponIcon = WeaponIconManager.Instance.magnumIcon },
            new WeaponInfo { weaponName = "AK-47", weaponId = "ak47", damage = 10f, weaponIcon = WeaponIconManager.Instance.ak47Icon },
            new WeaponInfo { weaponName = "LMG", weaponId = "lmg", damage = 13f, weaponIcon = WeaponIconManager.Instance.lmgIcon }
        };

        // Initialize with AK-47 and Magnum selected
        selectedWeapons = new List<string> { "ak47", "magnum" };
    }

    public static List<WeaponInfo> GetAvailableWeapons() => availableWeapons;
    public static List<string> GetSelectedWeapons() => selectedWeapons;
    public static bool IsWeaponSelected(string weaponId) => selectedWeapons.Contains(weaponId);

    public static bool TrySelectWeapon(string weaponId)
    {
        if (selectedWeapons.Contains(weaponId))
        {
            // Deselect if already selected
            selectedWeapons.Remove(weaponId);
            return false;
        }

        if (selectedWeapons.Count < MAX_WEAPONS)
        {
            selectedWeapons.Add(weaponId);
            return true;
        }

        return false; // Can't add more weapons
    }

    public static void DeselectWeapon(string weaponId)
    {
        selectedWeapons.Remove(weaponId);
    }

    public static int GetMaxWeapons() => MAX_WEAPONS;
    public static int GetSelectedWeaponCount() => selectedWeapons.Count;
}