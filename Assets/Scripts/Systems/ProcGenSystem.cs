using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    [SerializeField] private Grid worldGrid;
    [SerializeField] private Tile[] fgTiles;
    [SerializeField] private Tile[] bgTiles;
    [SerializeField] private Tile[] hazardTiles;
    [SerializeField] private Tile[] entityTiles;

    [Header("Entity Generation")]
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject door;

    private readonly int[] gridSizes = { 16, 24, 32 };

    void Start()
    {   
        // set the seed
        UnityEngine.Random.InitState(GameSystem.Instance.seed); 

        // first generate the world map
        MapGen.GenerateMap(gridSizes[GameSystem.Instance.length]);

        // then using map data generate rooms for the player to explore
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

                // check for special rooms first
                if (MapGen.MapGrid[y][x].name != null)
                {
                    string name = MapGen.MapGrid[y][x].name;
                    if (name.Contains("Start"))
                    {
                        roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[16]);
                        continue;
                    }
                    else if (name.Contains("Boss"))
                    {
                        roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[17]);
                        continue;
                    }
                    else if (name.Contains("Item"))
                    {
                        roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[18]);
                        continue;
                    }
                }

                // use bitmask to get right tiles to make connected rooms
                int connectionsMask = 0;
                if (MapGen.MapGrid[y][x].connections[0]) connectionsMask |= 1; // up
                if (MapGen.MapGrid[y][x].connections[1]) connectionsMask |= 2; // down
                if (MapGen.MapGrid[y][x].connections[2]) connectionsMask |= 4; // left
                if (MapGen.MapGrid[y][x].connections[3]) connectionsMask |= 8; // right

                switch (connectionsMask)
                {
                    case 0: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[0]); break;   // ---- single room
                    case 1: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[12]); break;  // U--- vertical bottom
                    case 2: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[10]); break;  // -D-- vertical top
                    case 3: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[11]); break;  // UD-- vertical middle
                    case 4: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[15]); break;  // --L- horizontal right
                    case 5: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[9]); break;   // U-L- bottom right
                    case 6: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[3]); break;   // -DL- top right 
                    case 7: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[6]); break;   // UDL- middle right 
                    case 8: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[13]); break;  // ---R horizontal left
                    case 9: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[7]); break;   // U--R bottom left 
                    case 10: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[1]); break;  // -D-R top left
                    case 11: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[4]); break;  // UD-R middle left
                    case 12: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[14]); break; // --LR horizontal middle
                    case 13: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[8]); break;  // U-LR bottom middle
                    case 14: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[2]); break;  // -DLR top middle
                    case 15: roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[5]); break;  // UDLR middle center
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
                    case 1: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[0]); break;   // U--- vertical bottom
                    case 2: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[1]); break;   // -D-- vertical top
                    case 3: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[12]); break;  // UD-- vertical middle
                    case 4: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[2]); break;   // --L- horizontal right
                    case 5: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[4]); break;   // U-L- bottom right
                    case 6: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[7]); break;   // -DL- top right 
                    case 7: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[11]); break;  // UDL- middle right 
                    case 8: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[3]); break;   // ---R horizontal left
                    case 9: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[5]); break;   // U--R bottom left 
                    case 10: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[6]); break;  // -D-R top left
                    case 11: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[9]); break;  // UD-R middle left
                    case 12: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[13]); break; // --LR horizontal middle
                    case 13: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[8]); break;  // U-LR bottom middle
                    case 14: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[10]); break; // -DLR top middle
                    case 15: doorsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), doorTiles[14]); break; // UDLR middle center
                }
            }
        }
    }

    private void SetupWorld()
    {
        // make special rooms
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                if (MapGen.MapGrid[y][x].name != null)
                {
                    int py = y * 18;
                    int px = x * 32;

                    // create new room at these coordinates, position it
                    GameObject roomObject = new GameObject($"Room_{x}_{y}");
                    roomObject.transform.SetParent(worldGrid.transform);
                    roomObject.transform.localPosition = new Vector3(px, FlipPY(py), 0);

                    // add background tilemap layer
                    GameObject bg = new GameObject($"Bg");
                    bg.transform.SetParent(roomObject.transform, false);
                    Tilemap bgTilemap = bg.AddComponent<Tilemap>();
                    TilemapRenderer bgRenderer = bg.AddComponent<TilemapRenderer>();  
                    bgTilemap.color = new Color(1, 1, 1, 32f/255f); // lower opacity
                    bgRenderer.sortingLayerName = "Environment";
                    bgRenderer.sortingOrder = 0;

                    // add foreground tilemap layer
                    GameObject fg = new GameObject($"Fg");
                    fg.transform.SetParent(roomObject.transform, false);
                    Tilemap fgTilemap = fg.AddComponent<Tilemap>();
                    TilemapRenderer fgRenderer = fg.AddComponent<TilemapRenderer>();  
                    TilemapCollider2D fgCollider = fg.AddComponent<TilemapCollider2D>();
                    fgRenderer.sortingLayerName = "Environment";
                    fgRenderer.sortingOrder = 1;
                    fg.layer = LayerMask.NameToLayer("Ground"); // make sure to set the layer so the player properly interacts with ground

                    // add hazard tilemap layer
                    GameObject hazard = new GameObject($"Hazard");
                    hazard.transform.SetParent(roomObject.transform, false);
                    Tilemap hazardTilemap = hazard.AddComponent<Tilemap>();
                    TilemapRenderer hazardRenderer = hazard.AddComponent<TilemapRenderer>();
                    TilemapCollider2D hazardCollider = hazard.AddComponent<TilemapCollider2D>();
                    hazardRenderer.sortingLayerName = "Environment";
                    hazardRenderer.sortingOrder = 1;
                    hazard.layer = LayerMask.NameToLayer("Hazard"); // make sure to set the layer so the player properly interacts with ground
                    hazardCollider.isTrigger = true;

                    // add entity tilemap layer
                    GameObject entity = new GameObject($"Entity");
                    entity.transform.SetParent(roomObject.transform, false);
                    Tilemap entityTilemap = entity.AddComponent<Tilemap>();
                    TilemapRenderer entityRenderer = entity.AddComponent<TilemapRenderer>();  
                    entityRenderer.enabled = false; // disable renderer to make entity editor tiles invisible
                    entityRenderer.sortingLayerName = "Environment";
                    entityRenderer.sortingOrder = 2;

                    // load csv layout data (special rooms will always be 1x1)
                    string path = $"Data/Rooms/Special/{MapGen.MapGrid[y][x].name}/";
                    List<List<int>> fgGrid = CsvUtility.LoadGridFromCSV(path + $"Fg/1.csv");  
                    List<List<int>> bgGrid = CsvUtility.LoadGridFromCSV(path + $"Bg/1.csv");
                    List<List<int>> hazardGrid = CsvUtility.LoadGridFromCSV(path + $"Hazard/1.csv");  
                    List<List<int>> entityGrid = CsvUtility.LoadGridFromCSV(path + $"Entity/1.csv");  

                    bool colors = true;

                    if (MapGen.MapGrid[y][x].name == "Start")
                    {
                        // move the player to the starting room
                        player.transform.localPosition = new Vector3(px + 6, FlipPY(py) + 4, 0);
                        colors = false;
                    }
                    
                    // draw the tiles to this room
                    if (fgGrid != null)
                        DrawToTilemap(1, fgTilemap, fgGrid, MapGen.MapGrid[y][x].region, colors);
                    if (bgGrid != null)
                        DrawToTilemap(2, bgTilemap, bgGrid, MapGen.MapGrid[y][x].region, colors);
                    if (hazardGrid != null)
                        DrawToTilemap(3, hazardTilemap, hazardGrid, MapGen.MapGrid[y][x].region, false);
                    if (entityGrid != null)
                        DrawToTilemap(4, entityTilemap, entityGrid, MapGen.MapGrid[y][x].region, false);
                }
            }
        }

        // make all other rooms for all the regions
        foreach (List<MapTile> room in MapGen.MapRooms)
        {
            if (room.Count == 0)
                continue;

            if (room[0].name != null)
                continue;

            int min_x = room[0].position.x;
            int min_y = room[0].position.y;

            int max_x = room[room.Count-1].position.x;
            int max_y = room[room.Count-1].position.y;
            
            // figure out the dimensions of the room
            Vector2Int dims = new Vector2Int(max_x - min_x + 1, max_y - min_y + 1);

            // pick a random room layout of the current size
            string path = $"Data/Rooms/{dims.x}/{dims.y}/";
            int numFolders = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, path)).Length;
            int layoutNum = UnityEngine.Random.Range(1, numFolders); // ignore the Base layout

            int index = 1;
            for (int y = min_y; y <= max_y; y++)
            {
                for (int x = min_x; x <= max_x; x++)
                {
                    int region = MapGen.MapGrid[y][x].region;

                    if (region == 0)
                        continue;

                    int py = y * 18;
                    int px = x * 32;

                    // create new room at these coordinates, position it
                    GameObject roomObject = new GameObject($"Room_{x}_{y}");
                    roomObject.transform.SetParent(worldGrid.transform);
                    roomObject.transform.localPosition = new Vector3(px, FlipPY(py), 0);

                    // add background tilemap layer
                    GameObject bg = new GameObject($"Bg");
                    bg.transform.SetParent(roomObject.transform, false);
                    Tilemap bgTilemap = bg.AddComponent<Tilemap>();
                    TilemapRenderer bgRenderer = bg.AddComponent<TilemapRenderer>();  
                    bgTilemap.color = new Color(1, 1, 1, 32f/255f); // lower opacity
                    bgRenderer.sortingLayerName = "Environment";
                    bgRenderer.sortingOrder = 0;

                    // add foreground tilemap layer
                    GameObject fg = new GameObject($"Fg");
                    fg.transform.SetParent(roomObject.transform, false);
                    Tilemap fgTilemap = fg.AddComponent<Tilemap>();
                    TilemapRenderer fgRenderer = fg.AddComponent<TilemapRenderer>();  
                    TilemapCollider2D fgCollider = fg.AddComponent<TilemapCollider2D>();
                    fgRenderer.sortingLayerName = "Environment";
                    fgRenderer.sortingOrder = 1;
                    fg.layer = LayerMask.NameToLayer("Ground"); // make sure to set the layer so the player properly interacts with ground

                    // add hazard tilemap layer
                    GameObject hazard = new GameObject($"Hazard");
                    hazard.transform.SetParent(roomObject.transform, false);
                    Tilemap hazardTilemap = hazard.AddComponent<Tilemap>();
                    TilemapRenderer hazardRenderer = hazard.AddComponent<TilemapRenderer>();
                    TilemapCollider2D hazardCollider = hazard.AddComponent<TilemapCollider2D>();
                    hazardRenderer.sortingLayerName = "Environment";
                    hazardRenderer.sortingOrder = 1;
                    hazard.layer = LayerMask.NameToLayer("Hazard"); // make sure to set the layer so the player properly interacts with ground
                    hazardCollider.isTrigger = true;

                    // add entity tilemap layer
                    GameObject entity = new GameObject($"Entity");
                    entity.transform.SetParent(roomObject.transform, false);
                    Tilemap entityTilemap = entity.AddComponent<Tilemap>();
                    TilemapRenderer entityRenderer = entity.AddComponent<TilemapRenderer>();  
                    entityRenderer.enabled = false; // disable renderer to make entity editor tiles invisible
                    entityRenderer.sortingLayerName = "Environment";
                    entityRenderer.sortingOrder = 2;

                    // load csv layout data
                    List<List<int>> fgGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_{layoutNum}/Fg/{index}.csv");  
                    List<List<int>> bgGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_{layoutNum}/Bg/{index}.csv");  
                    List<List<int>> hazardGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_{layoutNum}/Hazard/{index}.csv");  
                    List<List<int>> entityGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_{layoutNum}/Entity/{index}.csv");  

                    // fallback to Base if no custom layouts exist
                    if (fgGrid == null && bgGrid == null && entityGrid == null)
                    {
                        fgGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_Base/Fg/{index}.csv");  
                        bgGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_Base/Bg/{index}.csv"); 
                        hazardGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_Base/Hazard/{index}.csv");
                        entityGrid = CsvUtility.LoadGridFromCSV(path + $"{dims.x}x{dims.y}_Base/Entity/{index}.csv"); 
                    }
                    
                    // draw the tiles to this room for each included layer (some layouts like Base are missing some layers)
                    if (fgGrid != null)
                        DrawToTilemap(1, fgTilemap, fgGrid, region, true);
                    if (bgGrid != null)
                        DrawToTilemap(2, bgTilemap, bgGrid, region, true);
                    if (hazardGrid != null)
                        DrawToTilemap(3, hazardTilemap, hazardGrid, region, false);
                    if (entityGrid != null)
                        DrawToTilemap(4, entityTilemap, entityGrid, region, false);

                    // create a container to hold the entities for this room
                    GameObject entityContainer = new GameObject($"EntityContainer");
                    entityContainer.transform.SetParent(roomObject.transform, false);
                    fg.layer = LayerMask.NameToLayer("Ground"); // make sure to set the layer so the player properly interacts with ground

                    // spawn entities for the entity grid
                    if (entityGrid != null)
                    {
                        for (int ey = 0; ey < 18; ey++)
                        {
                            for (int ex = 0; ex < 32; ex++)
                            {
                                int tile = entityGrid[17 - ey][ex];

                                GameObject enemy = enemies[0];

                                // get the world position of an entity tile
                                Vector3 tilePos = entityTilemap.GetCellCenterWorld(new Vector3Int(ex, ey, 0));
                                switch (tile)
                                {
                                    case 1: enemy = Instantiate(enemies[0], tilePos, Quaternion.identity, entityContainer.transform); break; // jumper
                                    case 2: enemy = Instantiate(enemies[1], tilePos, Quaternion.identity, entityContainer.transform); break; // dropper
                                    case 3: enemy = Instantiate(enemies[2], tilePos, Quaternion.identity, entityContainer.transform); break; // scuttler
                                    case 4: enemy = Instantiate(enemies[3], tilePos, Quaternion.identity, entityContainer.transform); break; // drifter
                                }

                                enemy.layer = LayerMask.NameToLayer("Enemy");
                            }
                        }
                    }
                    
                    index++;
                }
            }
        }

        // carve doors between rooms
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                if (worldGrid.transform.Find($"Room_{x}_{y}") == null)
                    continue;
                
                GameObject roomObject = worldGrid.transform.Find($"Room_{x}_{y}").gameObject;
                Tilemap fgTilemap = roomObject.transform.Find("Fg").GetComponent<Tilemap>();
                int region = MapGen.MapGrid[y][x].region;

                // create a container to hold door entities for this room
                GameObject doorContainer = new GameObject($"Doors");
                doorContainer.transform.SetParent(roomObject.transform, false);

                if (MapGen.MapGrid[y][x].doors[0])
                {
                    // layer up_door tiles to carve door on top wall of the room
                    List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/up_door.csv");
                    DrawToTilemap(1, fgTilemap, grid, region, false);

                    // create a door facing down
                    Instantiate(door, new Vector3(roomObject.transform.position.x + 16, roomObject.transform.position.y + 18, 0), Quaternion.Euler(0, 0, 90), doorContainer.transform);
                }

                if (MapGen.MapGrid[y][x].doors[1])
                {
                    // layer down_door tiles to carve door on bottom wall of the room
                    List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/down_door.csv");  
                    DrawToTilemap(1, fgTilemap, grid, region, false);

                    // create a door facing up
                    Instantiate(door, new Vector3(roomObject.transform.position.x + 16, roomObject.transform.position.y, 0), Quaternion.Euler(0, 0, 270), doorContainer.transform);
                }

                if (MapGen.MapGrid[y][x].doors[2])
                {
                    // layer left_door tiles to carve door on left wall of a room
                    List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/left_door.csv");  
                    DrawToTilemap(1, fgTilemap, grid, region, false);

                    // create a door facing right
                    Instantiate(door, new Vector3(roomObject.transform.position.x, roomObject.transform.position.y + 9, 0), Quaternion.Euler(0, 0, 180), doorContainer.transform);
                }

                if (MapGen.MapGrid[y][x].doors[3])
                {
                    // layer right_door tiles to carve door on right wall of a room
                    List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/right_door.csv");  
                    DrawToTilemap(1, fgTilemap, grid, region, false);
                    
                    // create a door facing left
                    Instantiate(door, new Vector3(roomObject.transform.position.x + 32, roomObject.transform.position.y + 9, 0), Quaternion.identity, doorContainer.transform);
                }
            }
        }
    }

    private void DrawToTilemap(int layer, Tilemap tilemap, List<List<int>> grid, int region, bool colored)
    {
        Tile[] currentTiles = fgTiles;
        switch (layer)
        {
            case 1: currentTiles = fgTiles; break;
            case 2: currentTiles = bgTiles; break;
            case 3: currentTiles = hazardTiles; break;
            case 4: currentTiles = entityTiles; break;
        }

        Color[] currentColors = new Color[5]{Color.white, Color.white, Color.white, Color.white, Color.white};
        switch (region)
        {
            case 1: currentColors = r1Colors; break; // blue
            case 2: currentColors = r2Colors; break; // red
            case 3: currentColors = r3Colors; break; // orange
            case 4: currentColors = r4Colors; break; // green
        }
        
        for (int gy = 0; gy < 18; gy++)
        {
            for (int gx = 0; gx < 32; gx++)
            {
                // flip csv data to display properly
                int tile = grid[17 - gy][gx];
                switch (tile)
                {
                    case 0: break;
                    case 1: tilemap.SetTile(new Vector3Int(gx, gy, 0), currentTiles[0]); break;
                    case 2: tilemap.SetTile(new Vector3Int(gx, gy, 0), currentTiles[1]); break;
                    case 3: tilemap.SetTile(new Vector3Int(gx, gy, 0), currentTiles[2]); break;
                    case 4: tilemap.SetTile(new Vector3Int(gx, gy, 0), currentTiles[3]); break;
                    case 5: tilemap.SetTile(new Vector3Int(gx, gy, 0), null); break;
                }

                // pick a random color for this tile of its region
                if (colored)
                {
                    Color randColor = currentColors[UnityEngine.Random.Range(0, currentColors.Length)];
                    tilemap.SetTileFlags(new Vector3Int(gx, gy, 0), TileFlags.None);
                    tilemap.SetColor(new Vector3Int(gx, gy, 0), randColor);
                }
            }
        }
    }

    private int FlipY(int y)
    {
        return (MapGen.GridSize - 1) - y;
    }

    private int FlipPY(int py)
    {
        return ((MapGen.GridSize * 18) - 18) - py;
    }

    private readonly Color[] r1Colors = 
    {
        new Color(88f/255f, 184f/255f, 1), // +2 lighter
        new Color(42f/255f, 165f/255f, 1), // +1 lighter
        new Color(0, 148f/255f, 1), //  0 base color (blue)
        new Color(0, 123f/255f, 212f/255f), // -1 darker
        new Color(0, 100f/255f, 171f/255f), // -2 darker
    };

    private readonly Color[] r2Colors = 
    {
        new Color(1, 80f/255f, 80f/255f), // +2 lighter
        new Color(1, 40f/255f, 40f/255f), // +1 lighter
        new Color(1, 0, 0), //  0 base color (red)
        new Color(207f/255f, 0, 0), // -1 darker
        new Color(162f/255f, 0, 0), // -2 darker
    };

    private readonly Color[] r3Colors = 
    {
        new Color(1, 152f/255f, 79f/255f), // +2 lighter
        new Color(1, 130f/255f, 42f/255f), // +1 lighter
        new Color(1, 106f/255f, 0), //  0 base color (orange)
        new Color(214f/255f, 89f/255f, 0), // -1 darker
        new Color(164f/255f, 69f/255f, 0), // -2 darker
    };

    private readonly Color[] r4Colors = 
    {
        new Color(136f/255f, 1, 86f/255f), // +2 lighter
        new Color(107f/255f, 1, 45f/255f), // +1 lighter
        new Color(76f/255f, 1, 0), //  0 base color (green)
        new Color(63f/255f, 210f/255f, 0), // -1 darker
        new Color(49f/255f, 166f/255f, 0), // -2 darker
    };
}
