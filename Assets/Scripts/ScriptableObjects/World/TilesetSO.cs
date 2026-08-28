using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TilesetSO", menuName = "ScriptableObjects/World/TilesetSO")]
public class TilesetSO : ScriptableObject
{
    [Header("Tileset Settings")]
    public Color32[] colors;

    // fg, bg, hazard, and entity tilemaps for a room size
    [Header("Tile Types")]
    public TiletypeSO type1Tiles;
    public TiletypeSO type2Tiles;
    public TiletypeSO type3Tiles;
    public TiletypeSO type4Tiles;
}