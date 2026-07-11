using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterAnimations : MonoBehaviour
{
    [Header("Drifter Component References")]
    [SerializeField] Drifter drifter;
    [SerializeField] DrifterChecks drifterChecks;

    private SpriteRenderer sprite;
    private Animator animator;

    private enum DrifterState
    {
        Idle,
        Walk,
        Fall
    }

    private DrifterState currentState;

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
        // first get base states (Idle, Walk, Jump, Fall)
        if (drifterChecks.IsGround)
        {
            if (drifter.MovementVector.x != 0)
                currentState = DrifterState.Walk;
            else
                currentState = DrifterState.Idle;
        }
        else
        {
            if (drifter.MovementVector.y != 0)
                currentState = DrifterState.Fall;
        }
    }

    private void UpdateAnimator()
    {
        switch (currentState)
        {
            case DrifterState.Idle: animator.Play("Idle"); break;

            case DrifterState.Walk: animator.Play("Walk"); break;

            case DrifterState.Fall: animator.Play("Fall"); break;
        }
    }
}