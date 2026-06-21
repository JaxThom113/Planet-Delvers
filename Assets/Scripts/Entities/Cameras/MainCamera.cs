using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid roomGrid;
    [SerializeField] private GameObject player;

    public Vector3 playerPos;
    private Vector3Int cellPosition;

    private bool cameraSnapping;

    void Start()
    {
        cellPosition = roomGrid.WorldToCell(player.transform.position);
        cameraSnapping = true;
    }

    void Update()
    {
        // position of the player on Grid_Rooms
        cellPosition = roomGrid.WorldToCell(player.transform.position);

        playerPos = Vector3.zero;

        if (cameraSnapping)
        {
            // use bitmask to figure out what type of type of room the player is in and move the camera accordingly
            int connectionsMask = 0;
            if (MapGen.MapGrid[FlipY(cellPosition.y)][cellPosition.x].connections[0]) connectionsMask |= 1; // up
            if (MapGen.MapGrid[FlipY(cellPosition.y)][cellPosition.x].connections[1]) connectionsMask |= 2; // down
            if (MapGen.MapGrid[FlipY(cellPosition.y)][cellPosition.x].connections[2]) connectionsMask |= 4; // left
            if (MapGen.MapGrid[FlipY(cellPosition.y)][cellPosition.x].connections[3]) connectionsMask |= 8; // right

            switch (connectionsMask)
            {
                case 0: // ---- single room
                    playerPos = roomGrid.GetCellCenterWorld(cellPosition);

                    playerPos.z = transform.position.z;

                    transform.position = playerPos;
                    
                    break;
                case 1: // U--- vertical bottom
                    playerPos = player.transform.position;

                    playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    if (playerPos.y < roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 2: // -D-- vertical top
                    playerPos = player.transform.position;

                    playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    if (playerPos.y > roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 3: // UD-- vertical middle
                    playerPos = player.transform.position;

                    playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 4: // --L- horizontal right
                    playerPos = player.transform.position;

                    if (playerPos.x > roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 5: // U-L- bottom right
                    playerPos = player.transform.position;

                    if (playerPos.x > roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    if (playerPos.y < roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 6: // -DL- top right 
                    playerPos = player.transform.position;

                    if (playerPos.x > roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    if (playerPos.y > roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 7: // UDL- middle right 
                    playerPos = player.transform.position;

                    if (playerPos.x > roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 8: // ---R horizontal left
                    playerPos = player.transform.position;

                    if (playerPos.x < roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 9: // U--R bottom left 
                    playerPos = player.transform.position;

                    if (playerPos.x < roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    if (playerPos.y < roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 10: // -D-R top left
                    playerPos = player.transform.position;

                    if (playerPos.x < roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    if (playerPos.y > roomGrid.GetCellCenterWorld(cellPosition).y)
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 11: // UD-R middle left
                    playerPos = player.transform.position;

                    if (playerPos.x < roomGrid.GetCellCenterWorld(cellPosition).x) 
                        playerPos.x = roomGrid.GetCellCenterWorld(cellPosition).x;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 12: // --LR horizontal middle
                    playerPos = player.transform.position;

                    playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 13: // U-LR bottom middle
                    playerPos = player.transform.position;

                    if (playerPos.y < roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 14: // -DLR top middle
                    playerPos = player.transform.position;

                    if (playerPos.y > roomGrid.GetCellCenterWorld(cellPosition).y) 
                        playerPos.y = roomGrid.GetCellCenterWorld(cellPosition).y;
                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
                case 15: // UDLR middle center
                    playerPos = player.transform.position;

                    playerPos.z = transform.position.z;

                    transform.position = playerPos;

                    break;
            }
        } 
    }

    public void ToggleSnapping(bool snapped)
    {
        cameraSnapping = snapped;
    }

    private int FlipY(int y)
    {
        return (MapGen.GridSize - 1) - y;
    }
}
