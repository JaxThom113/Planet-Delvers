using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmMenu : MonoBehaviour
{
    private string destination;

    public void SetDestination(string destination)
    {
        this.destination = destination;
    }

    /*
        Confirm menu buttons
    */
    public void OnYesClicked()
    {
        Time.timeScale = 1;

        // if destination is "quit", close the game, else navigate to new scene
        if (destination == "Quit")
            Application.Quit();
        else
            SceneManager.LoadScene(destination);
    }

    public void OnNoClicked()
    {
        gameObject.SetActive(false);
    }
}
