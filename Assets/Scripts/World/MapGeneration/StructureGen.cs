using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StructureGen
{
    private static List<List<int>> structureGrid;
    private static int attempt;

    public static List<List<int>> CreateStructures()
    {
        structureGrid = MapGenUtility.InitializeGrid();
        attempt = 1;

        // use a randomly picked algorithm to generate unique structures in each region
        for (int region = 1; region <= 4; region++)
        {
            HashSet<Vector2Int> regionTiles = MapGenUtility.GetTiles(MapGen.RegionGrid, region);
            Vector2Int seed;

            if (region == 1)
            {
                seed = MapGenUtility.GetStartLocation();
                structureGrid[seed.y][seed.x] = 5;                
            }
            else
            {
                seed = regionTiles.ElementAt(UnityEngine.Random.Range(0, regionTiles.Count));
            }

            // go through random order in list of algorithms until one works
            bool success = false;
            int structureAttempt = 0;
            while (!success)
            {
                int randomAlgorithm = UnityEngine.Random.Range(0, 2);

                switch (randomAlgorithm)
                {
                    case 0:
                        structureGrid = RandomWalk.RandomWalkGenerate(structureGrid, region, seed);
                        break;
                    case 1:
                        structureGrid = Dfs.DfsGenerate(structureGrid, region, seed);
                        break;
                    default:
                        break;
                }

                // if structures touch regions in the correct way, count as a success and move on
                if (StructuresTouchRegions(region))
                {
                    success = true;
                }
                else
                {
                    structureAttempt++;
                    if (structureAttempt > 50)
                    {
                        Debug.LogWarning("Region " + region + " structure generation failed after 50 attempts - using current structure anyway");
                        success = true;
                    }
                    else
                    {
                        Debug.Log("Failed StructureGen Attempt #" + attempt + " - Region " + region + " structure not touching required adjacent Regions");
                        attempt++;
                        ClearStructureTiles(region);
                    }
                }
            
            }
        }

        // similar to regions, if structures aren't adjacent in the correct way, try again
        if (!MapGenUtility.EnsureProgression(structureGrid))
        {
            attempt++;
            if (attempt > 100)
            {
                Debug.LogError("Structure generation failed after 100 attempts - giving up");
                return structureGrid;
            }
            
            Debug.Log("Failed StructureGen Attempt #" + attempt + " - Structures do not touch properly");
            return CreateStructures();
        }



        return structureGrid;
    }

    /*
        Helper functions
    */

    private static void ClearStructureTiles(int region)
    {
        HashSet<Vector2Int> tiles = MapGenUtility.GetTiles(structureGrid, region);

        foreach (Vector2Int tile in tiles)
        {
            structureGrid[tile.y][tile.x] = 0;
        }
    }

    private static bool IsStructureAdjacentToRegion(int structureRegion, int region)
    {
        // check all tiles of a region against a target region
        HashSet<Vector2Int> structureTiles = MapGenUtility.GetTiles(structureGrid, structureRegion);

        Vector2Int[] directions =
        {
            new Vector2Int(0, -1), // up
            new Vector2Int(0, 1),  // down
            new Vector2Int(-1, 0), // left
            new Vector2Int(1, 0),  // right
        };

        foreach (Vector2Int tile in structureTiles)
        {
            // check all 4 neighbors of this tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = tile + dir;

                if (MapGenUtility.InBounds(neighbor))
                {
                    // if the structure tile is adjacent to the region tile at a single point, return true
                    if (MapGen.RegionGrid[neighbor.y][neighbor.x] == region)
                        return true;
                }
            }
        }

        // if no tiles touch, structure is not adjacent to the region
        return false;
    }

    private static bool StructuresTouchRegions(int currentRegion)
    {
        bool structure1TouchesRegion2 = IsStructureAdjacentToRegion(1, 2);
        bool structure2TouchesRegion1 = IsStructureAdjacentToRegion(2, 1);
        bool structure3TouchesRegion1Or2 = IsStructureAdjacentToRegion(3, 1) || IsStructureAdjacentToRegion(3, 2);
        bool structure4TouchesRegion1Or2Or3 = IsStructureAdjacentToRegion(4, 1) || IsStructureAdjacentToRegion(4, 2) || IsStructureAdjacentToRegion(4, 3);

        return (
            currentRegion == 1 && structure1TouchesRegion2 ||
            currentRegion == 2 && structure2TouchesRegion1 ||
            currentRegion == 3 && structure3TouchesRegion1Or2 ||
            currentRegion == 4 && structure4TouchesRegion1Or2Or3
        );
    }
}
