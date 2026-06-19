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
    [Range(1, 50)]
	[SerializeField] private float maxFallSpeed;

    [Header("Shooting Settings")]
	[SerializeField] private GameObject bullet;
	[SerializeField] private float shootCooldown;

	private Rigidbody2D rb;
    private GroundCheck groundCheck;
    private PlayerAnimations playerAnimations;

    public Vector2 MovementInput { get; private set; }
	private bool jumped;

    private float nextShootTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        playerAnimations = GetComponentInChildren<PlayerAnimations>();
    }

    void FixedUpdate()
    {
        Move();
        Jump();
    }

    /*
        Movement
    */
    public void OnMove(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        // change velocity depending on input
        rb.velocity = new Vector2(MovementInput.x * speed, rb.velocity.y);

        // cap falling speed
        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    /*
        Jumping
    */
    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
    }

    private void Jump()
    {
        if (jumped && groundCheck.IsGrounded)
        {
            // reset velocity before jumping, then apply jump force
            rb.velocity = Vector2.zero;
            rb.AddForce(transform.up * thrust * 100);
        }
    }

    /*
        Shooting
    */
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.time < nextShootTime)
            return;
        
        Shoot();

        nextShootTime = Time.time + shootCooldown;
    }

    private void Shoot()
    {
        Vector3 firePos = firePositions[0];
        Quaternion fireRotation = Quaternion.identity;

        switch (playerAnimations.currentFacing)
        {
            case PlayerAnimations.PlayerFacing.Left:
                firePos = firePositions[0];
                fireRotation = Quaternion.identity;
                break;
            case PlayerAnimations.PlayerFacing.Left_Up:
                firePos = firePositions[1];
                fireRotation = Quaternion.Euler(0, 0, -90);
                break;
            case PlayerAnimations.PlayerFacing.Left_Down:
                firePos = firePositions[2];
                fireRotation = Quaternion.Euler(0, 0, 90);
                break;
            case PlayerAnimations.PlayerFacing.Left_Prone:
                firePos = firePositions[3];
                fireRotation = Quaternion.identity;
                break;
            case PlayerAnimations.PlayerFacing.Right:
                firePos = firePositions[0];
                firePos.x = -firePos.x;
                fireRotation = Quaternion.Euler(0, 0, 180);
                break;
            case PlayerAnimations.PlayerFacing.Right_Up:
                firePos = firePositions[1];
                firePos.x = -firePos.x;
                fireRotation = Quaternion.Euler(0, 0, -90);
                break;
            case PlayerAnimations.PlayerFacing.Right_Down:
                firePos = firePositions[2];
                firePos.x = -firePos.x;
                fireRotation = Quaternion.Euler(0, 0, 90);
                break;
            case PlayerAnimations.PlayerFacing.Right_Prone:
                firePos = firePositions[3];
                firePos.x = -firePos.x;
                fireRotation = Quaternion.Euler(0, 0, 180);
                break;
        }

        Instantiate(bullet, transform.TransformPoint(firePos), fireRotation);
    }

    private readonly Vector3[] firePositions = 
    {
        new Vector3(-0.5f, 0.62f, 0),   // forward
        new Vector3(-0.128f, 1.43f, 0), // up
        new Vector3(-0, 0.128f, 0),     // down
        new Vector3(-0.5f, 0.24f, 0),   // prone
    };
}
