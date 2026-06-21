using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : PersistentSingleton<GameSystem>
{
    [Header("Mission Data")]
	[SerializeField] public int seed;
	[SerializeField] public int length;
	[SerializeField] public int level;

    public void InitializeData()
    {
        seed = 0;
        length = 0;
        level = 0;
    }
}
