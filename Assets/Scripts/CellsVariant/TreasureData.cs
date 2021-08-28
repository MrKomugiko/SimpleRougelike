using UnityEngine;

[CreateAssetMenu(fileName="New Treasure",menuName="GameData/Treasure")]
public class TreasureData : ScriptableObject
{
    public int ID;
    public string TreasureName; 
    public GameObject Icon_Sprite;
    public int Value;
    public TileTypes Type = TileTypes.treasure;
    public bool IsWalkable = true;
}
