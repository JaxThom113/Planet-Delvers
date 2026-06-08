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
    private const bool REVISIT_TILES = false;

    private int regionGenAttempts;
    private int structureGenAttempts;
    private Vector2Int startingRoom;

    private List<MapTile> allMapTiles;
    private List<Room> allRooms;

    private struct Room
    {
        public MapTile[] roomTiles;
        public int doorCount;
        
        public Room(MapTile[] roomTiles, int doorCount)
        {
            this.roomTiles = roomTiles;
            this.doorCount = doorCount;
        }

        // get a random MapTile within a room
        public MapTile GetRandomTile()
        {
            return roomTiles[UnityEngine.Random.Range(0, roomTiles.Length)];
        }

        public bool ContainsMapTile(Vector2Int position)
        {
            foreach (MapTile tile in roomTiles)
            {
                if (tile.position == position)
                    return true;
            }

            return false;
        }
    }

    private struct MapTile
    {
        public Room room;
        public Vector2Int position;
        public int region;
        public bool visited;
        public bool[] doors; // { up, down, left, right }
        public bool[] connections; // { up, down, left, right }

        public MapTile(Room room, Vector2Int position, int region, bool visited, bool[] doors, bool[] connections)
        {
            this.room = room;
            this.position = position;
            this.region = region;
            this.visited = visited;
            this.doors = doors;
            this.connections = connections;
        }
    }

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
        yield return StartCoroutine(PlaceDoors());
        
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
        // initialize MapTiles and Rooms lists
        allMapTiles = new List<MapTile>();
        allRooms = new List<Room>();

        // set the starting room tile so you can see it
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

                    // length is the size of the room about to be added
                    Room newRoom = new Room();
                    MapTile[] roomTilesToAdd = new MapTile[room.x * room.y];

                    // if the room fits within structure and region maps, add it
                    for (int dx = 0; dx < room.x; dx++)
                    {
                        for (int dy = 0; dy < room.y; dy++)
                        {
                            Vector2Int loc = new Vector2Int(current.x + dx, current.y + dy);

                            // set up a new MapTile
                            MapTile newMapTile = new MapTile(
                                newRoom, 
                                loc, 
                                region, 
                                false, 
                                new bool[4]{ false, false, false, false }, 
                                new bool[4]{ false, false, false, false }
                            );

                            // { up, down, left, right }
                            bool[] neighbors = new bool[4] { false, false, false, false };

                            // goes through each direction in order (up->down->left->right) and marks true if a neighbor is there
                            for (int i = 0; i < directions.Length; i++)
                            {
                                Vector2Int neighbor = loc + directions[i];
                                if (currentRoomTiles.Contains(neighbor))
                                    neighbors[i] = true;
                            }

                            // use bitmask to get right tiles and connections for rooms
                            int mask = 0;
                            if (neighbors[0]) mask |= 1; // up
                            if (neighbors[1]) mask |= 2; // down
                            if (neighbors[2]) mask |= 4; // left
                            if (neighbors[3]) mask |= 8; // right

                            switch (mask)
                            {
                                case 0: // ---- single room
                                    newMapTile.connections = new bool[4] { false, false, false, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[0]); 
                                    break;   
                                case 1: // U--- vertical bottom
                                    newMapTile.connections = new bool[4] { true, false, false, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[12]); 
                                    break;  
                                case 2: // -D-- vertical top
                                    newMapTile.connections = new bool[4] { false, true, false, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[10]); 
                                    break;  
                                case 3: // UD-- vertical middle
                                    newMapTile.connections = new bool[4] { true, true, false, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[11]); 
                                    break;  
                                case 4: // --L-, horizontal right
                                    newMapTile.connections = new bool[4] { false, false, true, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[15]); 
                                    break;  
                                case 5: // U-L- bottom right
                                    newMapTile.connections = new bool[4] { true, false, true, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[9]); 
                                    break;   
                                case 6: // -DL- top right
                                    newMapTile.connections = new bool[4] { false, true, true, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[3]); 
                                    break;   
                                case 7: // UDL- middle right
                                    newMapTile.connections = new bool[4] { true, true, true, false };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[6]); 
                                    break;   
                                case 8: // ---R, horizontal left
                                    newMapTile.connections = new bool[4] { false, false, false, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[13]); 
                                    break;  
                                case 9: // U--R, bottom left
                                    newMapTile.connections = new bool[4] { true, false, false, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[7]); 
                                    break;   
                                case 10: // -D-R, top left
                                    newMapTile.connections = new bool[4] { false, true, false, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[1]); 
                                    break;  
                                case 11: // UD-R, middle left
                                    newMapTile.connections = new bool[4] { true, true, false, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[4]); 
                                    break;  
                                case 12: // --LR, horizontal middle
                                    newMapTile.connections = new bool[4] { false, false, true, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[14]); 
                                    break; 
                                case 13: // U-LR, bottom middle
                                    newMapTile.connections = new bool[4] { true, false, true, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[8]); 
                                    break;  
                                case 14: // -DLR, top middle
                                    newMapTile.connections = new bool[4] { false, true, true, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[2]); 
                                    break;  
                                case 15: // UDLR, middle center
                                    newMapTile.connections = new bool[4] { true, true, true, true };
                                    roomsTilemap.SetTile(new Vector3Int(loc.x, loc.y, 0), currentTileSet[5]); 
                                    break;  
                            }

                            // add the new MapTile to its Room, and the new MapTile to the larger list of all MapTiles
                            // use dy * room.x + dx as index so tiles aren't overwritten
                            roomTilesToAdd[dy * room.x + dx] = newMapTile;
                            allMapTiles.Add(newMapTile);
                        }
                    }

                    // add the MapTiles to the new Room, and add the new Room to the larger list of all Rooms
                    newRoom.roomTiles = roomTilesToAdd;
                    allRooms.Add(newRoom);

                    yield return new WaitForSeconds(delay);

                    // if the process gets to this point, a room has been added, so skip over remaining shuffledRooms
                    break;
                }
            }
        }
    }

    private IEnumerator PlaceDoors()
    {
        /*

        Minimum checks:

        - there needs to be at least 1 door in between each region
            - 2->1, 3->1 or 3->2, 4->1 or 4->2 or 4->3
        - I believe every room needs to have at least 1 door so every room can be accessible



        Get all the rooms of a region, shuffle

        - add one door to each room at a time
        - keep adding / repeat until all rooms in a region are accessible
        - once every room has a least 1 door, every room should be accessible
            - repeating the loop makes it possible for some rooms to have more than 1 door
            - remember, adding a door to one room adds it to another room as well

        - then add doors between regions

        */ 

        // add doors to connect rooms within a region
        for (int region = 1; region <= 4; region++)
        {
            List<Room> rooms = GetRooms(region);
            Shuffle(rooms);

            foreach (Room room in rooms)
            {
                MapTile current = new MapTile();
                List<int> validNeighbors = new List<int>();

                // keep rerolling the random tile in a room until you have one with neighbors
                while (validNeighbors.Count == 0)
                {
                    current = room.GetRandomTile();

                    // { up, down, left, right }
                    bool[] neighbors = new bool[4] { false, false, false, false };
                    
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int neighbor = current.position + directions[i];
                        int neighborIndex = GetMapTileIndexAtPosition(neighbor);

                        if (!InBounds(neighbor))
                            continue;

                        if (neighborIndex != -1 && !room.ContainsMapTile(neighbor))
                            neighbors[i] = true;
                    }

                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (neighbors[i])
                            validNeighbors.Add(i);
                    }
                }

                // pick a random neighbor tile to carve a door to, 0 = up, 1 = down, 2 = left, 3 = right
                int selectedNeighbor = validNeighbors[UnityEngine.Random.Range(0, validNeighbors.Count)];
                current.doors[selectedNeighbor] = true;

                // this also means you need to get that neighbor MapTile and place a door in the opposite direction
                allMapTiles[GetMapTileIndexAtPosition(current.position)].doors[selectedNeighbor] = true;

                int opposite = selectedNeighbor;
                switch (selectedNeighbor)
                {
                    case 0: opposite = 1; break; // opposite of up is down
                    case 1: opposite = 0; break; // opposite of down is up
                    case 2: opposite = 3; break; // opposite of left is right
                    case 3: opposite = 2; break; // opposite of right is left
                }

                allMapTiles[GetMapTileIndexAtPosition(current.position + directions[selectedNeighbor])].doors[opposite] = true;
            }
        }

        // add doors to connect regions
        List<int[]> region2ConnectsRegion1 = FindAdjacentIndexesOfMapTiles(2, 1);
        foreach (int[] doorConnection in region2ConnectsRegion1)
        {
            allMapTiles[doorConnection[0]].doors[doorConnection[2]] = true;
            allMapTiles[doorConnection[1]].doors[doorConnection[3]] = true;
        }

        List<int[]> region3ConnectsRegion1Or2 = FindAdjacentIndexesOfMapTiles(3, 1);
        region3ConnectsRegion1Or2.AddRange(FindAdjacentIndexesOfMapTiles(3, 2));
        foreach (int[] doorConnection in region3ConnectsRegion1Or2)
        {
            allMapTiles[doorConnection[0]].doors[doorConnection[2]] = true;
            allMapTiles[doorConnection[1]].doors[doorConnection[3]] = true;
        }

        List<int[]> region4ConnectsRegion1Or2Or3 = FindAdjacentIndexesOfMapTiles(4, 1);
        region4ConnectsRegion1Or2Or3.AddRange(FindAdjacentIndexesOfMapTiles(4, 2));
        region4ConnectsRegion1Or2Or3.AddRange(FindAdjacentIndexesOfMapTiles(4, 3));
        foreach (int[] doorConnection in region4ConnectsRegion1Or2Or3)
        {
            allMapTiles[doorConnection[0]].doors[doorConnection[2]] = true;
            allMapTiles[doorConnection[1]].doors[doorConnection[3]] = true;
        }

        // draw the door tiles
        foreach (MapTile mapTile in allMapTiles)
        {
            // use bitmask to get right tiles and connections for rooms
            int mask = 0;
            if (mapTile.doors[0]) mask |= 1; // up
            if (mapTile.doors[1]) mask |= 2; // down
            if (mapTile.doors[2]) mask |= 4; // left
            if (mapTile.doors[3]) mask |= 8; // right

            switch (mask)
            {
                case 0: // ---- no doors
                    break;   
                case 1: // U--- up door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[0]); 
                    break;  
                case 2: // -D-- down door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[1]); 
                    break;  
                case 3: // UD-- up, down door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[12]); 
                    break;  
                case 4: // --L-, left door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[2]); 
                    break;  
                case 5: // U-L- up, left door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[4]); 
                    break;   
                case 6: // -DL- down, left door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[7]); 
                    break;   
                case 7: // UDL- up, down, left door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[11]); 
                    break;   
                case 8: // ---R, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[3]); 
                    break;  
                case 9: // U--R, up, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[5]); 
                    break;   
                case 10: // -D-R, down, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[6]); 
                    break;  
                case 11: // UD-R, up, down, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[9]); 
                    break;  
                case 12: // --LR, left, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[13]); 
                    break; 
                case 13: // U-LR, up, left, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[8]); 
                    break;  
                case 14: // -DLR, down, left, right door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[10]); 
                    break;  
                case 15: // UDLR, all directions door
                    doorsTilemap.SetTile(new Vector3Int(mapTile.position.x, mapTile.position.y, 0), doorTiles[14]); 
                    break;  
            }
        }

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
        // int pathLength = regionTiles.Count;
        int pathLength = UnityEngine.Random.Range(regionTiles.Count / 2, regionTiles.Count);

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

                if (!InBounds(next))
                    continue;

                if (regionTiles.Contains(next))
                {
                    if (REVISIT_TILES)
                    {
                        // Can revisit tiles, so accept any valid tile (filled or empty)
                        structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);
                        break;
                    }
                    else 
                    {
                        // Cannot revisit, only accept empty tiles
                        if (structureTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) == null)
                        {
                            structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);
                            break;
                        }
                    }
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

    private List<Room> GetRooms(int region)
    {
        List<Room> targetRegionRooms = new List<Room>();

        foreach (Room room in allRooms)
        {
            if (room.roomTiles[0].region == region)
                targetRegionRooms.Add(room);
        }

        return targetRegionRooms;
    }

    private int GetMapTileIndexAtPosition(Vector2Int position)
    {
        for (int i = 0; i < allMapTiles.Count; i++)
        {
            if (allMapTiles[i].position == position)
                return i;
        }

        return -1;
    }

    private List<int[]> FindAdjacentIndexesOfMapTiles(int regionA, int regionB)
    {
        // the int[] contains the index for regionA tile, index of regionB tile, direction of door for A, direction of door for B
        List<int[]> indexes = new List<int[]>();

        for (int i = 0; i < allMapTiles.Count; i++)
        {
            if (allMapTiles[i].region == regionA)
            {
                for (int k = 0; k < directions.Length; k++)
                {
                    Vector2Int neighbor = allMapTiles[i].position + directions[k];
                    int neighborIndex = GetMapTileIndexAtPosition(neighbor);

                    // non-existant tile
                    if (neighborIndex == -1)
                        break;

                    if (allMapTiles[neighborIndex].region == regionB)
                    {
                        int opposite = k;
                        switch (k)
                        {
                            case 0: opposite = 1; break; // opposite of up is down
                            case 1: opposite = 0; break; // opposite of down is up
                            case 2: opposite = 3; break; // opposite of left is right
                            case 3: opposite = 2; break; // opposite of right is left
                        }

                        indexes.Add(new int[4]{ i, neighborIndex, k, opposite });
                    }
                }
            }
        }

        return indexes;
    }
}
