using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScuttlerAnimations : MonoBehaviour
{
    [Header("Scuttler Component References")]
    [SerializeField] Scuttler scuttler;
    [SerializeField] ScuttlerChecks scuttlerChecks;

    private SpriteRenderer sprite;
    private Animator animator;

    private enum ScuttlerState
    {
        Idle,
        Walk,
        Fall
    }

    private ScuttlerState currentState;

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
        if (scuttlerChecks.IsGround)
        {
            if (scuttler.MovementVector.x != 0)
                currentState = ScuttlerState.Walk;
            else
                currentState = ScuttlerState.Idle;
        }
        else
        {
            if (scuttler.MovementVector.y != 0)
                currentState = ScuttlerState.Fall;
        }
    }

    private void UpdateAnimator()
    {
        switch (currentState)
        {
            case ScuttlerState.Idle: animator.Play("Idle"); break;

            case ScuttlerState.Walk: animator.Play("Walk"); break;

            case ScuttlerState.Fall: animator.Play("Fall"); break;
        }
    }
}