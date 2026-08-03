using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    public void CloseMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(CloseMenuCoroutine());
    }

    private IEnumerator CloseMenuCoroutine()
    {
        yield return null; // wait one frame
        gameObject.SetActive(false);
    }
}
