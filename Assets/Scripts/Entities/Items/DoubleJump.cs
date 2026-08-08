using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJump : Item
{
    [Header("Upgrade Settings")]
    [SerializeField] private int extraJumps;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // increase the player's max number of midair jumps
            Debug.Log("Double Jump collected!");
            Destroy(gameObject);
        }
    }
}
