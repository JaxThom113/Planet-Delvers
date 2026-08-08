using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Item
{
    [Header("Upgrade Settings")]
    [SerializeField] private int distance;
    [SerializeField] private int speed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // allow a horizontal dash move for the player when they press LEFT SHIFT
            Debug.Log("Dash collected!");
            Destroy(gameObject);
        }
    }
}
