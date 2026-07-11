using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumper : MonoBehaviour
{
    [Header("Jumper Component References")]
    [SerializeField] JumperChecks jumperChecks;
    [SerializeField] JumperAnimations jumperAnimations;

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float jumpForce;
	[SerializeField] private float attackJumpForce;
	[SerializeField] private float maxFallSpeed;
	[SerializeField] private float territoryRadius;

    private bool foundPlayer;
    public Vector2 playerDir;
    private Vector2 territoryPos;
    private bool moving;
    private bool jumped;

    private enum JumperState
    {
        Patrolling,
        Chasing,
        Wandering
    }

    private JumperState currentState;

    private Rigidbody2D rb;
    float losTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // establish territory origin
        territoryPos = transform.position;
        losTimer = 0;

        currentState = JumperState.Patrolling;
    }

    void Update()
    {
        CheckForPlayer();
    }

    private void CheckForPlayer()
    {
        if (jumperChecks.IsPlayer != null)
        {
            // check enemy line of sight 10 times per second
            losTimer += Time.deltaTime;
            if (losTimer >= 0.1f)
            {
                losTimer = 0f;

                // if player is in range, shoot rays to check if the enemy has line of sight
                foundPlayer = FindPlayer();
                if (foundPlayer)
                    currentState = JumperState.Chasing; Debug.Log("Chasing!");
            }   
        }
    }

    private bool FindPlayer()
    {
        bool found = false;

        // send a raycast in the direction of the player
        int raycastDistance = (int)jumperChecks.playerCheckRadius;
        playerDir = (jumperChecks.IsPlayer.transform.position - transform.position).normalized;
        RaycastHit2D hit =  Physics2D.Raycast(jumperChecks.playerCheckPos.position, playerDir, raycastDistance, ~LayerMask.GetMask("Enemy"));

        // debug
        Debug.DrawRay(jumperChecks.playerCheckPos.position, playerDir * raycastDistance, Color.red, 0.1f);

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
        CheckGround();

        FlipEnemy();
    }

    private void Move()
    {
        bool withinTerritory = Vector2.Distance(transform.position, territoryPos) <= territoryRadius;

        if (!moving)
        {
            jumped = false;

            switch (currentState)
            {
                // jumper moving randomly within its territory
                case JumperState.Patrolling:
                    int randDistance = Random.Range(1, 11);

                    // either move randomly within territory or move randomly back to territory
                    if (withinTerritory)
                        StartCoroutine(MoveInTerritory(randDistance));
                    else
                        StartCoroutine(MoveTowardTerritory(randDistance));

                    break;

                // jumper is chasing and jumping at the player
                case JumperState.Chasing:
                    StartCoroutine(ChaseAndAttackPlayer());
                    break;

                // has left its territory, just wander randomly now
                case JumperState.Wandering:
                    randDistance = Random.Range(1, 11);
                    StartCoroutine(MoveInTerritory(randDistance));
                    break;
            }
        }

        // cap falling speed
        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    private IEnumerator MoveInTerritory(float distance)
    {
        moving = true;

        // pick a random direction
        float randDir;
        if (Random.Range(0, 2) == 0)
            randDir = speed;
        else
            randDir = -speed;


        // start moving
        // keep a total distance traveled variable
        // once it equals float distance, stop moving
        float distanceTraveled = 0;
        Vector2 previousPosition = transform.position;
        Vector2 currentPosition = transform.position;

        while (distanceTraveled < distance)
        {
            if (foundPlayer)
            {
                Debug.Log("Found player while in territory!");
                yield break; 
            }
            
            rb.velocity = new Vector2(randDir, rb.velocity.y);
            yield return new WaitForFixedUpdate();

            // add the position delta to keep track of how much distance traveled
            distanceTraveled += Mathf.Abs(currentPosition.x - previousPosition.x);
            previousPosition = currentPosition;
        }

        // // move a random distance
        // float startX = transform.position.x;
        // while (Mathf.Abs(transform.position.x - startX) < distance)
        // {
        //     if (foundPlayer)
        //         yield break;
            
        //     rb.velocity = new Vector2(randDir, rb.velocity.y);
        //     yield return new WaitForFixedUpdate();
        // }

        // rb.velocity = new Vector2(0f, rb.velocity.y);

        // wait a random number of seconds
        int randSeconds = Random.Range(1, 11);
        yield return new WaitForSeconds(randSeconds);
        
        moving = false;
    }

    private IEnumerator MoveTowardTerritory(float distance)
    {
        moving = true;

        // move in direction of territory
        float territoryDir;
        if (territoryPos.x > transform.position.x)
            territoryDir = speed;
        else
            territoryDir = -speed;

        // start a timer of 5 seconds, when it ends, give up on moving back to the territory
        float timeout = 5f;
        float elapsed = 0f;

        // start moving
        // keep a total distance traveled variable
        // once it equals float distance, stop moving
        float distanceTraveled = 0;
        Vector2 previousPosition = transform.position;
        Vector2 currentPosition = transform.position;

        while (distanceTraveled < distance && elapsed < timeout)
        {
            if (foundPlayer)
            {
                Debug.Log("Found player while out of territory!");
                yield break; 
            }

            elapsed += Time.fixedDeltaTime;
            
            rb.velocity = new Vector2(territoryDir, rb.velocity.y);
            yield return new WaitForFixedUpdate();

            // add the position delta to keep track of how much distance traveled
            distanceTraveled += Mathf.Abs(currentPosition.x - previousPosition.x);
            previousPosition = currentPosition;
        }

        // move a random distance back toward the territory
        // float startX = transform.position.x;
        // while (Mathf.Abs(transform.position.x - startX) < distance && elapsed < timeout)
        // {
        //     if (foundPlayer)
        //         yield break;
            
        //     elapsed += Time.fixedDeltaTime;
            
        //     rb.velocity = new Vector2(territoryDir, rb.velocity.y);
        //     yield return new WaitForFixedUpdate();
        // }

        rb.velocity = new Vector2(0f, rb.velocity.y);

        if (elapsed >= timeout)
        {
            currentState = JumperState.Wandering;
            Debug.Log("Wandering!");
            yield break;
        }

        moving = false;
    }

    private IEnumerator ChaseAndAttackPlayer()
    {
        moving = true;

        // jumper will jump at player when they are within half of the detection radius
        float jumpDistance = jumperChecks.playerCheckRadius / 2;

        // get the player's location at this moment and time and move toward them
        Vector3 observedLocation = jumperChecks.IsPlayer.transform.position;

        // move towards player
        while (Vector3.Distance(transform.position, observedLocation) < jumpDistance)
        {
            if (playerDir.x > 0)
                rb.velocity = new Vector2(-Mathf.Abs(speed), 0);
            else
                rb.velocity = new Vector2(Mathf.Abs(speed), 0);
            
            yield return new WaitForFixedUpdate();
        }

        // when at a certain distance, stop for a couple of seconds and then jump
        yield return new WaitForSeconds(0.5f);

        Vector2 jumpDir = new Vector2(Mathf.Sign(speed)*attackJumpForce, attackJumpForce/2);
        rb.AddForce(jumpDir, ForceMode2D.Impulse);

        // have a slight cooldown after landing, foor the attack animation
        yield return new WaitForSeconds(0.5f);

        // if the player is outside the jumper's detection radius for more than 5 seconds, switch to wander

        moving = false;
    }

    private void CheckGround()
    {
        // if enemy is on the ground, allow ledge/wall checks
        if (jumperChecks.IsGround && rb.velocity.y == 0)
        {
            CheckLedge();
            CheckWall();
        }
    }

    private bool stopped = false;
    private void CheckLedge()
    {
        // if there is a steep ledge ahead (more than 2 blocks), turn around so the jumper does not fall
        if (jumperChecks.IsLedge)
        {
            // cast an additional ray downward to check how far of a drop it is
            int raycastDistance = (int)jumpForce;
            RaycastHit2D hit =  Physics2D.Raycast(transform.position, Vector2.down, raycastDistance);

            if (hit.collider == null)
            {
                if (Random.Range(0, 2) == 0 && !stopped)
                {
                    // just stop
                    rb.velocity = Vector2.zero;
                    stopped = true;
                }
                else
                {
                    // turn around, keep moving
                    rb.velocity *= -1;
                    
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;

                    stopped = false;
                }
            }
        }
    }

    private void CheckWall()
    {
        if (jumperChecks.IsHighWall)
        {
            // if there is a wall ahead that is more than 1 block tall, turn around
            speed = -speed;
        }
        else if (jumperChecks.IsWall && !jumped)
        {
            // if there is a 1 block tall wall ahead, jump over it
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumped = true;
        }
    }

    private void FlipEnemy()
    {
        if (rb.velocity.x == 0)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -Mathf.Sign(rb.velocity.x);
        transform.localScale = scale;
    }
}
