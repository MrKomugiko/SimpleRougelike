using UnityEngine;

public interface ISpecialTile
{
    TileTypes Type {get;}
    CellScript ParentCell { get; }
    string Name {get;set;}
    string Icon_Url {get;set;}

    void OnClick_MakeAction();

}