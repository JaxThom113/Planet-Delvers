using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUp : Item
{
    [Header("Upgrade Settings")]
    [SerializeField] private int healthIncrease;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // increase player's max health
            GameSystem.Instance.playerMaxHealth += healthIncrease;
            GameSystem.Instance.playerCurrentHealth = GameSystem.Instance.playerMaxHealth;

            GameSystem.Instance.healthUpsCollected++;

            Destroy(gameObject);
        }
    }
}
