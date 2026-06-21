using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryMenu inventoryMenu;

    [Header("Settings")]
	[SerializeField] private float movementSpeed;

	private Vector2 movementInput = Vector2.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // if inventory menu open and on map tab, allow movement
        if (inventoryMenu.mapTab.activeSelf)
        {
            Vector3 cameraPos = transform.position;

            // use unscaled delta time to allow movement while paused
            cameraPos.y += movementSpeed * movementInput.y * Time.unscaledDeltaTime;
            cameraPos.x += movementSpeed * movementInput.x * Time.unscaledDeltaTime;

            transform.position = cameraPos;
        }
    }
}
