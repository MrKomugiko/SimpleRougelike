using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DZIK_MONSTER : Monster_Cell
{
    public DZIK_MONSTER(CellScript parent, string name, string icon_Url, int maxHealthPoints, int speed, Pathfinding pathfinder = null) : base(parent, name, icon_Url, maxHealthPoints, speed, pathfinder)
    {
        Debug.Log("DZIK WKRACZA DO GRY");
    }
}
