using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

This script generates a 2D world for the player given a map structure generated
from MapGen. It random tile layouts for rooms from StreamingAssets/Data/Room and fits
them into the map structure to create unique worlds.

*/

public class WorldGen : MonoBehaviour
{
    [SerializeField] private CreateMap createMap;
    [SerializeField] private CreateTerrain createTerrain;

    public void GenerateWorld()
    {
        // Step #1: Fill in the minimap to help guide the camera
        createMap.SetupMap();

        // Step #2: Create tilemaps for each room and build out the game world
        createTerrain.SetupTerrain();
    }
}
