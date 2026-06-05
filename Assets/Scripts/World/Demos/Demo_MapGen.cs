using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Demo_MapGen : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 1)]
    [SerializeField] private float delay;
    [SerializeField] private bool seededRun;
    [SerializeField] private int seed;
    [SerializeField] private Button runButton;

    [Header("Step 1: Region Generation")]
    [SerializeField] private Tilemap regionTilemap;
    [SerializeField] private Tile[] regionTiles;

    [Header("Step 2: Structure Generation")]
    [SerializeField] private Tilemap structureTilemap;
    [SerializeField] private Tile[] structureTiles;

    [Header("Step 3: Room Generation")]
    [SerializeField] private Tilemap roomsTilemap;
    [SerializeField] private Tile[] r1Tiles;
    [SerializeField] private Tile[] r2Tiles;
    [SerializeField] private Tile[] r3Tiles;
    [SerializeField] private Tile[] r4Tiles;
    [SerializeField] private Tilemap doorsTilemap;
    [SerializeField] private Tile[] doorTiles;

    // grid size must be defined here because MapGen isn't run in the demo scene
    private const int GRID_SIZE = 16;
    private int regionGenAttempts;
    private int structureGenAttempts;
    private Vector2Int startingRoom;

    private struct Quadrant
    {
        public int xMin, xMax, yMin, yMax;

        public Quadrant(int xMin, int xMax, int yMin, int yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        // get a random point within a quadrant
        public Vector2Int GetRandomPoint()
        {
            return new Vector2Int(
                UnityEngine.Random.Range(xMin, xMax),
                UnityEngine.Random.Range(yMin, yMax)
            );
        }
    }

    private readonly Vector2Int[] directions =
    {
        new Vector2Int(0, 1),  // up
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    // length x height
    private readonly Vector2Int[] roomSizes =
    {
        new(1,1), new(1,2), new(1,3), new(1,4), new(1,5),
        new(2,1), new(2,2), new(2,3), new(2,4), new(2,5),
        new(3,1), new(3,2), new(3,3), new(3,4), new(3,5),
        new(4,1), new(4,2), new(4,3), new(4,4), new(4,5),
        new(5,1), new(5,2), new(5,3), new(5,4), new(5,5),
    };


    public void OnRunClicked()
    {
        runButton.interactable = false;

        regionTilemap.ClearAllTiles();
        structureTilemap.ClearAllTiles();
        roomsTilemap.ClearAllTiles();
        doorsTilemap.ClearAllTiles();

        regionGenAttempts = 1;
        structureGenAttempts = 1;

        // make sure to save the random seed number for the player's reference
        if (!seededRun)
            seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed); // set the seed

        StartCoroutine(GenerateMap());
    }

    private IEnumerator GenerateMap()
    {
        yield return StartCoroutine(FloodFillRegions());
        yield return StartCoroutine(CreateStructures());
        yield return StartCoroutine(CreateRooms());
        
        Debug.Log("Generation Complete!");
        runButton.interactable = true;
    }

    private IEnumerator FloodFillRegions()
    {
        // frontiers for each region
        Dictionary<int, Queue<Vector2Int>> frontiers = new Dictionary<int, Queue<Vector2Int>>();
        for (int i = 1; i <= 4; i++)
        {
            frontiers[i] = new Queue<Vector2Int>();
        }

        // initial seeds where regions will "grow" from, make sure to be in different quadrants of the grid
        Vector2Int[] seeds = GenerateSeeds();

        for (int i = 0; i < seeds.Length; i++)
        {
            // set starting locations for regions, add the region number there
            int region = i + 1;

            Vector2Int pos = seeds[i];
            if (!InBounds(pos))
                continue;

            regionTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), regionTiles[region]);
            frontiers[region].Enqueue(pos);
        }
       
        // flood fill expansion
        bool expanded = true;
        while (expanded)
        {
            yield return new WaitForSeconds(delay);

            expanded = false;

            // add to each region in a random order every iteration
            List<int> regionOrder = new List<int>(){ 1, 2, 3, 4 };
            Shuffle(regionOrder);

            foreach (int region in regionOrder)
            {
                // process all frontier items for this region
                int frontierCount = frontiers[region].Count;

                for (int i = 0; i < frontierCount; i++)
                {
                    Vector2Int current = frontiers[region].Dequeue();

                    // check each direction out from the previously added region location
                    List<Vector2Int> shuffledDirs = new List<Vector2Int>(directions);
                    Shuffle(shuffledDirs);

                    foreach (Vector2Int dir in shuffledDirs)
                    {
                        Vector2Int next = current + dir;
                        if (!InBounds(next))
                            continue;

                        // already filled
                        if (regionTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) != null)
                            continue;

                        // add to the region in the grid, queue this position to frontier
                        regionTilemap.SetTile(new Vector3Int(next.x, next.y, 0), regionTiles[region]);
                        frontiers[region].Enqueue(next);

                        expanded = true;
                    }
                }
            }
        }

        // set start room location somewhere in region 1
        List<Vector2Int> region1Tiles = GetTiles(regionTilemap, regionTiles, 1);
        if (region1Tiles.Count > 0)
        {
            startingRoom = region1Tiles[UnityEngine.Random.Range(0, region1Tiles.Count)];
            regionTilemap.SetTile(new Vector3Int(startingRoom.x, startingRoom.y, 0), regionTiles[5]);
        }

        // if regions aren't adjacent in the correct way, try again
        if (!EnsureProgression(regionTilemap, regionTiles))
        {
            Debug.Log("Failed RegionGen Attempt #" + regionGenAttempts + " - Regions do not touch");
            regionGenAttempts++;
            regionTilemap.ClearAllTiles();
            yield return StartCoroutine(FloodFillRegions());
        }
    }

    private IEnumerator CreateStructures()
    {
        // remember to add starting room location
        structureTilemap.SetTile(new Vector3Int(startingRoom.x, startingRoom.y, 0), structureTiles[5]);

        // make a list of alrgorithms to pick from
        List<Func<List<Vector2Int>, int, Vector2Int, IEnumerator>> algorithms = new List<Func<List<Vector2Int>, int, Vector2Int, IEnumerator>>()
        {
            (regionTiles, region, seed) => RandomWalkGenerate(regionTiles, region, seed),
            (regionTiles, region, seed) => DfsGenerate(regionTiles, region, seed),
        };

        // use a randomly picked algorithm to generate unique structures in each region
        for (int region = 1; region <= 4; region++)
        {
            List<Vector2Int> targetRegionTiles = GetTiles(regionTilemap, regionTiles, region);
            Vector2Int seed = targetRegionTiles[UnityEngine.Random.Range(0, targetRegionTiles.Count)];

            // Progression rules:
            // 1 must be touching 2
            // 2 must be touching 1
            // 3 must be touching 1 or 2
            // 4 must be touching 1 or 2 or 3

            // go through random order in list of algorithms until one works
            bool success = false;
            while (!success)
            {
                Shuffle(algorithms);

                foreach (var algorithm in algorithms)
                {
                    if (region == 1)
                        yield return StartCoroutine(algorithm(targetRegionTiles, region, startingRoom));
                    else
                        yield return StartCoroutine(algorithm(targetRegionTiles, region, seed));

                    bool structure1TouchesRegion2 = IsStructureAdjacentToRegion(1, 2);
                    bool structure2TouchesRegion1 = IsStructureAdjacentToRegion(2, 1);
                    bool structure3TouchesRegion1Or2 = IsStructureAdjacentToRegion(3, 1) || IsStructureAdjacentToRegion(3, 2);
                    bool structure4TouchesAnyRegion = IsStructureAdjacentToRegion(4, 1) || IsStructureAdjacentToRegion(4, 2) || IsStructureAdjacentToRegion(4, 3);

                    // if the conditions for a certain regions succeed, count as success and move on to next region
                    if (region == 1 && structure1TouchesRegion2 ||
                        region == 2 && structure2TouchesRegion1 ||
                        region == 3 && structure3TouchesRegion1Or2 ||
                        region == 4 && structure4TouchesAnyRegion)
                    {
                        success = true;
                        break;
                    }
                    else
                    {
                        Debug.Log("Failed StructureGen Attempt #" + structureGenAttempts + " - Region " + region + " structure not touching required adjacent Regions");
                        structureGenAttempts++;
                        ClearStructureTiles(region);
                    }
                }
            }
        }

        // similar to regions, if structures aren't adjacent in the correct way, try again
        if (!EnsureProgression(structureTilemap, structureTiles))
        {
            Debug.Log("Failed StructureGen Attempt #" + structureGenAttempts + " - Structures do not touch properly");
            structureGenAttempts++;
            structureTilemap.ClearAllTiles();
            yield return StartCoroutine(CreateStructures());
        }
    }

    private IEnumerator CreateRooms()
    {
        roomsTilemap.SetTile(new Vector3Int(startingRoom.x, startingRoom.y, 0), regionTiles[5]);

        for (int region = 1; region <= 4; region++)
        {
            List<Vector2Int> targetStructureTiles = GetTiles(structureTilemap, structureTiles, region);

            Tile[] currentTileSet = r1Tiles;
            switch (region)
            {
                case 1: currentTileSet = r1Tiles; break;
                case 2: currentTileSet = r2Tiles; break;
                case 3: currentTileSet = r3Tiles; break;
                case 4: currentTileSet = r4Tiles; break;
            }
            
            foreach (Vector2Int tile in targetStructureTiles)
            {
                // skip tiles already occupied by a room
                if (roomsTilemap.GetTile(new Vector3Int(tile.x, tile.y, 0)) != null)
                    continue;

                Vector2Int current = new Vector2Int(tile.x, tile.y);

                List<Vector2Int> shuffledRooms = new List<Vector2Int>(roomSizes);
                Shuffle(shuffledRooms);

                foreach (Vector2Int room in shuffledRooms)
                {
                    bool roomFits = true;
                    List<Vector2Int> currentRoomTiles = new List<Vector2Int>();

                    // for the currently selected room size, check if all tiles are part of the structure tilemap
                    for (int dx = 0; dx < room.x; dx++)
                    {
                        for (int dy = 0; dy < room.y; dy++)
                        {
                            Vector2Int loc = new Vector2Int(current.x + dx, current.y + dy);

                            if (!targetStructureTiles.Contains(loc))
                            {
                                // if any one of the tiles is not in the current region's structure tiles, this room size can't be placed
                                roomFits = false;
                                break;
                            }
                            else if (roomsTilemap.GetTile(new Vector3Int(loc.x, loc.y, 0)) != null)
                            {
                                // if any one of the tiles is over top a tile of an existing room, this room size can't be placed
                                roomFits = false;
                                break;
                            }
                            else
                            {
                                currentRoomTiles.Add(new Vector2Int(loc.x, loc.y));
                            }
                        }

                        if (!roomFits)
                            break;
                    }

                    if (!roomFits)
                        continue;

                    // if the room fits withn structure and region maps, add it
                    for (int dx = 0; dx < room.x; dx++)
                    {
                        for (int dy = 0; dy < room.y; dy++)
                        {
                            Vector2Int loc = new Vector2Int(current.x + dx, current.y + dy);

                            // { up, down, left, right }
                            bool[] neighbors = new bool[4] { false, false, false, false };

                            // goes through each direction in order (up->down->left->right) and marks true if a neighbor is there
                            for (int i = 0; i < directions.Length; i++)
                            {
                                Vector2Int neighbor = loc + directions[i];
                                if (currentRoomTiles.Contains(neighbor))
                                    neighbors[i] = true;
                            }

                            // use bitmask to get right tiles for rooms
                            int mask = 0;
                            if (neighbors[0]) mask |= 1; // up
                            if (neighbors[1]) mask |= 2; // down
                            if (neighbors[2]) mask |= 4; // left
                            if (neighbors[3]) mask |= 8; // right

                            switch (mask)
                            {
                                case 0: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[0]); break;   // ---- single room
                                case 1: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[12]); break;  // U--- vertical bottom
                                case 2: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[10]); break;  // -D-- vertical top
                                case 3: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[11]); break;  // UD-- vertical middle
                                case 4: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[15]); break;  // --L-, horizontal right
                                case 5: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[9]); break;   // U-L- bottom right
                                case 6: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[3]); break;   // -DL- top right
                                case 7: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[6]); break;   // UDL- middle right
                                case 8: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[13]); break;  // ---R, horizontal left
                                case 9: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[7]); break;   // U--R, bottom left
                                case 10: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[1]); break;  // -D-R, top left
                                case 11: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[4]); break;  // UD-R, middle left
                                case 12: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[14]); break; // --LR, horizontal middle
                                case 13: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[8]); break;  // U-LR, bottom middle
                                case 14: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[2]); break;  // -DLR, top middle
                                case 15: roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[5]); break;  // UDLR, middle center
                            }
                        }
                    }

                    yield return new WaitForSeconds(delay);

                    // if the process gets to this point, a room has been added, so skip over remaining shuffledRooms
                    break;
                }
            }
        }
    }

    private IEnumerator PlaceDoors()
    {
        yield return null;
    }

    private IEnumerator DfsGenerate(List<Vector2Int> regionTiles, int region, Vector2Int pos)
    {
        yield return new WaitForSeconds(delay);

        Vector2Int current = new Vector2Int(pos.x, pos.y);

        Vector2Int[] doubledDirections =
        {
            new Vector2Int(0, 2),  // up
            new Vector2Int(0, -2), // down
            new Vector2Int(-2, 0), // left
            new Vector2Int(2, 0),  // right
        };

        // randomize directions
        List<Vector2Int> dirs = new List<Vector2Int>(doubledDirections);
        Shuffle(dirs);

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int next = current + dir;

            // check bounds
            if (InBounds(next) && regionTiles.Contains(next))
            {
                if (structureTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) == null) // unvisited
                {
                    // carve path between current and neighbor
                    structureTilemap.SetTile(new Vector3Int(pos.x + dir.x / 2, pos.y + dir.y / 2, 0), structureTiles[region]);
                    structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);

                    // yield return needs to be here because this is a recursive function
                    yield return StartCoroutine(DfsGenerate(regionTiles, region, new Vector2Int(next.x, next.y)));
                }
            }
        }
    }

    private IEnumerator RandomWalkGenerate(List<Vector2Int> regionTiles, int region, Vector2Int seed)
    {
        // random walk could fill a 4th of the region at the low end, full region at high end
        int pathLength = regionTiles.Count;
        // int pathLength = UnityEngine.Random.Range(regionTiles.Count / 2, regionTiles.Count);

        Vector2Int current = new Vector2Int(seed.x, seed.y);
        List<Vector2Int> dirs = new List<Vector2Int>(directions);

        for (int i = 0; i < pathLength; i++)
        {
            yield return new WaitForSeconds(delay);

            Shuffle(dirs);

            Vector2Int next = current;

            foreach (Vector2Int dir in dirs)
            {
                // try all 4 directions, take the first one that works (they are randomized by Shuffle)
                next = current + dir;

                if (InBounds(next) && regionTiles.Contains(next))
                {
                    // tiles that have already been visited can be visited again
                    structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);
                    break;
                }
            }

            current = next;
        }
    }

    private bool InBounds(Vector2Int pos)
    {
        bool inBounds = (
            pos.x >= 0 && 
            pos.x < GRID_SIZE && 
            pos.y >= 0 &&
            pos.y < GRID_SIZE
        );

        return inBounds;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private Vector2Int[] GenerateSeeds()
    {
        // define all 4 quadrants
        List<Quadrant> quadrants = new List<Quadrant>
        {
            new Quadrant(GRID_SIZE/2, GRID_SIZE, GRID_SIZE/2, GRID_SIZE), // Q1 (top right)
            new Quadrant(0, GRID_SIZE/2, GRID_SIZE/2, GRID_SIZE),         // Q2 (top left)
            new Quadrant(0, GRID_SIZE/2, 0, GRID_SIZE/2),                 // Q3 (bottom left)
            new Quadrant(GRID_SIZE/2, GRID_SIZE, 0, GRID_SIZE/2),         // Q4 (bottom right)
        };

        // starting quadrant for seed 1
        int startQuadrant = UnityEngine.Random.Range(0, 4);

        // adjacent quadrants for seeds 2/3
        int adjacent1 = (startQuadrant + 1) % 4;
        int adjacent2 = (startQuadrant + 3) % 4;

        // opposite quadrant for seed 4
        int opposite = (startQuadrant + 2) % 4;

        Vector2Int[] seeds = new Vector2Int[4];

        seeds[0] = quadrants[startQuadrant].GetRandomPoint();

        // randomize which adjacent quadrant gets seed 2 or 3
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            seeds[1] = quadrants[adjacent1].GetRandomPoint();
            seeds[2] = quadrants[adjacent2].GetRandomPoint();
        }
        else
        {
            seeds[1] = quadrants[adjacent2].GetRandomPoint();
            seeds[2] = quadrants[adjacent1].GetRandomPoint();
        }

        seeds[3] = quadrants[opposite].GetRandomPoint();

        return seeds;
    }

    private List<Vector2Int> GetTiles(Tilemap map, Tile[] tiles, int region)
    {
        // add all tile of region to a list and return
        List<Vector2Int> foundTiles = new List<Vector2Int>();
        Tile targetTile = tiles[region];

        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                if (map.GetTile(new Vector3Int(x, y, 0)) == targetTile)
                    foundTiles.Add(new Vector2Int(x, y));
            }
        }

        return foundTiles;
    }

    private bool EnsureProgression(Tilemap map, Tile[] tiles)
    {
        bool region2TouchesRegion1 = AreTilesAdjacent(map, tiles, 1, 2);
        bool region3TouchesRegion1Or2 = AreTilesAdjacent(map, tiles, 3, 1) || AreTilesAdjacent(map, tiles, 3, 2);
        bool region4TouchesAnyRegion = AreTilesAdjacent(map, tiles, 4, 1) || AreTilesAdjacent(map, tiles, 4, 2) || AreTilesAdjacent(map, tiles, 4, 3);
        
        return region2TouchesRegion1 && region3TouchesRegion1Or2 && region4TouchesAnyRegion;
    }

    private bool AreTilesAdjacent(Tilemap map, Tile[] tiles, int regionA, int regionB)
    {
        // check all tiles of a region against a target region
        List<Vector2Int> tilesA = GetTiles(map, tiles, regionA);
        Tile targetTileB = tiles[regionB];

        foreach (Vector2Int tileA in tilesA)
        {
            // check all 4 neighbors of this tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = tileA + dir;

                if (!InBounds(neighbor))
                    continue;

                // if the regions touch at a single point,, return true
                if (map.GetTile(new Vector3Int(neighbor.x, neighbor.y, 0)) == targetTileB)
                    return true;
            }
        }

        // if no tiles touch, regions are not adjacent
        return false;
    }

    private bool IsStructureAdjacentToRegion(int structureRegion, int region)
    {
        // check all tiles of a structure region against a target region
        List<Vector2Int> structureRegionTiles = GetTiles(structureTilemap, structureTiles, structureRegion);
        Tile targetRegionTile = regionTiles[region];

        foreach (Vector2Int tile in structureRegionTiles)
        {
            // check all 4 neighbors of this tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = tile + dir;

                if (!InBounds(neighbor))
                    continue;

                // if the structure tile is adjacent to the region tile, return true
                if (regionTilemap.GetTile(new Vector3Int(neighbor.x, neighbor.y, 0)) == targetRegionTile)
                    return true;
            }
        }

        // if no tiles touch, structure is not adjacent to the region
        return false;
    }

    private void ClearStructureTiles(int region)
    {
        List<Vector2Int> tiles = GetTiles(structureTilemap, structureTiles, region);

        foreach (Vector2Int tile in tiles)
        {
            structureTilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), null);
        }
    }
}
