using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropperChecks : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius;

    [Header("Falling Ground Check Settings")]
    [SerializeField] public Transform fallingGroundCheckPos;
    [SerializeField] private float fallingGroundCheckRadius;

    [Header("Wall Check Settings")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private float wallCheckRadius;

    [Header("Player Check Settings")]
    [SerializeField] public Transform playerCheckPos;
    [SerializeField] public float playerCheckRadius;

    public bool IsGround { get; private set; }
    public bool IsFallingGround { get; private set; }
    public bool IsWall { get; private set; }
    public Collider2D IsPlayer { get; private set; }

    void FixedUpdate()
    {
        // checks if the enemy is on the ground
        IsGround = Physics2D.OverlapCircle(
            groundCheckPos.position,
            groundCheckRadius,
            groundLayer
        );

        // checks if a player is within the enemy's detection radius
        IsFallingGround = Physics2D.OverlapCircle(
            fallingGroundCheckPos.position,
            fallingGroundCheckRadius,
            groundLayer
        );

        // checks if a wall is in front of the enemy
        IsWall = Physics2D.OverlapCircle(
            wallCheckPos.position,
            wallCheckRadius,
            groundLayer
        );

        // checks if a player is within the enemy's detection radius
        IsPlayer = Physics2D.OverlapCircle(
            playerCheckPos.position,
            playerCheckRadius,
            playerLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        // ground check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);

        // falling ground check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(fallingGroundCheckPos.position, fallingGroundCheckRadius);

        // wall check
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheckPos.position, wallCheckRadius);

        // player check
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerCheckPos.position, playerCheckRadius);
    }
}
