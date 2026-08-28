using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TiletypeSO", menuName = "ScriptableObjects/World/TiletypeSO")]
public class TiletypeSO : ScriptableObject
{
    [Header("Tile Type Settings")]
    public Tile[] tiles;
    public bool colored;
}