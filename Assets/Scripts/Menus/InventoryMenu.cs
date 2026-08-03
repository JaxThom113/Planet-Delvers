using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class InventoryMenu : Menu
{
    [Header("Menu References")]
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Inventory Tab")]
    [SerializeField] private Toggle inventoryToggle;
    [SerializeField] public GameObject inventoryTab;
    [SerializeField] private GameObject playerImage;

    [Header("Map Tab")]
    [SerializeField] private Toggle mapToggle;
    [SerializeField] public GameObject mapTab;
    [SerializeField] private MapCamera mapCamera;
    [SerializeField] private TextMeshProUGUI coordinates;

    [Header("Stats Tab")]
    [SerializeField] private Toggle statsToggle;
    [SerializeField] public GameObject statsTab;
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI seed;
    [SerializeField] private TextMeshProUGUI size;
    [SerializeField] private TextMeshProUGUI level;

    void OnEnable()
    {
        HandleColors();
        UpdateStats();
    }

    void Update()
    {
        UpdateMap();
        UpdateClock();
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
        else if (!pauseMenu.gameObject.activeSelf)
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
        CloseMenu();
        Time.timeScale = 1;
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
    private void UpdateMap()
    {
        // update coordinates to current position of map camera
        coordinates.text = $"({mapCamera.cameraPos.x:F2}, {mapCamera.cameraPos.y:F2})"; 
    }

    /*
        Stats tab
    */
    private void UpdateStats()
    {
        // update current seed display
        seed.text = GameSystem.Instance.seed.ToString();

        // update world size
        switch (GameSystem.Instance.size)
        {
            case 0: 
                size.text = "Small";
                size.color = new Color32(76, 255, 0, 255);
                break;
            case 1: 
                size.text = "Normal";
                size.color = new Color32(0, 148, 255, 255);
                break;
            case 2: 
                size.text = "Large";
                size.color = new Color32(255, 0, 0, 255);
                break;
        }

        // update game difficulty level
        switch (GameSystem.Instance.level)
        {
            case 0: 
                level.text = "Easy";
                level.color = new Color32(76, 255, 0, 255);
                break;
            case 1: 
                level.text = "Medium";
                level.color = new Color32(0, 148, 255, 255);
                break;
            case 2: 
                level.text = "Hard";
                level.color = new Color32(255, 0, 0, 255);
                break;
        }
    }

    private void UpdateClock()
    {
        float elaspedTime = Time.timeSinceLevelLoad;

        int hours = Mathf.FloorToInt(elaspedTime / 3600);
        int minutes = Mathf.FloorToInt((elaspedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elaspedTime % 60);

        time.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}
