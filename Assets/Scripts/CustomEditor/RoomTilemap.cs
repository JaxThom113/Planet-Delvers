using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class RoomTilemap
{
    // dimensions of this room
    [SerializeField] public Vector2Int dims;

    // mark true for if you want to save this tilemap as csv
    [SerializeField] public bool write;

    // fg, bg, hazard, and entity tilemaps for a room size
    [SerializeField] public Tilemap fg;
    [SerializeField] public Tilemap bg;
    [SerializeField] public Tilemap hazard;
    [SerializeField] public Tilemap entity;
}