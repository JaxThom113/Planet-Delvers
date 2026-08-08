using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : Menu
{
    void Start()
    {
        // buttonAnimator.Rebind();
        // buttonAnimator.Update(0);
    }

    /*
        Credits menu buttons
    */
    public void OnBackClicked()
    {
        CloseMenu();
    }
    
    public void OnItchClicked()
    {
        Application.OpenURL("https://jax-th.itch.io/");
    }
}
