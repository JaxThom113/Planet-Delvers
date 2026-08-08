using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeRifle : Item
{
    [Header("Upgrade Settings")]
    [SerializeField] private int chargeTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // allow a charge shot to be fired by clicking and holding RMB
            Debug.Log("Charge Rifle collected!");
            Destroy(gameObject);
        }
    }
}
