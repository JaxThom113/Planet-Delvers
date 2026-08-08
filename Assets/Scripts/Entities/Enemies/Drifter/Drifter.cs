using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drifter : Enemy
{
    [Header("Drifter Component References")]
    [SerializeField] DrifterChecks drifterChecks;
    [SerializeField] DrifterAnimations drifterAnimations;

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float maxFallSpeed;

    public Vector2 MovementVector { get; private set; }

	private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // randomize starting direction
        if (Random.Range(0, 2) == 0)
            speed = -speed;
    }

    void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    private void Move()
    {
        MovementVector = new Vector2(speed, rb.velocity.y);
        rb.velocity = MovementVector;

        // cap falling speed
        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    private void CheckGround()
    {
        // if enemy is on the ground, allow ledge/wall checks
        if (drifterChecks.IsGround && rb.velocity.y == 0)
        {
            CheckLedge();
            CheckWall();
        }
    }

    private void CheckLedge()
    {
        // if there is a ledge ahead, turn around
        if (drifterChecks.IsLedge)
        {
            speed = -speed;

            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void CheckWall()
    {
        // if there is a wall ahead, turn around
        if (drifterChecks.IsWall)
        {
            speed = -speed;

            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}
