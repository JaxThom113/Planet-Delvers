using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    [SerializeField] private float playerSmoothingSpeed;
    [SerializeField] private float cameraSmoothingSpeed;

    private MainCamera mainCamera;
    private GameObject cameraObject;

    private PlayerController playerController;
    private PlayerInput playerInput;
    private GameObject player;
    private Rigidbody2D playerRb;
    private CapsuleCollider2D playerCollider;

    private PlayerCheck playerCheck;

    private bool transitionActive;
    [NonSerialized] public bool hasEntered;

    private readonly Vector3[] exitPositions = 
    {
        new Vector3(0, 3.2f, 0),   // up
        new Vector3(0, -5, 0),     // down
        new Vector3(-4, -1.9f, 0), // left
        new Vector3(4, -1.9f, 0),  // right
    };

    void Awake()
    {
        mainCamera = FindFirstObjectByType<MainCamera>();
        cameraObject =  mainCamera.gameObject;

        playerController = FindFirstObjectByType<PlayerController>();
        player = playerController.gameObject;
        playerInput = player.GetComponent<PlayerInput>();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerCollider = player.GetComponent<CapsuleCollider2D>();

        playerCheck = GetComponentInChildren<PlayerCheck>();

        transitionActive = false;
        hasEntered = false;
    }

    void Update()
    {
        if (playerCheck.PlayerCanEnter && !transitionActive)
        {
            hasEntered = true;

            switch (transform.eulerAngles.z)
            {
                case 0: // door facing left
                    if (playerController.MovementInput.x > 0)
                        StartCoroutine(DoorTransition(3));
                    break;
                case 90: // door facing down
                    if (playerController.MovementInput.y > 0)
                        StartCoroutine(DoorTransition(0));
                    break;
                case 180: // door facing right
                    if (playerController.MovementInput.x < 0)
                        StartCoroutine(DoorTransition(2));
                    break;
                case 270: // door facing up
                    if (playerController.MovementInput.y < 0)
                        StartCoroutine(DoorTransition(1));
                    break;
            }
        }
    }

    private IEnumerator DoorTransition(int direction)
    {
        transitionActive = true;

        // disable player's control
        playerInput.enabled = false;
        playerRb.simulated = false;
        playerCollider.enabled = false;

        // pull the player in towards the first door's origin
        yield return LerpUtility.Lerp(player, transform.position, playerSmoothingSpeed);

        // lerp the camera to the next room position
        mainCamera.ToggleSnapping(false);

        Vector3 cameraTarget = Vector3.zero;
        switch (direction)
        {
            case 0: cameraTarget = new Vector3(mainCamera.playerPos.x, mainCamera.playerPos.y + 18, -10); break;
            case 1: cameraTarget = new Vector3(mainCamera.playerPos.x, mainCamera.playerPos.y - 18, -10); break;
            case 2: cameraTarget = new Vector3(mainCamera.playerPos.x - 32, mainCamera.playerPos.y, -10); break;
            case 3: cameraTarget = new Vector3(mainCamera.playerPos.x + 32, mainCamera.playerPos.y, -10); break;
        }
        yield return LerpUtility.Lerp(cameraObject, cameraTarget, cameraSmoothingSpeed);

        mainCamera.ToggleSnapping(true);

        // place the player outside the second door, set new temp respawn point
        yield return LerpUtility.Lerp(player, transform.position + exitPositions[direction], playerSmoothingSpeed);
        playerController.tempRespawn = transform.position + exitPositions[direction];

        // re-enable player's control
        playerInput.enabled = true;
        playerRb.simulated = true;
        playerCollider.enabled = true;

        transitionActive = false;
    }
}
