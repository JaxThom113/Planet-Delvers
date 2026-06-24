using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private HurtboxCheck hurtboxCheck;
    [SerializeField] private PlayerController playerController;

    private SpriteRenderer sprite;
    private Animator animator;

    public enum PlayerFacing
    {
        Left, Left_Up, Left_Down, Left_Prone,
        Right, Right_Up, Right_Down, Right_Prone,
    }

    public PlayerFacing currentFacing { get; private set; }

    private enum PlayerState
    {
        Idle, Idle_Up,
        Walk, Walk_Up,
        Jump, Jump_Up, Jump_Down,
        Fall, Fall_Up, Fall_Down,
        Prone,
        Hurt
    }

    private PlayerState currentState;

    private bool isHurt;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        currentFacing = PlayerFacing.Left;
    }

    void Update()
    {
        UpdateState();
        UpdateAnimator();
        UpdateFacing();
    }

    private void UpdateState()
    {
        // first get base states (Idle, Walk, Jump, Fall)
        if (groundCheck.IsGrounded)
        {
            if (playerController.MovementInput.x != 0)
                currentState = PlayerState.Walk;
            else
                currentState = PlayerState.Idle;
        }
        else
        {
            if (rb.velocity.y > 0)
                currentState = PlayerState.Jump;
            else if (rb.velocity.y < 0)
                currentState = PlayerState.Fall;
        }

        // then get directional states (Idle_Up, Walk_Up, etc.)
        switch (currentState)
        {
            case PlayerState.Idle:
                if (playerController.MovementInput.y > 0)
                    currentState = PlayerState.Idle_Up;
                else if (playerController.MovementInput.y < 0)
                    currentState = PlayerState.Prone;
                break;

            case PlayerState.Walk:
                if (playerController.MovementInput.y > 0)
                    currentState = PlayerState.Walk_Up;
                break;

            case PlayerState.Jump:
                if (playerController.MovementInput.y > 0)
                    currentState = PlayerState.Jump_Up;
                else if (playerController.MovementInput.y < 0)
                    currentState = PlayerState.Jump_Down;
                break;

            case PlayerState.Fall:
                if (playerController.MovementInput.y > 0)
                    currentState = PlayerState.Fall_Up;
                else if (playerController.MovementInput.y < 0)
                    currentState = PlayerState.Fall_Down;
                break;
        }

        if (!isHurt && hurtboxCheck.IsHazard)
        {
            StartCoroutine(HazardHurtAnimation());
        }
        else if (!isHurt && hurtboxCheck.IsEnemy)
        {
            StartCoroutine(EnemyHurtAnimation());
        }

        if (isHurt)
        {
            currentState = PlayerState.Hurt;
            return;
        }
    }

    private void UpdateAnimator()
    {
        // flip the player sprite based on movement direction
        if (playerController.MovementInput.x > 0)
            sprite.flipX = true;
        else if (playerController.MovementInput.x < 0)
            sprite.flipX = false;
        
        switch (currentState)
        {
            case PlayerState.Idle:
                animator.Play("Idle");
                break;

            case PlayerState.Idle_Up:
                animator.Play("Idle_Up");
                break;

            case PlayerState.Walk:
                animator.Play("Walk");
                break;

            case PlayerState.Walk_Up:
                animator.Play("Walk_Up");
                break;

            case PlayerState.Jump:
                animator.Play("Jump");
                break;

            case PlayerState.Jump_Up:
                animator.Play("Jump_Up");
                break;

            case PlayerState.Jump_Down:
                animator.Play("Jump_Down");
                break;

            case PlayerState.Fall:
                animator.Play("Fall");
                break;

            case PlayerState.Fall_Up:
                animator.Play("Fall_Up");
                break;

            case PlayerState.Fall_Down:
                animator.Play("Fall_Down");
                break;
            
            case PlayerState.Prone:
                animator.Play("Prone");
                break;

            case PlayerState.Hurt:
                animator.Play("Hurt");
                break;
        }
    }

    private void UpdateFacing()
    {
        if (sprite.flipX)
        {
            // facing right
            switch (currentState)
            {
                case PlayerState.Idle: currentFacing = PlayerFacing.Right; break;
                case PlayerState.Walk: currentFacing = PlayerFacing.Right; break;
                case PlayerState.Jump: currentFacing = PlayerFacing.Right; break;
                case PlayerState.Fall: currentFacing = PlayerFacing.Right; break;

                case PlayerState.Idle_Up: currentFacing = PlayerFacing.Right_Up; break;
                case PlayerState.Walk_Up: currentFacing = PlayerFacing.Right_Up; break;
                case PlayerState.Jump_Up: currentFacing = PlayerFacing.Right_Up; break;
                case PlayerState.Fall_Up: currentFacing = PlayerFacing.Right_Up; break;

                case PlayerState.Jump_Down: currentFacing = PlayerFacing.Right_Down; break;
                case PlayerState.Fall_Down: currentFacing = PlayerFacing.Right_Down; break;
                 
                case PlayerState.Prone: currentFacing = PlayerFacing.Right_Prone; break;
            }
        }
        else
        {
            // facing left
            switch (currentState)
            {
                case PlayerState.Idle: currentFacing = PlayerFacing.Left; break;
                case PlayerState.Walk: currentFacing = PlayerFacing.Left; break;
                case PlayerState.Jump: currentFacing = PlayerFacing.Left; break;
                case PlayerState.Fall: currentFacing = PlayerFacing.Left; break;

                case PlayerState.Idle_Up: currentFacing = PlayerFacing.Left_Up; break;
                case PlayerState.Walk_Up: currentFacing = PlayerFacing.Left_Up; break;
                case PlayerState.Jump_Up: currentFacing = PlayerFacing.Left_Up; break;
                case PlayerState.Fall_Up: currentFacing = PlayerFacing.Left_Up; break;

                case PlayerState.Jump_Down: currentFacing = PlayerFacing.Left_Down; break;
                case PlayerState.Fall_Down: currentFacing = PlayerFacing.Left_Down; break;
                 
                case PlayerState.Prone: currentFacing = PlayerFacing.Left_Prone; break;
            }
        }
    }

    private IEnumerator HazardHurtAnimation()
    {
        isHurt = true;
        yield return new WaitForSeconds(playerController.resetSpeed);
        isHurt = false;
    }

    private IEnumerator EnemyHurtAnimation()
    {
        isHurt = true;
        yield return new WaitForSeconds(playerController.hurtLength);
        isHurt = false;
    }
}