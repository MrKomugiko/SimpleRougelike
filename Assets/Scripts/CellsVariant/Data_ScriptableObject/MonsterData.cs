using UnityEngine;

[CreateAssetMenu(fileName="New Monster",menuName="GameData/Monster")]
public class MonsterData : ScriptableObject
{
    public int ID;
    public string MonsterName; 
    public GameObject Icon_Sprite;
    public GameObject Corpse_Sprite;
    public int MaxHealthPoints; 
    public int CurrentHealthPoints;
    public int Speed;
    public TreasureData LootData;
    public int Damage;
    public int Level;
    public int ExperiencePoints;
    public TileTypes Type = TileTypes.monster;
    public bool IsWalkable = true;
    public bool IsPathfinderRequired = true;
}
