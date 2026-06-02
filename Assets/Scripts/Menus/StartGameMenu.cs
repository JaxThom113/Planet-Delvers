using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    private int selectedLength;
    private int selectedLevel;
    private int seed;

    void OnEnable()
    {
        HandleLengthColors();
        HandleLevelColors();

        selectedLength = 1;
        selectedLevel = 1;
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
        SceneManager.LoadScene("Overworld");
    }

    public void OnHelpClicked()
    {
        helpMenu.gameObject.SetActive(true);
    }

    public void OnShortLengthToggleChanged()
    {
        HandleLengthColors();
        selectedLength = 1;
    }

    public void OnMediumLengthToggleChanged()
    {
        HandleLengthColors();
        selectedLength = 2;
    }

    public void OnLongLengthToggleChanged()
    {
        HandleLengthColors();
        selectedLength = 3;
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
        selectedLevel = 1;
    }

    public void OnMediumLevelToggleChanged()
    {
        HandleLevelColors();
        selectedLevel = 1;
    }

    public void OnHardLevelToggleChanged()
    {
        HandleLevelColors();
        selectedLevel = 1;
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
    }
}
