using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private StartGameMenu startGameMenu;
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private CreditsMenu creditsMenu;
    [SerializeField] private ConfirmMenu confirmMenu;

    /*
        Main menu buttons
    */
    public void OnStartGameClicked()
    {
        startGameMenu.gameObject.SetActive(true);
    }

    public void OnOptionsClicked()  
    {
        optionsMenu.gameObject.SetActive(true);
    }

    public void OnCreditsClicked()
    {
        creditsMenu.gameObject.SetActive(true);
    }

    public void OnQuitClicked()
    {
        confirmMenu.SetDestination("Quit");
        confirmMenu.gameObject.SetActive(true);
    }
}
