using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutPanel : MonoBehaviour
{
    [SerializeField] private Transform weaponListContainer;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Button deployButton;
    [SerializeField] private Button changeLoadoutButton;
    [SerializeField] private Button closeButton;

    private List<WeaponButton> weaponButtons = new List<WeaponButton>();

    void Start()
    {
        LoadoutData.InitializeWeapons();
        PopulateWeaponList();
        SetupDeployButton();
        SetupLoadoutPanelToggle();

        // Hide panel on start
        gameObject.SetActive(false);
    }

    private void SetupLoadoutPanelToggle()
    {
        if (changeLoadoutButton != null)
        {
            changeLoadoutButton.onClick.AddListener(OpenLoadoutPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLoadoutPanel);
        }
    }

    private void OpenLoadoutPanel()
    {
        // Refresh button colors to match current selection state
        RefreshButtonColors();

        gameObject.SetActive(true);
        Debug.Log("Loadout panel opened");
    }

    private void CloseLoadoutPanel()
    {
        gameObject.SetActive(false);
        Debug.Log("Loadout panel closed");
    }

    private void RefreshButtonColors()
    {
        foreach (WeaponButton btn in weaponButtons)
        {
            btn.RefreshSelection();
        }
    }

    private void PopulateWeaponList()
    {
        // Clear existing buttons
        foreach (Transform child in weaponListContainer)
        {
            Destroy(child.gameObject);
        }
        weaponButtons.Clear();

        // Create buttons for each weapon
        List<WeaponInfo> weapons = LoadoutData.GetAvailableWeapons();
        foreach (WeaponInfo weapon in weapons)
        {
            GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponListContainer);
            WeaponButton weaponBtn = buttonObj.GetComponent<WeaponButton>();

            if (weaponBtn != null)
            {
                weaponBtn.Initialize(weapon, OnWeaponSelectionChanged);
                weaponButtons.Add(weaponBtn);
            }
        }
    }

    private void OnWeaponSelectionChanged(WeaponInfo weapon, bool isSelected)
    {
        Debug.Log($"{weapon.weaponName} {(isSelected ? "selected" : "deselected")}");
    }

    private void SetupDeployButton()
    {
        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeploy);
        }
    }

    private void OnDeploy()
    {
        // Save selected weapons to LobbyData before closing
        LobbyData.SelectedWeapons = new List<string>(LoadoutData.GetSelectedWeapons());

        Debug.Log($"Deployed with {LobbyData.SelectedWeapons.Count} weapons: {string.Join(", ", LobbyData.SelectedWeapons)}");
        CloseLoadoutPanel();
    }
}