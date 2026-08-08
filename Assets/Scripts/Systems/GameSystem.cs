using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : PersistentSingleton<GameSystem>
{
    [Header("Mission Data")]
	public int seed = 0;
	public int size = 1;
	public int level = 1;

    [Header("Player Data")]
    public int playerMaxHealth = 100;
    public int playerCurrentHealth = 100;
    public int playerMaxEnergy = 5;
    public int playerCurrentEnergy = 5;

    [Header("Game Data")]
    public int jumps = 0;
    public int shotsFired = 0;
    public int damageTaken = 0;
    public int enemiesDefeated = 0;
    public int bossesDefeated = 0;
    public int healthUpsCollected = 0;
    public int energyUpsCollected = 0;

    public void InitializeData()
    {
        // reset mission data
        seed = 0;
        size = 1;  // default to normal size
        level = 1; // default to medium level

        // reset player data
        playerMaxHealth = 100;
        playerCurrentHealth = 100;
        playerMaxHealth = 5;
        playerCurrentEnergy = 5;

        // reset game data
        jumps = 0;
        shotsFired = 0;
        damageTaken = 0;
        enemiesDefeated = 0;
        bossesDefeated = 0;
        healthUpsCollected = 0;
        energyUpsCollected = 0;
    }
}
