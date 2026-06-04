using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
	[Range(5, 15)]
	[SerializeField] private float speed;
	[Range(0, 100)]
	[SerializeField] private float thrust;

	private Rigidbody2D rb;
    private GroundCheck groundCheck;

    public Vector2 MovementInput { get; private set; }
	private bool jumped;

    /*
        Player input actions
    */
    public void OnMove(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void FixedUpdate()
    {
        // jumping logic
        if (jumped && groundCheck.IsGrounded)
        {
            rb.AddForce(transform.up * thrust * 100);
            groundCheck.ResetGrounded();
        }

        // movement logic
        rb.velocity = new Vector2(MovementInput.x * speed, rb.velocity.y);
    }
}
