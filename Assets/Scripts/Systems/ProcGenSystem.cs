using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProcGenSystem : Singleton<ProcGenSystem>
{
    [SerializeField] private WorldGen worldGen;

    private readonly int[] gridSizes = { 16, 24, 32 };

    void Start()
    {   
        // set the seed
        UnityEngine.Random.InitState(GameSystem.Instance.seed); 
        if (GameSystem.Instance.seed == 0)
            UnityEngine.Random.InitState(Environment.TickCount); 

        // first generate the world map
        MapGen.GenerateMap(gridSizes[GameSystem.Instance.length]);

        // then using map data generate rooms for the player to explore
        worldGen.GenerateWorld();
    }
}
