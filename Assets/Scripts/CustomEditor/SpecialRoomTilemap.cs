using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SpecialRoomTilemap : RoomTilemap
{
    // dimensions of this room
    [SerializeField] public string roomName;
}