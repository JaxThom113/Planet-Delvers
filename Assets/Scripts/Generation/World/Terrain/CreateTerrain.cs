using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateTerrain : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Room Generation")]
    [SerializeField] private Grid worldGrid;
    [SerializeField] private Tile[] fgTiles;
    [SerializeField] private Tile[] bgTiles;
    [SerializeField] private Tile[] hazardTiles;
    [SerializeField] private Tile[] entityTiles;

    [Header("Entity Generation")]
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject[] bosses;
    [SerializeField] private GameObject[] items;
    [SerializeField] private GameObject door;

    [Header("Lighting")]
    [SerializeField] private Material lightingMaterial;

    private int roomIndex;

    public void SetupTerrain()
    {
        roomIndex = 0;

        // Step #1: Create all 1x1 special rooms (Start, Bosses, Items)
        CreateSpecialRooms();

        // Step #2: Create remaining normal rooms, build out overworld
        CreateAllRooms();

        // Step #3: Carve doors between rooms
        CarveAllDoors();
    }

    private void CreateSpecialRooms()
    {
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                string name = MapGen.MapGrid[y][x].name;

                if (name != null)
                {
                    bool colors = true;

                    // special cells that need unique instructions
                    if (name == "Start")
                    {
                        int py = y * 18;
                        int px = x * 32;

                        // move the player to the starting room
                        player.transform.localPosition = new Vector3(px + 6, FlipPY(py) + 4, 0);
                        colors = false;
                    }

                    string path = $"Data/Rooms/Special/{name}/";

                    // create new room gameobject to store all of a room's cells and their data
                    GameObject roomObject = new GameObject($"Room_{roomIndex}");
                    roomObject.transform.SetParent(worldGrid.transform);
                    roomObject.transform.localPosition = Vector3.zero;
                    CreateRoom(
                        roomObject,
                        x, y,
                        path,
                        colors
                    );

                    roomIndex++;
                }
            }
        }    
    }

    private void CreateAllRooms()
    {
        foreach (List<MapTile> room in MapGen.MapRooms)
        {
            // skip custom rooms
            if (room[0].name != null)
                continue;

            if (room.Count == 0)
                continue;

            int min_x = room[0].position.x;
            int min_y = room[0].position.y;

            int max_x = room[room.Count-1].position.x;
            int max_y = room[room.Count-1].position.y;
            
            // figure out the room / cell dimensions of the room
            Vector2Int roomDims = new Vector2Int(max_x - min_x + 1, max_y - min_y + 1);
            Vector2Int cellDims = new Vector2Int(roomDims.x * 32, roomDims.y * 18);

            // pick a random room layout of the current size from csv
            string path = $"Data/Rooms/{roomDims.x}/{roomDims.y}/";
            int numFolders = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, path)).Length;
            int layoutNum = UnityEngine.Random.Range(1, numFolders); // ignore the Base layout

            // create new room gameobject to store all of a room's cells and their data
            GameObject roomObject = new GameObject($"Room_{roomIndex}");
            roomObject.transform.SetParent(worldGrid.transform);
            roomObject.transform.localPosition = Vector3.zero;
            CreateRoom(
                roomObject, 
                min_x, min_y, 
                max_x, max_y,
                path, 
                layoutNum,
                roomDims,
                cellDims
            );

            roomIndex++;
        }
    }

    private void CarveAllDoors()
    {
        int tempRoomIndex = 0;

        foreach (List<MapTile> room in MapGen.MapRooms)
        {
            // access the previously created roomObject
            GameObject roomObject = worldGrid.transform.Find($"Room_{tempRoomIndex}").gameObject;
            if (roomObject == null)
                continue;

            for (int y = 0; y < MapGen.GridSize; y++)
            {
                for (int x = 0; x < MapGen.GridSize; x++)
                {
                    // access the cell objects created inside this room
                    if (roomObject.transform.Find($"Cell_{x}_{y}") == null)
                        continue;

                    GameObject cellObject = roomObject.transform.Find($"Cell_{x}_{y}").gameObject;
                    Tilemap fgTilemap = cellObject.transform.Find("Fg").GetComponent<Tilemap>();
                    int region = MapGen.MapGrid[y][x].region;

                    // create a container to hold door entities for this room
                    GameObject doorContainer = new GameObject($"Doors");
                    doorContainer.transform.SetParent(cellObject.transform, false);

                    if (MapGen.MapGrid[y][x].doors[0])
                    {
                        // layer up_door tiles to carve door on top wall of the room
                        List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/up_door.csv");
                        DrawToTilemap(fgTiles, fgTilemap, grid, region, false);

                        // create a door facing down
                        Instantiate(door, new Vector3(cellObject.transform.position.x + 16, cellObject.transform.position.y + 18, 0), Quaternion.Euler(0, 0, 90), doorContainer.transform);
                    }

                    if (MapGen.MapGrid[y][x].doors[1])
                    {
                        // layer down_door tiles to carve door on bottom wall of the room
                        List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/down_door.csv");  
                        DrawToTilemap(fgTiles, fgTilemap, grid, region, false);

                        // create a door facing up
                        Instantiate(door, new Vector3(cellObject.transform.position.x + 16, cellObject.transform.position.y, 0), Quaternion.Euler(0, 0, 270), doorContainer.transform);
                    }

                    if (MapGen.MapGrid[y][x].doors[2])
                    {
                        // layer left_door tiles to carve door on left wall of a room
                        List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/left_door.csv");  
                        DrawToTilemap(fgTiles, fgTilemap, grid, region, false);

                        // create a door facing right
                        Instantiate(door, new Vector3(cellObject.transform.position.x, cellObject.transform.position.y + 9, 0), Quaternion.Euler(0, 0, 180), doorContainer.transform);
                    }

                    if (MapGen.MapGrid[y][x].doors[3])
                    {
                        // layer right_door tiles to carve door on right wall of a room
                        List<List<int>> grid = CsvUtility.LoadGridFromCSV("Data/World/right_door.csv");  
                        DrawToTilemap(fgTiles, fgTilemap, grid, region, false);
                        
                        // create a door facing left
                        Instantiate(door, new Vector3(cellObject.transform.position.x + 32, cellObject.transform.position.y + 9, 0), Quaternion.identity, doorContainer.transform);
                    }
                }
            }
            
            tempRoomIndex++;
        }
    }

    /*
        Base room creation functions for creating special rooms
    */

    private void CreateRoom(GameObject roomObject, int x, int y, string path, bool color)
    {
        List<List<int>> roomData = MapGenUtility.InitializeIntGrid(32, 18);

        // create all the cells for this room
        int region = MapGen.MapGrid[y][x].region;

        if (region == 0)
            return;

        int py = y * 18;
        int px = x * 32;

        // assign the position of the roomObject (should be the top left cell)
        if (roomObject.transform.localPosition == Vector3.zero)
            roomObject.transform.localPosition = new Vector3(px, FlipPY(py), 0);

        // create new cell at these coordinates, position it
        GameObject cellObject = new GameObject($"Cell_{x}_{y}");
        cellObject.transform.SetParent(roomObject.transform);
        cellObject.transform.position = new Vector3(px, FlipPY(py), 0);
        CreateCell(
            cellObject,
            path,
            color,
            region
        );

        // get fg grid data to add to the depth map
        List<List<int>> fgGrid = CsvUtility.LoadGridFromCSV(path + $"Fg/1.csv");  
        if (fgGrid == null)
            fgGrid = CsvUtility.LoadGridFromCSV($"Data/Rooms/1/1/1x1_Base/Fg/1.csv");

        for (int gy = 0; gy < 18; gy++)
        {
            for (int gx = 0; gx < 32; gx++)
            {
                int tile = fgGrid[17 - gy][gx];
                roomData[gy][gx] = tile;
            }
        }

        // create a container to hold tile-based lighting for each room
        GameObject lighting = new GameObject($"Lighting");
        lighting.transform.SetParent(roomObject.transform, false);
        lighting.transform.localPosition = Vector3.zero;

        // with all the fg tilemap data in roomData, convert it to the actual depth map, then a vertex depth map
        List<List<float>> depthMap = LightingMesh.GenerateDepthMap(roomData);
        List<List<float>> vertexDepthMap = LightingMesh.GenerateVertexDepthMap(depthMap);

        LightingMesh.CreateLightingMesh(lighting, vertexDepthMap, lightingMaterial, 1f);
    }

    private void CreateCell(GameObject cellObject, string path, bool color, int region)
    {
        // Step #1: Create the background layer
        CreateBg(cellObject, path, color, region);

        // Step #2: Create the foreground layer
        CreateFg(cellObject, path, color, region);
        
        // Step #3: Creat the hazard layer
        CreateHazard(cellObject, path, region);
        
        // Step #4 Create the entity layer + spawn entities
        CreateEntity(cellObject, path, region);
    }

    private void CreateBg(GameObject cellObject, string path, bool color, int region)
    {
        // add background tilemap layer
        GameObject bg = new GameObject($"Bg");
        bg.transform.SetParent(cellObject.transform, false);
        Tilemap bgTilemap = bg.AddComponent<Tilemap>();
        TilemapRenderer bgRenderer = bg.AddComponent<TilemapRenderer>();  
        bgTilemap.color = new Color(1, 1, 1, 32f/255f); // lower opacity
        bgRenderer.sortingLayerName = "Environment";
        bgRenderer.sortingOrder = 0;

        // load csv data, if not found, default to Base room layout
        List<List<int>> bgGrid = CsvUtility.LoadGridFromCSV(path + $"Bg/1.csv");  
        if (bgGrid == null)
            bgGrid = CsvUtility.LoadGridFromCSV($"Data/Rooms/1/1/1x1_Base/Bg/1.csv");
        
        // draw to the tilemap
        DrawToTilemap(bgTiles, bgTilemap, bgGrid, region, color);
    }

    private void CreateFg(GameObject cellObject, string path, bool color, int region)
    {
        // add foreground tilemap layer
        GameObject fg = new GameObject($"Fg");
        fg.transform.SetParent(cellObject.transform, false);
        Tilemap fgTilemap = fg.AddComponent<Tilemap>();
        TilemapRenderer fgRenderer = fg.AddComponent<TilemapRenderer>();  
        TilemapCollider2D fgCollider = fg.AddComponent<TilemapCollider2D>();
        fgRenderer.sortingLayerName = "Environment";
        fgRenderer.sortingOrder = 1;
        fg.layer = LayerMask.NameToLayer("Ground"); // make sure to set the layer so the player properly interacts with ground

        // load csv data, if not found, default to Base room layout
        List<List<int>> fgGrid = CsvUtility.LoadGridFromCSV(path + $"Fg/1.csv");  
        if (fgGrid == null)
            fgGrid = CsvUtility.LoadGridFromCSV($"Data/Rooms/1/1/1x1_Base/Fg/1.csv");
        
        // draw to the tilemap
        DrawToTilemap(fgTiles, fgTilemap, fgGrid, region, color);

        // make sure to process fg tilemap changes, otherwise collisions will have unexpected behavior
        fgCollider.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
    }

    private void CreateHazard(GameObject cellObject, string path, int region)
    {
        // add hazard tilemap layer
        GameObject hazard = new GameObject($"Hazard");
        hazard.transform.SetParent(cellObject.transform, false);
        Tilemap hazardTilemap = hazard.AddComponent<Tilemap>();
        TilemapRenderer hazardRenderer = hazard.AddComponent<TilemapRenderer>();
        TilemapCollider2D hazardCollider = hazard.AddComponent<TilemapCollider2D>();
        hazardRenderer.sortingLayerName = "Environment";
        hazardRenderer.sortingOrder = 1;
        hazard.layer = LayerMask.NameToLayer("Hazard"); // make sure to set the layer so the player properly interacts with ground
        hazardCollider.isTrigger = true;

        // load csv data, if not found, default to Base room layout
        List<List<int>> hazardGrid = CsvUtility.LoadGridFromCSV(path + $"Hazard/1.csv");  
        if (hazardGrid == null)
            hazardGrid = CsvUtility.LoadGridFromCSV($"Data/Rooms/1/1/1x1_Base/Hazard/1.csv");
        
        // draw to the tilemap
        DrawToTilemap(hazardTiles, hazardTilemap, hazardGrid, region, false);
    }

    private void CreateEntity(GameObject cellObject, string path, int region)
    {
        // add entity tilemap layer
        GameObject entity = new GameObject($"Entity");
        entity.transform.SetParent(cellObject.transform, false);
        Tilemap entityTilemap = entity.AddComponent<Tilemap>();
        TilemapRenderer entityRenderer = entity.AddComponent<TilemapRenderer>();  
        entityRenderer.enabled = false; // disable renderer to make entity editor tiles invisible
        entityRenderer.sortingLayerName = "Environment";
        entityRenderer.sortingOrder = 2;

        // load csv data, if not found, default to Base room layout
        List<List<int>> entityGrid = CsvUtility.LoadGridFromCSV(path + $"Entity/1.csv");  
        if (entityGrid == null)
            entityGrid = CsvUtility.LoadGridFromCSV($"Data/Rooms/1/1/1x1_Base/Entity/1.csv");
        
        // draw to the tilemap
        DrawToTilemap(entityTiles, entityTilemap, entityGrid, region, false);

        // create a container to hold the entities for this room
        GameObject entityContainer = new GameObject($"EntityContainer");
        entityContainer.transform.SetParent(cellObject.transform, false);

        // spawn entities for the entity grid
        for (int ey = 0; ey < 18; ey++)
        {
            for (int ex = 0; ex < 32; ex++)
            {
                int tile = entityGrid[17 - ey][ex];
                if (tile == 0)
                    continue;

                Vector3 tilePos = entityTilemap.GetCellCenterWorld(new Vector3Int(ex, ey, 0));

                GameObject newEntity;
                int entityIndex = tile - 1;

                switch (tile)
                {
                    // items
                    case > 32: 
                        entityIndex -= 32;
                        newEntity = Instantiate(items[entityIndex], tilePos, Quaternion.identity, entityContainer.transform);
                        break;

                    // bosses
                    case > 16: 
                        entityIndex -= 16;
                        newEntity = Instantiate(bosses[entityIndex], tilePos, Quaternion.identity, entityContainer.transform);
                        break;

                    // enemies
                    default: 
                        newEntity = Instantiate(enemies[entityIndex], tilePos, Quaternion.identity, entityContainer.transform);
                        break;
                }
                
                newEntity.layer = LayerMask.NameToLayer("Enemy");
            }
        }
    }

    /*
        Overload functions for creating rooms randomly
    */

    private void CreateRoom(GameObject roomObject, int min_x, int min_y, int max_x, int max_y, string path, int layoutNum, Vector2Int roomDims, Vector2Int cellDims)
    {
        List<List<int>> roomData = MapGenUtility.InitializeIntGrid(cellDims.x, cellDims.y);

        Vector3 lightingPos = Vector3.zero;

        // create all the cells for this room
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

                // assign the position of the roomObject (should be the top left cell)
                if (roomObject.transform.localPosition == Vector3.zero)
                    roomObject.transform.localPosition = new Vector3(px, FlipPY(py), 0);
                
                // create new cell at these coordinates, position it
                GameObject cellObject = new GameObject($"Cell_{x}_{y}");
                cellObject.transform.SetParent(roomObject.transform);
                cellObject.transform.position = new Vector3(px, FlipPY(py), 0);
                CreateCell(
                    cellObject,
                    path,
                    layoutNum,
                    index,
                    roomDims,
                    region
                );

                // get local position of bottom left cell to position lighting correctly
                if (y == max_y && x == min_x)
                    lightingPos = cellObject.transform.localPosition;

                // get fg grid data to add to the depth map
                List<List<int>> fgGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_{layoutNum}/Fg/{index}.csv");  
                if (fgGrid == null)
                    fgGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_Base/Fg/{index}.csv");

                int offsetX = (x - min_x) * 32;
                int offsetY = (max_y - y) * 18; // flip y so mesh displays correctly

                for (int gy = 0; gy < 18; gy++)
                {
                    for (int gx = 0; gx < 32; gx++)
                    {
                        int tile = fgGrid[17 - gy][gx];

                        int roomX = offsetX + gx;
                        int roomY = offsetY + gy;

                        roomData[roomY][roomX] = tile;
                    }
                }
                
                index++;
            }
        }

        // create a container to hold tile-based lighting for each room
        GameObject lighting = new GameObject($"Lighting");
        lighting.transform.SetParent(roomObject.transform, false);
        lighting.transform.localPosition = lightingPos; // offset lighting to bottom left cell

        // with all the fg tilemap data in roomData, convert it to the actual depth map, then a vertex depth map
        List<List<float>> depthMap = LightingMesh.GenerateDepthMap(roomData);
        List<List<float>> vertexDepthMap = LightingMesh.GenerateVertexDepthMap(depthMap);

        LightingMesh.CreateLightingMesh(lighting,  vertexDepthMap, lightingMaterial, 1f);
    }

    private void CreateCell(GameObject cellObject, string path, int layoutNum, int index, Vector2Int roomDims, int region)
    {
        // Step #1: Create the background layer
        CreateBg(cellObject, path, layoutNum, index, roomDims, region);

        // Step #2: Create the foreground layer
        CreateFg(cellObject, path, layoutNum, index, roomDims, region);
        
        // Step #3: Creat the hazard layer
        CreateHazard(cellObject, path, layoutNum, index, roomDims, region);
        
        // Step #4 Create the entity layer + spawn entities
        CreateEntity(cellObject, path, layoutNum, index, roomDims, region);
    }

    private void CreateBg(GameObject cellObject, string path, int layoutNum, int index, Vector2Int roomDims, int region)
    {
        // add background tilemap layer
        GameObject bg = new GameObject($"Bg");
        bg.transform.SetParent(cellObject.transform, false);
        Tilemap bgTilemap = bg.AddComponent<Tilemap>();
        TilemapRenderer bgRenderer = bg.AddComponent<TilemapRenderer>();  
        bgTilemap.color = new Color(1, 1, 1, 32f/255f); // lower opacity
        bgRenderer.sortingLayerName = "Environment";
        bgRenderer.sortingOrder = 0;

        // load csv data, if not found, default to Base room layout
        List<List<int>> bgGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_{layoutNum}/Bg/{index}.csv");  
        if (bgGrid == null)
            bgGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_Base/Bg/{index}.csv");
        
        // draw to the tilemap
        DrawToTilemap(bgTiles, bgTilemap, bgGrid, region, true);
    }

    private void CreateFg(GameObject cellObject, string path, int layoutNum, int index, Vector2Int roomDims, int region)
    {
        // add foreground tilemap layer
        GameObject fg = new GameObject($"Fg");
        fg.transform.SetParent(cellObject.transform, false);
        Tilemap fgTilemap = fg.AddComponent<Tilemap>();
        TilemapRenderer fgRenderer = fg.AddComponent<TilemapRenderer>();  
        TilemapCollider2D fgCollider = fg.AddComponent<TilemapCollider2D>();
        fgRenderer.sortingLayerName = "Environment";
        fgRenderer.sortingOrder = 1;
        fg.layer = LayerMask.NameToLayer("Ground"); // make sure to set the layer so the player properly interacts with ground

        // load csv data, if not found, default to Base room layout
        List<List<int>> fgGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_{layoutNum}/Fg/{index}.csv");  
        if (fgGrid == null)
            fgGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_Base/Fg/{index}.csv");
        
        // draw to the tilemap
        DrawToTilemap(fgTiles, fgTilemap, fgGrid, region, true);

        // make sure to process fg tilemap changes, otherwise collisions will have unexpected behavior
        fgCollider.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
    }

    private void CreateHazard(GameObject cellObject, string path, int layoutNum, int index, Vector2Int roomDims, int region)
    {
        // add hazard tilemap layer
        GameObject hazard = new GameObject($"Hazard");
        hazard.transform.SetParent(cellObject.transform, false);
        Tilemap hazardTilemap = hazard.AddComponent<Tilemap>();
        TilemapRenderer hazardRenderer = hazard.AddComponent<TilemapRenderer>();
        TilemapCollider2D hazardCollider = hazard.AddComponent<TilemapCollider2D>();
        hazardRenderer.sortingLayerName = "Environment";
        hazardRenderer.sortingOrder = 1;
        hazard.layer = LayerMask.NameToLayer("Hazard"); // make sure to set the layer so the player properly interacts with ground
        hazardCollider.isTrigger = true;

        // load csv data, if not found, default to Base room layout
        List<List<int>> hazardGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_{layoutNum}/Hazard/{index}.csv");  
        if (hazardGrid == null)
            hazardGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_Base/Fg/{index}.csv");
        
        // draw to the tilemap
        DrawToTilemap(hazardTiles, hazardTilemap, hazardGrid, region, false);
    }

    private void CreateEntity(GameObject cellObject, string path, int layoutNum, int index, Vector2Int roomDims, int region)
    {
        // add entity tilemap layer
        GameObject entity = new GameObject($"Entity");
        entity.transform.SetParent(cellObject.transform, false);
        Tilemap entityTilemap = entity.AddComponent<Tilemap>();
        TilemapRenderer entityRenderer = entity.AddComponent<TilemapRenderer>();  
        entityRenderer.enabled = false; // disable renderer to make entity editor tiles invisible
        entityRenderer.sortingLayerName = "Environment";
        entityRenderer.sortingOrder = 2;

        // load csv data, if not found, default to Base room layout
        List<List<int>> entityGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_{layoutNum}/Entity/{index}.csv");  
        if (entityGrid == null)
            entityGrid = CsvUtility.LoadGridFromCSV(path + $"{roomDims.x}x{roomDims.y}_Base/Entity/{index}.csv");
        
        // draw to the tilemap
        DrawToTilemap(entityTiles, entityTilemap, entityGrid, region, false);

        // create a container to hold the entities for this room
        GameObject entityContainer = new GameObject($"EntityContainer");
        entityContainer.transform.SetParent(cellObject.transform, false);

        // spawn entities for the entity grid
        for (int ey = 0; ey < 18; ey++)
        {
            for (int ex = 0; ex < 32; ex++)
            {
                int tile = entityGrid[17 - ey][ex];
                if (tile == 0)
                    continue;

                Vector3 tilePos = entityTilemap.GetCellCenterWorld(new Vector3Int(ex, ey, 0));

                GameObject newEntity;
                int entityIndex = tile - 1;

                switch (tile)
                {
                    // items
                    case > 32: 
                        entityIndex -= 32;
                        newEntity = Instantiate(items[entityIndex], tilePos, Quaternion.identity, entityContainer.transform);
                        break;

                    // bosses
                    case > 16: 
                        entityIndex -= 16;
                        newEntity = Instantiate(bosses[entityIndex], tilePos, Quaternion.identity, entityContainer.transform);
                        break;

                    // enemies
                    default: 
                        newEntity = Instantiate(enemies[entityIndex], tilePos, Quaternion.identity, entityContainer.transform);
                        break;
                }
                
                newEntity.layer = LayerMask.NameToLayer("Enemy");
            }
        }
    }

    /*
        Helper functions
    */

    private void DrawToTilemap(Tile[] tiles, Tilemap tilemap, List<List<int>> grid, int region, bool colored)
    {
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
                    case 1: tilemap.SetTile(new Vector3Int(gx, gy, 0), tiles[0]); break;
                    case 2: tilemap.SetTile(new Vector3Int(gx, gy, 0), tiles[1]); break;
                    case 3: tilemap.SetTile(new Vector3Int(gx, gy, 0), tiles[2]); break;
                    case 4: tilemap.SetTile(new Vector3Int(gx, gy, 0), tiles[3]); break;
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
