using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scuttler : Enemy
{
    [Header("Scuttler Component References")]
    [SerializeField] ScuttlerChecks scuttlerChecks;
    [SerializeField] ScuttlerAnimations scuttlerAnimations;

    [Header("Surface Check Settings")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float maxFallSpeed;

    public Vector2 MovementVector { get; private set; }
    private Vector2 surfaceNormal;
    private bool traversing;

	private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // find the closest surface and stick to it
        rb.gravityScale = 0f;
        FindSurface();
        MovementVector = new Vector2(surfaceNormal.y, -surfaceNormal.x).normalized;

        // randomize starting direction
        if (Random.Range(0, 2) == 0)
        {
            MovementVector *= -1;
            
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    private void FindSurface()
    {
        int raycastDistance = 5;
        
        // send a raycast in each direction to find the closest surface
        RaycastHit2D[] hits = 
        {
            Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, LayerMask.GetMask("Ground")),
            Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, LayerMask.GetMask("Ground")),
            Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, LayerMask.GetMask("Ground")),
            Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, LayerMask.GetMask("Ground")),
        };

        // debug
        // Debug.DrawRay(transform.position, Vector2.up * raycastDistance, Color.red, 5f);
        // Debug.DrawRay(transform.position, Vector2.down * raycastDistance, Color.blue, 5f);
        // Debug.DrawRay(transform.position, Vector2.left * raycastDistance, Color.green, 5f);
        // Debug.DrawRay(transform.position, Vector2.right * raycastDistance, Color.yellow, 5f);

        RaycastHit2D closestHit = new RaycastHit2D();
        bool found = false;
        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                found = true;
            }
        }

        if (found)
        {
            // rotate and position the enemy onto the wall
            surfaceNormal = closestHit.normal;
            transform.up = surfaceNormal;
            transform.position = closestHit.point;
        }
    }

    private void Move()
    {
        transform.position += (Vector3)(MovementVector * speed * Time.fixedDeltaTime);

        // cap falling speed
        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    private void CheckGround()
    {
        // if enemy is on the ground, allow ledge/wall checks
        if (scuttlerChecks.IsGround && rb.velocity.y == 0)
        {
            CheckLedge();
            CheckWall();
        }
    }

    private void CheckLedge()
    {
        // if there is a ledge ahead, traverse around it
        if (!traversing && scuttlerChecks.IsLedge)
        {
            StartCoroutine(CornerTraverse(1));
        }
    }

    private void CheckWall()
    {
        // if there is a wall ahead, traverse on to it
        if (!traversing && scuttlerChecks.IsWall)
        {
            StartCoroutine(CornerTraverse(2));
        }
    }

    private IEnumerator CornerTraverse(int option)
    {
        traversing = true;

        RaycastHit2D hit;
        if (option == 1)
        {
            // ledge
            hit = Physics2D.Raycast(transform.position + (Vector3)(MovementVector * 0.2f), -surfaceNormal, 2f, groundLayer);
        }
        else
        {
            // wall
            hit = Physics2D.Raycast(transform.position, MovementVector, 1f, groundLayer);
        }

        if (hit.collider != null)
        {
            Vector2 newNormal = hit.normal;
            Vector2 newPosition = hit.point;

            yield return StartCoroutine(RotateToSurface(newNormal));

            surfaceNormal = newNormal;
            MovementVector = new Vector2(surfaceNormal.y, -surfaceNormal.x);

            if (Vector2.Dot(MovementVector, MovementVector) < 0)
                MovementVector *= -1;
        }

        traversing = false;
    }

    private IEnumerator RotateToSurface(Vector2 newNormal)
    {
        float startAngle = transform.eulerAngles.z;
        float targetAngle = Mathf.Atan2(newNormal.y, newNormal.x) * Mathf.Rad2Deg - 90f;

        float duration = 0.25f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = t / duration;
            float angle = Mathf.LerpAngle(startAngle, targetAngle, alpha);

            transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }
}
