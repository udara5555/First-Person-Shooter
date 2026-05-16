using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLoadoutDisplay : MonoBehaviour
{
    [SerializeField] private Image gun1Image;
    [SerializeField] private Image gun2Image;

    private void Start()
    {
        DisplaySelectedWeapons();
    }

    private void DisplaySelectedWeapons()
    {
        // Get selected weapons from LobbyData
        List<string> selectedWeaponIds = LobbyData.SelectedWeapons;
        List<WeaponInfo> allWeapons = LoadoutData.GetAvailableWeapons();

        // Clear both images first
        if (gun1Image != null)
            gun1Image.sprite = null;
        if (gun2Image != null)
            gun2Image.sprite = null;

        // Display selected weapons
        for (int i = 0; i < selectedWeaponIds.Count; i++)
        {
            WeaponInfo weapon = allWeapons.Find(w => w.weaponId == selectedWeaponIds[i]);

            if (weapon != null && weapon.weaponIcon != null)
            {
                if (i == 0 && gun1Image != null)
                {
                    gun1Image.sprite = weapon.weaponIcon;
                    Debug.Log($"Gun1 set to: {weapon.weaponName}");
                }
                else if (i == 1 && gun2Image != null)
                {
                    gun2Image.sprite = weapon.weaponIcon;
                    Debug.Log($"Gun2 set to: {weapon.weaponName}");
                }
            }
        }

        Debug.Log($"Map: Displayed {selectedWeaponIds.Count} selected weapons");
    }
}