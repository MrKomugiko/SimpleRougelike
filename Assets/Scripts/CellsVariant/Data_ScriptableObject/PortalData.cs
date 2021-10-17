using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Chest;
using static Treasure_Cell;
using static DungeonRoomScript;

[CreateAssetMenu(fileName="New Portal",menuName="GameData/Special/Portal")]
public class PortalData : ScriptableObject
{
    public int ID;
    public GameObject Icon_Sprite;
    public TileTypes Type = TileTypes.portal;
    public bool IsWalkable = false;

    public string PortalType = "standard";
    public string PortalStatus = "open";
    public Vector2Int StartLocation;
    public Vector2Int EndLocation;
}



