using System;
using UnityEngine;

[CreateAssetMenu(fileName="New Monster",menuName="Monster")]
public class MonsterData : ScriptableObject
{
    public string MonsterName; 
    public GameObject Icon_Sprite;
    public GameObject Corpse_Sprite;
    public int MaxHealthPoints; 
    public int Speed;
    public int LootID;
    public int Damage;
    public int Level;
    public int ExperiencePoints;
    public TileTypes Type = TileTypes.monster;
    public bool IsWalkable = true;
    public bool IsPathfinderRequired = true;
}
