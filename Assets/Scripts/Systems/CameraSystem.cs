using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraSystem : MonoBehaviour
{
    [Header("Camera Alignment Components")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private Tile[] Tiles;

    [Header("Room Indication Components")]
    [SerializeField] private Tilemap mapTilemap;
    [SerializeField] private GameObject playerDot;

    void Update()
    {
        // position of the player on Grid_Rooms
        Vector3Int cellPosition = grid.WorldToCell(player.transform.position);

        // if map open, move the player dot to the right cell, otherwise move the main camera with the player
        if (!GameData.mapActive)
        {
            //mainCamera.transform.position = grid.GetCellCenterWorld(cellPosition);

            if (mapTilemap.GetTile(cellPosition) == Tiles[0])
            {
                // single cell room
                Vector3 Pos = grid.GetCellCenterWorld(cellPosition);

                Pos.z = mainCamera.transform.position.z;
                
                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[1])
            {
                // top left corner room
                Vector3 Pos = player.transform.position;

                if (Pos.x < grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                if (Pos.y > grid.GetCellCenterWorld(cellPosition).y)
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[2])
            {
                // top middle room
                Vector3 Pos = player.transform.position;

                if (Pos.y > grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[3])
            {
                // top right corner room
                Vector3 Pos = player.transform.position;

                if (Pos.x > grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                if (Pos.y > grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[4])
            {
                // middle left room
                Vector3 Pos = player.transform.position;

                if (Pos.x < grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[5])
            {
                // center room
                Vector3 Pos = player.transform.position;

                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[6])
            {
                // middle right room
                Vector3 Pos = player.transform.position;

                if (Pos.x > grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[7])
            {
                // bottom left corner room
                Vector3 Pos = player.transform.position;

                if (Pos.x < grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                if (Pos.y < grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[8])
            {
                // bottom middle room
                Vector3 Pos = player.transform.position;

                if (Pos.y < grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[9])
            {
                // bottom right corner room
                Vector3 Pos = player.transform.position;

                if (Pos.x > grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                if (Pos.y < grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[10])
            {
                // vertical top room
                Vector3 Pos = player.transform.position;

                Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                if (Pos.y > grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[11])
            {
                // vertical center room
                Vector3 Pos = player.transform.position;

                Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[12])
            {
                // vertical bottom room
                Vector3 Pos = player.transform.position;

                Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                if (Pos.y < grid.GetCellCenterWorld(cellPosition).y) 
                    Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[13])
            {
                // horizontal left room
                Vector3 Pos = player.transform.position;

                if (Pos.x < grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[14])
            {
                // horizontal center room
                Vector3 Pos = player.transform.position;

                Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
            else if (mapTilemap.GetTile(cellPosition) == Tiles[15])
            {
                // horizontal right room
                Vector3 Pos = player.transform.position;

                if (Pos.x > grid.GetCellCenterWorld(cellPosition).x) 
                    Pos.x = grid.GetCellCenterWorld(cellPosition).x;
                Pos.y = grid.GetCellCenterWorld(cellPosition).y;
                Pos.z = mainCamera.transform.position.z;

                mainCamera.transform.position = Pos;
            }
        }
        else
        {
            playerDot.transform.position = mapTilemap.GetCellCenterWorld(cellPosition);
        }
    }
}
