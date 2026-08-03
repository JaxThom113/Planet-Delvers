using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : PersistentSingleton<GameSystem>
{
    [Header("Mission Data")]
	[SerializeField] public int seed;
	[SerializeField] public int size;
	[SerializeField] public int level;

    public void InitializeData()
    {
        seed = 0;
        size = 1;  // default to normal size
        level = 1; // default to medium level
    }
}
