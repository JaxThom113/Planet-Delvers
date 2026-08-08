using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : Enemy
{
    [Header("Dropper Component References")]
    [SerializeField] DropperChecks dropperChecks;
    [SerializeField] DropperAnimations dropperAnimations;

    [Header("Surface Check Settings")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float maxFallSpeed;

    private Vector2 surfaceNormal;
    private bool foundPlayer;
    public Vector2 playerDir;
    private bool moving;

    public enum DropperState
    {
        Hiding,
        Falling,
        Running
    }

    private DropperState currentState;
    public DropperState CurrentState => currentState;

    private Rigidbody2D rb;
    float losTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // find the closest ceiling and stick to it
        rb.bodyType = RigidbodyType2D.Kinematic;
        losTimer = 0;
        FindSurface();

        currentState = DropperState.Hiding;
    }

    void Update()
    {
        if (currentState != DropperState.Falling)
        {
            // check enemy line of sight 10 times per second
            losTimer += Time.deltaTime;
            if (losTimer >= 0.1f)
            {
                losTimer = 0f;
                CheckForPlayer();
            }   
        }
    }

    private void CheckForPlayer()
    {
        if (dropperChecks.IsPlayer != null)
        {
            // if player is in range, shoot rays to check if the enemy has line of sight
            foundPlayer = FindPlayer();

            if (foundPlayer)
            {
                // first confirm with the angle of the ray that the player is below the dropper
                // look in the direction of the player
                if (transform.rotation.eulerAngles.z == 0)
                {
                    // normal orientation
                    // the dropper has fallen already, and has ran away and now is hiding on the ground
                    float playerDistance = Vector2.Distance(transform.position, dropperChecks.IsPlayer.transform.position);
                    if (playerDistance <= (dropperChecks.playerCheckRadius / 5))
                    {
                        // if player is within 1/5 of the playerCheckRadius, run away again
                        currentState = DropperState.Running;
                    }
                }
                else
                {
                    // upside down
                    float tolerance = 0.95f; // 18 degrees from down
                    if (Vector2.Dot(playerDir.normalized, Vector2.down) >= tolerance)
                    {
                        // fall if the player is there
                        currentState = DropperState.Falling;
                    }
                }
            }
        }
    }

    private void FindSurface()
    {
        int raycastDistance = 5;

        // send a raycast up to find the closest ceiling tile
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, LayerMask.GetMask("Ground"));

        // debug
        // Debug.DrawRay(transform.position, Vector2.up * raycastDistance, Color.red, 5f);

        if (hit.collider != null)
        {
            // rotate and position the enemy onto the ceiling
            surfaceNormal = hit.normal;
            transform.up = surfaceNormal;

            transform.position = hit.point;
        }
    }

    private bool FindPlayer()
    {
        bool found = false;

        // send a raycast in the direction of the player, ignore the Enemy layer and make sure it is cast from playerCheck origin
        int raycastDistance = (int)dropperChecks.playerCheckRadius;
        playerDir = (dropperChecks.IsPlayer.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(dropperChecks.playerCheckPos.position, playerDir, raycastDistance, ~LayerMask.GetMask("Enemy"));

        // debug
        Debug.DrawRay(dropperChecks.playerCheckPos.position, playerDir * raycastDistance, Color.red, 0.1f);

        if (hit.collider != null)
        {
            // if the Player layer was hit, player has been found
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                found = true;
        }

        return found;
    }

    void FixedUpdate()
    {
        Move();

        if (currentState != DropperState.Hiding)
        {
            CheckGround();
            FlipEnemy();
        }
    }

    private void Move()
    {
        if (!moving)
        {
            switch (currentState)
            {
                // dropper is concealed as a spike
                case DropperState.Hiding:
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    break;

                // dropper has detected the player and falls straight down
                case DropperState.Falling: 
                    // the OnCollisionEnter will handle the fall 
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    CheckFallingGround();
                    break;

                // dropper has recovered from its fall and flees from the player
                case DropperState.Running:
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                    break;
            }
        }

        // cap falling speed
        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    private IEnumerator StuckInGround()
    {
        moving = true;

        // stay stuck for 1.4 seconds, same length as the animation in DropperAnimations
        // the vibration animation plays in DropperAnimations at the same time as this coroutine
        yield return new WaitForSeconds(1.5f);

        // when the dropper starts running, pick a random direction
        if (Random.Range(0, 2) == 0)
            speed = -speed;

        currentState = DropperState.Running;
        
        moving = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector2.zero;
            currentState = DropperState.Running;
            moving = false;

            if (currentState == DropperState.Falling)
            {
                // when the dropper starts running, pick a random direction
                if (Random.Range(0, 2) == 0)
                    speed = -speed;
            }
            else
            {
                // turn around so you don't keep running at the player
                speed = -speed;
            }
        }
    }

    private void CheckGround()
    {
        // if enemy is on the ground, allow wall checks
        if (dropperChecks.IsGround && rb.velocity.y == 0)
        {
            CheckWall();
        }
    }

    private bool hasTurnedAtWall = false;
    private void CheckWall()
    {
        // if there is a wall ahead, turn around
        if (dropperChecks.IsWall)
        {
            if (!hasTurnedAtWall)
            {
                speed = -speed;
                hasTurnedAtWall = true;
            }
        }
        else
        {
            hasTurnedAtWall = false;
        }
    }

    private void CheckFallingGround()
    {
        if (dropperChecks.IsFallingGround)
        {
            // get stuck in the ground for a couple seconds, then jump up and run away
            StartCoroutine(StuckInGround());
        }
    }

    private void FlipEnemy()
    {
        // flip the enemy gameobject depending on changes to rb.velocity
        if (rb.velocity.x == 0)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -Mathf.Sign(rb.velocity.x);
        transform.localScale = scale;
    }
}
