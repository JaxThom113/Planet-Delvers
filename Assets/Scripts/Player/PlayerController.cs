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
    private SpriteRenderer sprite;
    private GroundCheck groundCheck;
	private Vector2 movementInput = Vector2.zero;
	private bool jumped;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        sprite = GetComponentInChildren<SpriteRenderer>();
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
        rb.velocity = new Vector2(movementInput.x * speed, rb.velocity.y);

        // flip the player sprite based on movement direction
        if (movementInput.x > 0)
            sprite.flipX = true;
        else if (movementInput.x < 0)
            sprite.flipX = false;
    }
}
