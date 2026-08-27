using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossDoor : Door
{
    private bool transitionActive;

    void Update()
    {
        if (hasEntered)
        {
            StartCoroutine(BossDoorTransition());
        }
    }

    private IEnumerator BossDoorTransition()
    {
        transitionActive = true;

        transitionActive = false;

        yield return null;
    }
}
