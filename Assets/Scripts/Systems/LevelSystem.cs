using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : Singleton<LevelSystem>
{
    [SerializeField] private bool seededRun;
    [SerializeField] private int seed;

    void Start()
    {
        // make sure to save the random seed number for the player's reference
        if (!seededRun)
            seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed); // set the seed
    }
}
