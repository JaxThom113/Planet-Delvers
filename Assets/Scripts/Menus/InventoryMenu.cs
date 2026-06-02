using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class InventoryMenu : MonoBehaviour
{
    [Header("Menu Tabs")]
    [SerializeField] private Toggle inventoryToggle;
    [SerializeField] private Toggle mapToggle;
    [SerializeField] private Toggle statsToggle;
    [SerializeField] private GameObject inventoryTab;
    [SerializeField] private GameObject mapTab;
    [SerializeField] private GameObject statsTab;

    void OnEnable()
    {
        HandleColors();
    }

    /*
        Pause menu input actions
    */
    public void OnInventory(InputAction.CallbackContext context)
    {
        inventoryToggle.isOn = true;
        ToggleMenu();
    }

    public void OnMap(InputAction.CallbackContext context)
    {
        mapToggle.isOn = true;
        ToggleMenu();
    }

    public void OnStats(InputAction.CallbackContext context)
    {
        statsToggle.isOn = true;
        ToggleMenu();
    }

    private void ToggleMenu()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    /*
        Inventory menu buttons
    */
    public void OnInventoryToggleChanged()
    {
        if (inventoryToggle.isOn)
            ShowTab(inventoryTab);
    }

    public void OnMapToggleChanged()
    {
        if (mapToggle.isOn)
            ShowTab(mapTab);
    }

    public void OnStatsToggleChanged()
    {
        if (statsToggle.isOn)
            ShowTab(statsTab);
    }
    
    public void OnExitClicked()
    {
        ToggleMenu();
    }

    private void ShowTab(GameObject tabToShow)
    {
        HandleColors();

        inventoryTab.SetActive(tabToShow == inventoryTab);
        mapTab.SetActive(tabToShow == mapTab);
        statsTab.SetActive(tabToShow == statsTab);
    }

    private void HandleColors()
    {
        inventoryToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        mapToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        statsToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        if(inventoryToggle.isOn)
            inventoryToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (mapToggle.isOn)
            mapToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (statsToggle.isOn)
            statsToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
    }

    /*
        Inventory tab
    */

    /*
        Map tab
    */

    /*
        Stats tab
    */
}
