using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private HelpMenu helpMenu;
    [SerializeField] private ConfirmMenu confirmMenu;

    /*
        Pause menu input actions
    */
    public void OnPause(InputAction.CallbackContext context)
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
        Pause menu buttons
    */
    public void OnResumeClicked()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnOptionsClicked()
    {
        optionsMenu.gameObject.SetActive(true);
    }

    public void OnMainMenuClicked()
    {
        confirmMenu.SetDestination("MainMenu");
        confirmMenu.gameObject.SetActive(true);
    }
    
    public void OnHelpClicked()
    {
        helpMenu.gameObject.SetActive(true);
    }
}
