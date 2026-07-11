using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperChecks : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius;

    [Header("Ledge Check Settings")]
    [SerializeField] private Transform ledgeCheckPos;
    [SerializeField] private float ledgeCheckRadius;

    [Header("Wall Check Settings")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private Transform highWallCheckPos;
    [SerializeField] private float wallCheckRadius;

    [Header("Player Check Settings")]
    [SerializeField] public Transform playerCheckPos;
    [SerializeField] public float playerCheckRadius;

    public bool IsGround { get; private set; }
    public bool IsLedge { get; private set; }
    public bool IsWall { get; private set; }
    public bool IsHighWall { get; private set; }
    public Collider2D IsPlayer { get; private set; }

    void FixedUpdate()
    {
        // checks if the enemy is on the ground
        IsGround = Physics2D.OverlapCircle(
            groundCheckPos.position,
            groundCheckRadius,
            groundLayer
        );

        // checks if a ledge is in front of the enemy
        IsLedge = Physics2D.OverlapCircle(
            ledgeCheckPos.position,
            ledgeCheckRadius,
            groundLayer
        ) == null;

        // checks if a wall is in front of the enemy
        IsWall = Physics2D.OverlapCircle(
            wallCheckPos.position,
            wallCheckRadius,
            groundLayer
        );

        // checks if there is a tall wall in front of the enemy that can't be traversed
        IsHighWall = Physics2D.OverlapCircle(
            highWallCheckPos.position,
            wallCheckRadius,
            groundLayer
        );

        // checks if a player is within the enemy's detection radiusd
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

        // ledge check
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ledgeCheckPos.position, ledgeCheckRadius);

        // wall checks
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheckPos.position, wallCheckRadius);
        Gizmos.DrawWireSphere(highWallCheckPos.position, wallCheckRadius);

        // player check
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerCheckPos.position, playerCheckRadius);
    }
}
