using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateMap : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool revealMap; 

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap roomsTilemap; // hidden map used to guide camera
    [SerializeField] private Tilemap doorsTilemap;
    [SerializeField] private Tilemap roomsFoundTilemap; // visible map showing discovered rooms
    [SerializeField] private Tilemap doorsFoundTilemap;

    [Header("Tiles")]
    [SerializeField] private Tile bgTile;
    [SerializeField] private Tile[] r1Tiles;
    [SerializeField] private Tile[] r2Tiles;
    [SerializeField] private Tile[] r3Tiles;
    [SerializeField] private Tile[] r4Tiles;
    [SerializeField] private Tile[] doorTiles;

    public void SetupMap()
    {
        // Step #1: Draw background tiles
        DrawBackgroundTiles();

        // Step #2: Draw connected room tiles to guide camera,  keep POIs visible
        DrawRoomTiles();

        // Step #3: Draw door tiles
        DrawDoorTiles();
    }

    private void DrawBackgroundTiles()
    {
        for (int y = 0; y < MapGen.GridSize; y++)
        {
            for (int x = 0; x < MapGen.GridSize; x++)
            {
                // make black grid background
                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), bgTile);
            }
        }
    }

    private void DrawRoomTiles()
    {
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
                        roomsFoundTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[16]);
                        roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[16]);
                        continue;
                    }
                    else if (name.Contains("Boss"))
                    {
                        roomsFoundTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[17]);
                        roomsTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[17]);
                        continue;
                    }
                    else if (name.Contains("Item"))
                    {
                        roomsFoundTilemap.SetTile(new Vector3Int(x, FlipY(y), 0), currentTileSet[18]);
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

        // make the world map visible
        if (revealMap)
            roomsTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = true;
    }

    private void DrawDoorTiles()
    {
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

        // make the world map visible
        if (revealMap)
            doorsTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = true;
    }

    /*
        Helper functions
    */

    private int FlipY(int y)
    {
        return (MapGen.GridSize - 1) - y;
    }
}
