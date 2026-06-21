using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class StartGameMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private HelpMenu helpMenu;

    [Header("Mission Length")]
    [SerializeField] private Toggle shortLengthToggle;
    [SerializeField] private Toggle mediumLengthToggle;
    [SerializeField] private Toggle longLengthToggle;

    [Header("Mission Level")]
    [SerializeField] private Toggle easyLevelToggle;
    [SerializeField] private Toggle mediumLevelToggle;
    [SerializeField] private Toggle hardLevelToggle;

    [Header("Seed")]
    [SerializeField] private Toggle seedToggle;
    [SerializeField] private TMP_InputField seedInputField;

    private int length;
    private int level;
    private int seed;

    private bool isSeededRun;
    private bool validSeed;

    void OnEnable()
    {
        HandleLengthColors();
        HandleLevelColors();

        length = 0;
        level = 0;
        seed = 0;

        isSeededRun = false;
    }

    /*
        Start game menu buttons
    */
    public void OnBackClicked()
    {
        gameObject.SetActive(false);
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

        GameSystem.Instance.length = length;
        GameSystem.Instance.level = level;

        SceneManager.LoadScene("Overworld");
    }

    public void OnHelpClicked()
    {
        helpMenu.gameObject.SetActive(true);
    }

    public void OnShortLengthToggleChanged()
    {
        HandleLengthColors();
        length = 0;
    }

    public void OnMediumLengthToggleChanged()
    {
        HandleLengthColors();
        length = 1;
    }

    public void OnLongLengthToggleChanged()
    {
        HandleLengthColors();
        length = 2;
    }
    
    private void HandleLengthColors()
    {
        shortLengthToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        mediumLengthToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        longLengthToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        if(shortLengthToggle.isOn)
            shortLengthToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (mediumLengthToggle.isOn)
            mediumLengthToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        else if (longLengthToggle.isOn)
            longLengthToggle.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
    }

    public void OnEasyLevelToggleChanged()
    {
        HandleLevelColors();
        level = 0;
    }

    public void OnMediumLevelToggleChanged()
    {
        HandleLevelColors();
        level = 1;
    }

    public void OnHardLevelToggleChanged()
    {
        HandleLevelColors();
        level = 2;
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
}
