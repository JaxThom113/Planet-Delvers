using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProcGenSystem : Singleton<ProcGenSystem>
{
    [SerializeField] private WorldGen worldGen;

    private readonly int[] gridSizes = { 8, 16, 24 };

    void Start()
    {   
        // set the seed
        UnityEngine.Random.InitState(GameSystem.Instance.seed); 
        if (GameSystem.Instance.seed == 0)
        {
            GameSystem.Instance.seed = Environment.TickCount;
            UnityEngine.Random.InitState(GameSystem.Instance.seed); 
        }

        // first generate the world map
        MapGen.GenerateMap(gridSizes[GameSystem.Instance.size]);

        // then using map data generate rooms for the player to explore
        worldGen.GenerateWorld();
    }
}
