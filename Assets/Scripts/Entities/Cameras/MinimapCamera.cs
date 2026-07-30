using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinimapCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid roomGrid;
    [SerializeField] private Tilemap roomsTilemap; // invisible map  
    [SerializeField] private Tilemap roomsFoundTilemap; // visible map 
    [SerializeField] private Tilemap doorsTilemap;
    [SerializeField] private Tilemap doorsFoundTilemap;
    [SerializeField] private GameObject player;
    [SerializeField] private Tilemap mapTilemap;
    [SerializeField] private GameObject playerDot;

    [Header("Settings")]
    [SerializeField] private float smoothingSpeed;



    private Vector3Int cellPosition;
    private Vector3Int previousCellPosition;

    private Vector3 minimapTargetPosition;
    private Coroutine minimapMoveCoroutine;

    void Start()
    {
        cellPosition = roomGrid.WorldToCell(player.transform.position);
        previousCellPosition = cellPosition;

        // get new target position for the minimap
        Vector3 newTarget = mapTilemap.GetCellCenterWorld(cellPosition);
        playerDot.transform.position = newTarget;

        // preserve z value of camera so minimap doesn't go blank
        minimapTargetPosition = newTarget;
        minimapTargetPosition.z = transform.position.z;

        StartCoroutine(MoveMinimapCamera());
    }

    void Update()
    {
        // position of the player on Grid_Rooms
        cellPosition = roomGrid.WorldToCell(player.transform.position);

        if (cellPosition != previousCellPosition)
        {
            // get new target position for the minimap
            Vector3 newTarget = mapTilemap.GetCellCenterWorld(cellPosition);
            playerDot.transform.position = newTarget;

            // update the visible tiles map
            TileBase roomTile = roomsTilemap.GetTile(cellPosition);
            roomsFoundTilemap.SetTile(cellPosition, roomTile);
            TileBase doorTile = doorsTilemap.GetTile(cellPosition);
            doorsFoundTilemap.SetTile(cellPosition, doorTile);

            // preserve z value of camera so minimap doesn't go blank
            minimapTargetPosition = newTarget;
            minimapTargetPosition.z = transform.position.z;

            // lerp minimap camera to focus on new position if not already running
            if (minimapMoveCoroutine == null)
                minimapMoveCoroutine = StartCoroutine(MoveMinimapCamera());
        }

        previousCellPosition = cellPosition;
    }

    private IEnumerator MoveMinimapCamera()
    {
        while (true)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                minimapTargetPosition,
                smoothingSpeed * Time.deltaTime
            );

            // if it's close enough, set the position exact
            if (Vector3.Distance(transform.position,minimapTargetPosition) < 0.01f)
                transform.position = minimapTargetPosition;

            yield return null;
        }
    }
}
