using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProcGenSystem : Singleton<ProcGenSystem>
{
    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Seed")]
    [SerializeField] private bool seededRun;
    [SerializeField] private int seed;

    [Header("Map Generation")]
    [SerializeField] private Tilemap cameraTilemap; // hidden map used to guide the camera correctly
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tile bgTile;
    [SerializeField] private Tilemap roomsTilemap; // actual map that uses the visited feature for tiles to simulate fog of war  
    [SerializeField] private Tile[] r1Tiles;
    [SerializeField] private Tile[] r2Tiles;
    [SerializeField] private Tile[] r3Tiles;
    [SerializeField] private Tile[] r4Tiles;
    [SerializeField] private Tilemap doorsTilemap;
    [SerializeField] private Tile[] doorTiles;

    [Header("Room Generation")]
    [SerializeField] private Grid roomGrid;
    [SerializeField] private Tile[] roomTiles;
    [SerializeField] private Grid worldGrid;

    void Start()
    {   
        // make sure to save the random seed number for the player's reference
        if (!seededRun)
            seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed); // set the seed

        // first generate the world map
        Debug.Log("Starting map generate...");
        MapGen.GenerateMap();

        // then using map data generate rooms for the player to explore
        Debug.Log("Starting to build game world...");
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        // Step #1: Fill in the minimap to help guide the camera
        SetupMap();

        // Step #2: Create tilmaps for each room and build out the game world
        SetupWorld();
    }

    private void SetupMap()
    {
        // create black grid background according to the grid size
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), bgTile);
            }
        }

        // add room tiles to map
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                if (MapGen.MapGrid[y][x].region == 0)
                    continue;

                Tile[] currentTileSet = r1Tiles;
                switch (MapGen.MapGrid[y][x].region)
                {
                    case 1: currentTileSet = r1Tiles; break;
                    case 2: currentTileSet = r2Tiles; break;
                    case 3: currentTileSet = r3Tiles; break;
                    case 4: currentTileSet = r4Tiles; break;
                }

                // use bitmask to get right tiles to make connected rooms
                int connectionsMask = 0;
                if (MapGen.MapGrid[y][x].connections[0]) connectionsMask |= 1; // up
                if (MapGen.MapGrid[y][x].connections[1]) connectionsMask |= 2; // down
                if (MapGen.MapGrid[y][x].connections[2]) connectionsMask |= 4; // left
                if (MapGen.MapGrid[y][x].connections[3]) connectionsMask |= 8; // right

                switch (connectionsMask)
                {
                    case 0: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[0]); break;   // ---- single room
                    case 1: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[12]); break;  // U--- vertical bottom
                    case 2: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[10]); break;  // -D-- vertical top
                    case 3: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[11]); break;  // UD-- vertical middle
                    case 4: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[15]); break;  // --L- horizontal right
                    case 5: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[9]); break;   // U-L- bottom right
                    case 6: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[3]); break;   // -DL- top right 
                    case 7: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[6]); break;   // UDL- middle right 
                    case 8: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[13]); break;  // ---R horizontal left
                    case 9: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[7]); break;   // U--R bottom left 
                    case 10: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[1]); break;  // -D-R top left
                    case 11: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[4]); break;  // UD-R middle left
                    case 12: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[14]); break; // --LR horizontal middle
                    case 13: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[8]); break;  // U-LR bottom middle
                    case 14: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[2]); break;  // -DLR top middle
                    case 15: roomsTilemap.SetTile(new Vector3Int(x, y, 0), currentTileSet[5]); break;  // UDLR middle center
                }
            }
        }

        // add door tiles to map
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                if (MapGen.MapGrid[y][x].region == 0)
                    continue;

                // use bitmask to get right tiles for doors
                int doorsMask = 0;
                if (MapGen.MapGrid[y][x].doors[0]) doorsMask |= 1; // up
                if (MapGen.MapGrid[y][x].doors[1]) doorsMask |= 2; // down
                if (MapGen.MapGrid[y][x].doors[2]) doorsMask |= 4; // left
                if (MapGen.MapGrid[y][x].doors[3]) doorsMask |= 8; // right

                switch (doorsMask)
                {
                    case 0: break;   // ---- single room
                    case 1: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[0]); break;   // U--- vertical bottom
                    case 2: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[1]); break;   // -D-- vertical top
                    case 3: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[12]); break;  // UD-- vertical middle
                    case 4: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[2]); break;   // --L- horizontal right
                    case 5: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[4]); break;   // U-L- bottom right
                    case 6: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[7]); break;   // -DL- top right 
                    case 7: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[11]); break;  // UDL- middle right 
                    case 8: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[3]); break;   // ---R horizontal left
                    case 9: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[5]); break;   // U--R bottom left 
                    case 10: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[6]); break;  // -D-R top left
                    case 11: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[9]); break;  // UD-R middle left
                    case 12: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[13]); break; // --LR horizontal middle
                    case 13: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[8]); break;  // U-LR bottom middle
                    case 14: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[10]); break; // -DLR top middle
                    case 15: doorsTilemap.SetTile(new Vector3Int(x, y, 0), doorTiles[14]); break; // UDLR middle center
                }
            }
        }
    }

    private void SetupWorld()
    {
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                if (MapGen.MapGrid[y][x].region == 0)
                    continue;

                int py = y * 18;
                int px = x * 32;



                // MapGen.MapRooms


                // create new tilemap at these coordinates, position it
                GameObject roomObject = new GameObject($"Room_{x}_{y}");
                roomObject.transform.SetParent(worldGrid.transform);
                roomObject.transform.localPosition = new Vector3(px, py, 0);

                // add components
                Tilemap tilemap = roomObject.AddComponent<Tilemap>();
                TilemapRenderer renderer = roomObject.AddComponent<TilemapRenderer>();  
                TilemapCollider2D collider = roomObject.AddComponent<TilemapCollider2D>();

                // for now, just check for the starting room
                List<List<int>> grid;
                if (MapGen.MapGrid[y][x].startRoom)
                    grid = CsvUtility.LoadGridFromCSV("Data/Rooms/start.csv");
                else
                    grid = CsvUtility.LoadGridFromCSV("Data/Rooms/1/1/1x1_2/1.csv");   

                // draw the tiles to this room
                for (int gy = 0; gy < 18; gy++)
                {
                    for (int gx = 0; gx < 32; gx++)
                    {
                        // flip csv data to display properly
                        int tile = grid[17 - gy][gx];
                        switch (tile)
                        {
                            case 0: break;
                            case 1: tilemap.SetTile(new Vector3Int(gx, gy, 0), roomTiles[0]); break;
                            case 2: tilemap.SetTile(new Vector3Int(gx, gy, 0), roomTiles[1]); break;
                            case 3: tilemap.SetTile(new Vector3Int(gx, gy, 0), roomTiles[2]); break;
                            case 4: tilemap.SetTile(new Vector3Int(gx, gy, 0), roomTiles[3]); break;
                        }
                    }
                }

                
            }
        }
    }
}
