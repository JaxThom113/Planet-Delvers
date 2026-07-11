using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScuttlerChecks : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius;

    [Header("Ledge Check Settings")]
    [SerializeField] private Transform ledgeCheckPos;
    [SerializeField] private float ledgeCheckRadius;

    [Header("Wall Check Settings")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private float wallCheckRadius;

    public bool IsGround { get; private set; }
    public bool IsLedge { get; private set; }
    public bool IsWall { get; private set; }

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
    }

    private void OnDrawGizmosSelected()
    {
        // ground check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);

        // ledge check
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ledgeCheckPos.position, ledgeCheckRadius);

        // wall check
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheckPos.position, wallCheckRadius);
    }
}