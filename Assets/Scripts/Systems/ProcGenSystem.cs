using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProcGenSystem : Singleton<ProcGenSystem>
{
    [SerializeField] private List<Tile> levelTiles;
    [SerializeField] private List<Tile> mapTiles;

    void Start()
    {
        MapGen.GenerateMap();
    }

    void Update()
    {
        
    }
}
