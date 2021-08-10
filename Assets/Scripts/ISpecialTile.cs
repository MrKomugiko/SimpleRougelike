using UnityEngine;

public interface ISpecialTile
{
    Vector2Int Position {get;set;}
    TileTypes Type {get;set;}
    string Name {get;set;}
    string Effect {get;set;}
    string Icon {get;set;}
    
    void MakeAction();
    void SpawnOnMap();
}