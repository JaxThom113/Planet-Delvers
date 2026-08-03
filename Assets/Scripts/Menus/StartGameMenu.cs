using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class StartGameMenu : Menu
{
    [Header("Menu References")]
    [SerializeField] private HelpMenu helpMenu;

    [Header("Mission Displays")]
    [SerializeField] private List<GameObject> displays;

    [Header("Mission Size")]
    [SerializeField] private Toggle smallSizeToggle;
    [SerializeField] private Toggle normalSizeToggle;
    [SerializeField] private Toggle largeSizeToggle;
    [SerializeField] private TextMeshProUGUI sizeDescription;

    [Header("Mission Level")]
    [SerializeField] private Toggle easyLevelToggle;
    [SerializeField] private Toggle mediumLevelToggle;
    [SerializeField] private Toggle hardLevelToggle;
    [SerializeField] private TextMeshProUGUI levelDescription;

    [Header("Seed")]
    [SerializeField] private Toggle seedToggle;
    [SerializeField] private TMP_InputField seedInputField;

    private int size;
    private int level;
    private int seed;

    private int displayIndex;
    private int sizeOffset;
    private int levelOffset;

    private bool isSeededRun;
    private bool validSeed;

    void OnEnable()
    {
        HandleSizeColors();
        HandleLevelColors();

        size = 1;
        level = 1;
        seed = 0;

        displayIndex = 4;
        sizeOffset = 3;
        levelOffset = 1;

        isSeededRun = false;
    }

    /*
        Start game menu buttons
    */
    public void OnBackClicked()
    {
        CloseMenu();
    }

    public void OnLaunchClicked()
    {
        if (isSeededRun)
        {
            if (validSeed)
            {
                GameSystem.Instance.seed = seed;
            }
            else
            {
                Debug.LogWarning("Invalid seed inputted, no longer seeded run & now generating new seed...");
                GameSystem.Instance.seed = Environment.TickCount;
            }
        }
        else
        {
            GameSystem.Instance.seed = Environment.TickCount;
        }

        GameSystem.Instance.size = size;
        GameSystem.Instance.level = level;

        SceneManager.LoadScene("Overworld");
    }

    public void OnHelpClicked()
    {
        helpMenu.gameObject.SetActive(true);
    }

    public void OnSmallSizeToggleChanged()
    {
        HandleSizeColors();
        sizeDescription.text = "Smaller world to get in & out quickly. 8x8 world, 10 min playtime.";
        size = 0;

        sizeOffset = 0;
        UpdateDisplay();
    }

    public void OnNormalSizeToggleChanged()
    {
        HandleSizeColors();
        sizeDescription.text = "Intended experience. 16x16 world, 20 min playtime.";
        size = 1;

        sizeOffset = 3;
        UpdateDisplay();
    }

    public void OnLargeSizeToggleChanged()
    {
        HandleSizeColors();
        sizeDescription.text = "Large world for veteran delvers. 24x24 world, 40 min playtime.";
        size = 2;

        sizeOffset = 6;
        UpdateDisplay();
    }
    
    private void HandleSizeColors()
    {
        smallSizeToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        normalSizeToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        largeSizeToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        if(smallSizeToggle.isOn)
            smallSizeToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (normalSizeToggle.isOn)
            normalSizeToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (largeSizeToggle.isOn)
            largeSizeToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
    }

    public void OnEasyLevelToggleChanged()
    {
        HandleLevelColors();
        levelDescription.text = "Low-intensity option. Eneimes deal half damage, bosses are less deadly.";
        level = 0;

        levelOffset = 0;
        UpdateDisplay();
    }

    public void OnMediumLevelToggleChanged()
    {
        HandleLevelColors();
        levelDescription.text = "Intended experience. Enemies deal moderate damage, bosses are fair.";
        level = 1;

        levelOffset = 1;
        UpdateDisplay();
    }

    public void OnHardLevelToggleChanged()
    {
        HandleLevelColors();
        levelDescription.text = "For the most skilled delvers. Enemies deal double damage, bosses are punishing.";
        level = 2;

        levelOffset = 2;
        UpdateDisplay();
    }

    private void HandleLevelColors()
    {
        easyLevelToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        mediumLevelToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        hardLevelToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        if(easyLevelToggle.isOn)
            easyLevelToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (mediumLevelToggle.isOn)
            mediumLevelToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (hardLevelToggle.isOn)
            hardLevelToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
    }

    public void OnSeedToggleChanged()
    {
        seedInputField.gameObject.SetActive(seedToggle.isOn);
        isSeededRun = !isSeededRun;
    }

    public void OnSeedInputValueChanged()
    {
        if (int.TryParse(seedInputField.text, out int result))
        {
            // number given as a seed
            seed = result;
            validSeed = true;
        }
        else
        {
            // invalid seed given, ignore
            validSeed = false;
        }

        // implement special seeds later down the line
        // else if (SeedSystem.Instance.GetSpecialSeed(seedInputField.text) != null)
        // {
        //     // unique name given as a seed, check if it exists in SeedSystem as a special seed
        //     GameData.SpecialSeed = seedInputField.text;
        //     validSeed = true;
        // }
    }

    public void UpdateDisplay()
    {
        Debug.Log(displayIndex);
        displayIndex = sizeOffset + levelOffset;

        foreach (GameObject display in displays)
            display.SetActive(false);

        if (displayIndex >= displays.Count)
            return;

        displays[displayIndex].SetActive(true);
    }
}
