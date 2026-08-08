using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyUp : Item
{
    [Header("Upgrade Settings")]
    [SerializeField] private int energyIncrease;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // increase player's max energy
            GameSystem.Instance.playerMaxEnergy += energyIncrease;
            GameSystem.Instance.playerCurrentEnergy = GameSystem.Instance.playerMaxEnergy;

            GameSystem.Instance.energyUpsCollected++;

            Destroy(gameObject);
        }
    }
}
