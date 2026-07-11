using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropperAnimations : MonoBehaviour
{
    [Header("Dropper Component References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Dropper dropper;
    [SerializeField] DropperChecks dropperChecks;

    private Animator animator;
    private SpriteRenderer sprite;

    private enum DropperState
    {
        Idle,
        Walk, Walk_Up, Walk_Down,
        Look_Left, Look_Up_Left, Look_Up, Look_Up_Right, Look_Right,
        Peek1, Peek2, Peek3,
        Fall, Hide
    }

    private DropperState currentState;
    private DropperState previousState;

    private Coroutine peekRoutine;
    private Coroutine stuckRoutine;
    private bool isPeeking;
    private bool hasFallen;

    void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        isPeeking = false;
        hasFallen = false;
    }

    void Update()
    {
        UpdateState();
        UpdateAnimator();
    }

    private void UpdateState()
    {
        if (currentState == DropperState.Hide)
            return;

        // check the current state in the Dropper's movement script
        switch (dropper.CurrentState)
        {
            case Dropper.DropperState.Hiding: 

                if (currentState != DropperState.Peek1 && currentState != DropperState.Peek2 && currentState != DropperState.Peek3)
                {
                    currentState = DropperState.Idle;
                }

                if (!isPeeking)
                    StartPeeking();

                if (dropperChecks.IsPlayer != null && !isPeeking)
                    HidingLookAtPlayer();

                break;

            case Dropper.DropperState.Falling: 

                StopPeeking();

                currentState = DropperState.Fall;

                if (dropperChecks.IsFallingGround && !hasFallen)
                    StartStuck();

                break;

            case Dropper.DropperState.Running: 

                StopPeeking();
                StopStuck();

                currentState = DropperState.Walk;

                if (dropperChecks.IsPlayer != null)
                    RunningLookAtPlayer();

                return;
        }
    }

    private void UpdateAnimator()
    {
        // if current state is equal to last state, no need to replay the animation
        if (currentState == previousState)
            return;

        switch (currentState)
        {
            case DropperState.Idle: animator.Play("Idle"); break;

            case DropperState.Walk: animator.Play("Walk"); break;
            case DropperState.Walk_Up: animator.Play("Walk_Up"); break;
            case DropperState.Walk_Down: animator.Play("Walk_Down"); break;

            case DropperState.Look_Left: animator.Play("Look_Left"); break;
            case DropperState.Look_Up_Left: animator.Play("Look_Up_Left"); break;
            case DropperState.Look_Up: animator.Play("Look_Up"); break;
            case DropperState.Look_Up_Right: animator.Play("Look_Up_Right"); break;
            case DropperState.Look_Right: animator.Play("Look_Right"); break;

            case DropperState.Peek1: animator.Play("Peek1"); break;
            case DropperState.Peek2: animator.Play("Peek2"); break;
            case DropperState.Peek3: animator.Play("Peek3"); break;

            case DropperState.Fall: animator.Play("Fall"); break;
            case DropperState.Hide: animator.Play("Hide"); break;
        }

        previousState = currentState;
    }

    /*
        One-shot animations
    */

    private void StartPeeking()
    {
        if (peekRoutine == null)
        {
            peekRoutine = StartCoroutine(PeekAnimations());
        }
    }

    private void StopPeeking()
    {
        if (peekRoutine != null)
        {
            StopCoroutine(peekRoutine);
            peekRoutine = null;
        }
        isPeeking = false; 
    }

    private IEnumerator PeekAnimations()
    {
        while (true)
        {
            isPeeking = true;

            switch (Random.Range(0, 3))
            {
                case 0: currentState = DropperState.Peek1; break;
                case 1: currentState = DropperState.Peek2; break;
                case 2: currentState = DropperState.Peek3; break;
            }

            yield return null;

            // wait 10-15 seconds (keep in mind, longest peek animation is 6 seconds)
            yield return new WaitForSeconds(Random.Range(10f, 15f));

            isPeeking = false;
            currentState = DropperState.Idle;
            peekRoutine = null;
            yield break;
        }
    }

    private void StartStuck()
    {
        if (stuckRoutine == null)
        {
            stuckRoutine = StartCoroutine(StuckInGroundAnimation());
        }
    }

    private void StopStuck()
    {
        if (stuckRoutine != null)
        {
            StopCoroutine(stuckRoutine);
            stuckRoutine = null;
        }
    }

    private IEnumerator StuckInGroundAnimation()
    {
        hasFallen = true;

        currentState = DropperState.Hide;

        Vector3 startPosition = sprite.transform.position;
        Quaternion startRotation = sprite.transform.rotation;

        Vector3 pivot = dropperChecks.fallingGroundCheckPos.position;

        // stay stuck for 1 second
        yield return new WaitForSeconds(1f);

        // vibration parameters
        float vibrationDuration = 0.4f;
        float elapsed = 0f;
        float maxAngle = 8f;
        float frequency = 25f;
        float previousAngle = 0f;

        while (elapsed < vibrationDuration)
        {
            elapsed += Time.deltaTime;

            float currentAngle = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * maxAngle;
            float deltaAngle = currentAngle - previousAngle;

            // rotate around the pivot point (the spike's tip)
            sprite.transform.RotateAround(pivot, Vector3.forward, deltaAngle);
            previousAngle = currentAngle;

            yield return null;
        }

        // snap back exactly to original points
        sprite.transform.position = startPosition;
        sprite.transform.rotation = startRotation;

        currentState = DropperState.Walk;
        stuckRoutine = null;
    }

    /*
        Directional animations
    */

    private void HidingLookAtPlayer()
    {
        // looking is done in Idle, so stop a peeking animation if one is running
        StopPeeking();

        // if upside down, flip into the enemy's local up direction
        Vector2 dir = dropper.playerDir.normalized;
        if (transform.rotation.eulerAngles.z != 0)
            dir = -dir;

        // Atan2 gives -180..180, where 0 = right, 90 = up, 180/-180 = left
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // clamp so when the player is below the enemy an edge wedge is still used
        angle = Mathf.Clamp(angle, 0f, 180f);

        switch (angle)
        {
            case < 36f: currentState = DropperState.Look_Left; break;
            case < 72f: currentState = DropperState.Look_Up_Left; break;
            case < 108f: currentState = DropperState.Look_Up; break;
            case < 144f: currentState = DropperState.Look_Up_Right; break;
            case <= 180f: currentState = DropperState.Look_Right; break;
        }
    }

    private void RunningLookAtPlayer()
    {
        Vector2 dir = dropper.playerDir.normalized;
        if (dir == Vector2.zero)
            dir = Vector2.left * Mathf.Sign(rb.velocity.x);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle > 45f)
            currentState = DropperState.Walk_Up;
        else if (angle < -45f)
            currentState = DropperState.Walk_Down;
        else
            currentState = DropperState.Walk;
    }
}
