using UnityEngine;

public interface ISpecialTile
{
    TileTypes Type {get;}
    string Name {get;set;}
    string Effect_Url {get;set;}
    string Icon_Url {get;set;}
    CellScript ParentCell { get; }
    bool Active { get; set; }
    bool IsReadyToUse { get; }
    void OnClick_MakeAction();

}