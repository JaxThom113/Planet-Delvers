using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Item
{
    [Header("Upgrade Settings")]
    [SerializeField] private int numBombs;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // allow player to lay a bomb by pressing X
            Debug.Log("Bombs collected!");
            Destroy(gameObject);
        }
    }
}
