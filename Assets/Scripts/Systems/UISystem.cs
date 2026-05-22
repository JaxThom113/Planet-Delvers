using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UISystem : Singleton<UISystem>
{
    [Header("HUD References")]
    [SerializeField] private GameObject pauseMenu;
    
    /*
        UI input actions
    */
    public void OnPause(InputAction.CallbackContext context)
    {
        if (pauseMenu.activeSelf)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
    }

    /*
        Pause menu
    */
    public void OnResumeClicked()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnOptionsClicked()
    {
        Debug.Log("Options will be implemented soon!");
    }

    public void OnMainMenuClicked()
    {
        Debug.Log("Main menu will be implemented soon!");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
