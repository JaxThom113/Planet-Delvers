using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Respawn/Damage Settings")]
	[SerializeField] public Vector3 tempRespawn; // set to the position of last door a player walks through
	[SerializeField] public Vector3 respawn;     // set to the start room or last visited boss room
    [SerializeField] private float invincibilityFrames;
    [SerializeField] private Image fadeScreen;
    [SerializeField] public float resetSpeed;
    [SerializeField] public float hurtLength;


	private Rigidbody2D rb;
    private CapsuleCollider2D physicsCollider;
    private PlayerInput playerInput;

    private GroundCheck groundCheck;
    private HurtboxCheck hurtboxCheck;
    private PlayerAnimations playerAnimations;

    public Vector2 MovementInput { get; private set; }
	private bool jumped;

    private float nextShootTime = 0f;

    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCollider = GetComponent<CapsuleCollider2D>();
        playerInput = GetComponent<PlayerInput>();

        groundCheck = GetComponentInChildren<GroundCheck>();
        hurtboxCheck = GetComponentInChildren<HurtboxCheck>();
        playerAnimations = GetComponentInChildren<PlayerAnimations>();
    }

    void FixedUpdate()
    {
        Move();
        Jump();
        CheckCollisions();
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
        if (canMove)
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

    /*
        Damage
    */
    private void CheckCollisions()
    {
        if (hurtboxCheck.IsHazard)
        {
            // damage and reset the player's position
            StartCoroutine(HazardCollision());
        }
        else if (hurtboxCheck.IsEnemy)
        {
            // damage and knockback the player
            StartCoroutine(EnemyCollision());
        }
    }

    private IEnumerator HazardCollision()
    {
        StartCoroutine(StartInvincibilityFrames());

        // disable player's control
        playerInput.enabled = false;
        rb.simulated = false;
        physicsCollider.enabled = false;

        yield return StartCoroutine(FadeScreen(1f));

        // move player to room-specific respawn point 
        transform.position = tempRespawn;

        yield return new WaitForSeconds(0.2f);

        // re-enable player's control
        playerInput.enabled = true;
        rb.simulated = true;
        physicsCollider.enabled = true;

        yield return StartCoroutine(FadeScreen(0f));
    }

    private IEnumerator EnemyCollision()
    {
        StartCoroutine(StartInvincibilityFrames());

        canMove = false;

        // disable player's control
        playerInput.enabled = false;

        // get knockback direction and force based on the collision
        Vector2 dir = hurtboxCheck.KnockbackDir;
        float force = hurtboxCheck.knockbackForce;
     
        // launch the player back
        dir.Normalize();
        rb.velocity = Vector2.zero;
        rb.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(hurtLength);

        // re-enable player's control
        playerInput.enabled = true;

        canMove = true;
    }

    private IEnumerator StartInvincibilityFrames()
    {
        SpriteRenderer sprite = playerAnimations.GetComponent<SpriteRenderer>();
        
        float flickerInterval = 0.1f; // time between flickers
        float time = 0f;

        // disable hurtbox
        hurtboxCheck.GetComponent<CapsuleCollider2D>().enabled = false;

        while (time < invincibilityFrames)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(flickerInterval);
            time += flickerInterval;
        }

        // make sure sprite is visible at end, re-enable hurtbox
        sprite.enabled = true;
        hurtboxCheck.GetComponent<CapsuleCollider2D>().enabled = true;
    }

    private IEnumerator FadeScreen(float targetAlpha)
    {
        Color color = fadeScreen.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < resetSpeed)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / resetSpeed);

            color.a = newAlpha;
            fadeScreen.color = color;

            yield return null;
        }

        // snap exactly to target alpha
        color.a = targetAlpha;
        fadeScreen.color = color;
    }
}
