using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCell : Item
{
    [Header("Item Settings")]
    [SerializeField] private int addHealthAmount;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // increase player's max health
            if (GameSystem.Instance.playerCurrentHealth < GameSystem.Instance.playerMaxHealth) 
                GameSystem.Instance.playerCurrentHealth += 5;

            Destroy(gameObject);
        }
    }
}
