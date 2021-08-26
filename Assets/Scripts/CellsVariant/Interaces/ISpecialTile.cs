using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpecialTile
{
    TileTypes Type {get;}
    CellScript ParentCell { get; }
    string Name {get;set;}
    string Icon_Url {get;set;}

    List<(Action action,string description)> AvaiableActions {get;}

    void OnClick_MakeAction();

}