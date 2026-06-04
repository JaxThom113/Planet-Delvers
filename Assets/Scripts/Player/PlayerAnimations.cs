using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private PlayerController playerController;

    private SpriteRenderer sprite;
    private Animator animator;
    private bool prevGrounded;

    private enum PlayerState
    {
        Idle,
        Idle_Up,
        Walk,
        Walk_Up,
        Jump,
        Jump_Up,
        Jump_Down,
        Fall,
        Fall_Up,
        Fall_Down,
        Prone,
        Hurt
    }

    private PlayerState currentState;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateState();
        UpdateAnimator();
        prevGrounded = groundCheck.IsGrounded;
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
}