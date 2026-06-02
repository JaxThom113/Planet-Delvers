using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapSystem : MonoBehaviour
{
	[SerializeField] private GameObject inventoryMenu;
	[SerializeField] private GameObject mapTab;
	[SerializeField] private GameObject mapCamera;
    [Range(5, 15)]
	[SerializeField] private float speed;

	private Vector2 movementInput = Vector2.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // if inventory menu open and on map tab, allow movement
        if (inventoryMenu.activeSelf)
        {
            Vector3 pos1 = mapCamera.transform.position;

            // use unscaled delta time to allow movement while paused
            pos1.y += speed * movementInput.y * Time.unscaledDeltaTime;
            pos1.x += speed * movementInput.x * Time.unscaledDeltaTime;

            mapCamera.transform.position = pos1;
        }
    }
}
