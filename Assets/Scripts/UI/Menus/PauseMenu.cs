using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : Menu
{
    [Header("Menu References")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private InventoryMenu inventoryMenu;
    [SerializeField] private HelpMenu helpMenu;
    [SerializeField] private ConfirmMenu confirmMenu;

    /*
        Pause menu input actions
    */
    public void OnPause(InputAction.CallbackContext context)
    {
        bool noOtherMenusOpen = !optionsMenu.gameObject.activeSelf
            && !inventoryMenu.gameObject.activeSelf
            && !helpMenu.gameObject.activeSelf
            && !confirmMenu.gameObject.activeSelf;
        
        if (gameObject.activeSelf && noOtherMenusOpen)
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }
        else if (!inventoryMenu.gameObject.activeSelf)
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
        CloseMenu();
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
