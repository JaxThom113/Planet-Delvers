using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "ScriptableObjects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("Enemy Data")]
    public int baseHealth;
    public int baseDamage;

    [Header("Item Drops")]
    public GameObject healthCell;
    public int healthCellCount;
    public bool healthCellCountMaxRandom;

    public GameObject energyCell;
    public int energyCellCount;
    public bool energyCellCountMaxRandom;

    public List<GameObject> specialDrops;
    public bool pickRandomSpecial;
}