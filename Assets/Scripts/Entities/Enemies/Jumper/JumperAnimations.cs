using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperAnimations : MonoBehaviour
{
    [Header("Jumper Component References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] Jumper jumper;
    [SerializeField] JumperChecks jumperChecks;

    private SpriteRenderer sprite;
    private Animator animator;

    private enum JumperState
    {
        Idle,
        Walk, Walk_Up, Walk_Down,
        Jump, Fall,
        Attack
    }

    private JumperState currentState;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateState();
        UpdateAnimator();
    }

    private void UpdateState()
    {
        if (jumperChecks.IsGround)
        {
            if (rb.velocity.x != 0)
                currentState = JumperState.Walk;
            else
                currentState = JumperState.Idle;
        }
        else
        {
            if (rb.velocity.y > 0)
                currentState = JumperState.Jump;
            else if (rb.velocity.y < 0)
                currentState = JumperState.Fall;
        }
    }

    private void UpdateAnimator()
    {
        switch (currentState)
        {
            case JumperState.Idle: animator.Play("Idle"); break;

            case JumperState.Walk: animator.Play("Walk"); break;
            case JumperState.Walk_Up: animator.Play("Walk_Up"); break;
            case JumperState.Walk_Down: animator.Play("Walk_Down"); break;

            case JumperState.Jump: animator.Play("Jump"); break;
            case JumperState.Fall: animator.Play("Fall"); break;

            case JumperState.Attack: animator.Play("Attack"); break;
        }
    }
}
