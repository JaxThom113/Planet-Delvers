using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCell : Item
{
    [Header("Item Settings")]
    [SerializeField] private int addEnergyAmount;

     private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // increase player's max health
            if (GameSystem.Instance.playerCurrentEnergy < GameSystem.Instance.playerMaxEnergy) 
                GameSystem.Instance.playerCurrentEnergy += 1;
            
            Destroy(gameObject);
        }
    }
}
