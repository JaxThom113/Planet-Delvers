using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class CameraSystem : Singleton<CameraSystem>
{
    [Header("Camera Alignment Components")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private Tile[] Tiles;

    [Header("Room Indication Components")]
    [SerializeField] private Tilemap mapTilemap;
    [SerializeField] private GameObject playerDot;

    [Header("Minimap")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private float smoothingSpeed;

    private Vector3Int cellPosition;
    private Vector3Int previousCellPosition;

    void Start()
    {
        cellPosition = grid.WorldToCell(player.transform.position);
        previousCellPosition = cellPosition;

        playerDot.transform.position = mapTilemap.GetCellCenterWorld(cellPosition);
        StartCoroutine(MoveMinimapCamera(mapTilemap.GetCellCenterWorld(cellPosition)));
    }

    void Update()
    {
        // position of the player on Grid_Rooms
        cellPosition = grid.WorldToCell(player.transform.position);

        if (mapTilemap.GetTile(cellPosition) == Tiles[0])
        {
            // single cell room
            Vector3 playerPos = grid.GetCellCenterWorld(cellPosition);

            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[1])
        {
            // top left corner room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x < grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            if (playerPos.y > grid.GetCellCenterWorld(cellPosition).y)
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[2])
        {
            // top middle room
            Vector3 playerPos = player.transform.position;

            if (playerPos.y > grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[3])
        {
            // top right corner room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x > grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            if (playerPos.y > grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[4])
        {
            // middle left room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x < grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[5])
        {
            // center room
            Vector3 playerPos = player.transform.position;

            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[6])
        {
            // middle right room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x > grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[7])
        {
            // bottom left corner room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x < grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            if (playerPos.y < grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[8])
        {
            // bottom middle room
            Vector3 playerPos = player.transform.position;

            if (playerPos.y < grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[9])
        {
            // bottom right corner room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x > grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            if (playerPos.y < grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[10])
        {
            // vertical top room
            Vector3 playerPos = player.transform.position;

            playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            if (playerPos.y > grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[11])
        {
            // vertical center room
            Vector3 playerPos = player.transform.position;

            playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[12])
        {
            // vertical bottom room
            Vector3 playerPos = player.transform.position;

            playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            if (playerPos.y < grid.GetCellCenterWorld(cellPosition).y) 
                playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[13])
        {
            // horizontal left room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x < grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[14])
        {
            // horizontal center room
            Vector3 playerPos = player.transform.position;

            playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }
        else if (mapTilemap.GetTile(cellPosition) == Tiles[15])
        {
            // horizontal right room
            Vector3 playerPos = player.transform.position;

            if (playerPos.x > grid.GetCellCenterWorld(cellPosition).x) 
                playerPos.x = grid.GetCellCenterWorld(cellPosition).x;
            playerPos.y = grid.GetCellCenterWorld(cellPosition).y;
            playerPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = playerPos;
        }

        if (cellPosition != previousCellPosition)
        {
            // move the player dot on the map
            playerDot.transform.position = mapTilemap.GetCellCenterWorld(cellPosition);

            // lerp minimap camera to focus on new position
            StartCoroutine(MoveMinimapCamera(mapTilemap.GetCellCenterWorld(cellPosition)));
        }

        previousCellPosition = cellPosition;
    }

    private IEnumerator MoveMinimapCamera(Vector3 targetPosition)
    {
        // preserve z value of camera so minimap doesn't go blank
        targetPosition.z = minimapCamera.transform.position.z;

        // lerp until minimap camera is close enough to target position
        while (Vector3.Distance(minimapCamera.transform.position, targetPosition) > 0.01f)
        {
            minimapCamera.transform.position = Vector3.Lerp(
                minimapCamera.transform.position,
                targetPosition,
                smoothingSpeed * Time.deltaTime
            );
            yield return null;
        }

        // set the position exact to target
        minimapCamera.transform.position = targetPosition;
    }
}
