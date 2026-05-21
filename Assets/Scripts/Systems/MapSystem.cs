using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapSystem : MonoBehaviour
{
	[SerializeField] private GameObject map;
	[SerializeField] private GameObject mapCamera;
    [Range(5, 15)]
	[SerializeField] private float speed;
    private bool mapPressed;

    private bool buttonMap;
	private Vector2 movementInput = Vector2.zero;

    public void OnMap(InputAction.CallbackContext context)
    {
        buttonMap = context.action.triggered;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void Start()
    {
        map.SetActive(false);
        GameData.mapActive = false;
    }

    void Update()
    {
        if (buttonMap && !mapPressed)
        {
            if (GameData.mapActive)
            {
                map.SetActive(false);
                GameData.mapActive = false;
            }
            else
            {
                map.SetActive(true);
                GameData.mapActive = true;
            }
            mapPressed = true;
        }

        if (!buttonMap)
        {
            mapPressed = false;
        }

        if (GameData.mapActive)
        {
            Vector3 pos1 = mapCamera.transform.position;

            pos1.y += speed * movementInput.y * Time.deltaTime;
            pos1.x += speed * movementInput.x * Time.deltaTime;

            mapCamera.transform.position = pos1;
        }
    }
}
