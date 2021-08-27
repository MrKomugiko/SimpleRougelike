using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpecialTile
{
    TileTypes Type {get;}
    CellScript ParentCell { get; }
    string Name {get;set;}
    GameObject Icon_Sprite {get;set;}
    List<(Action action,string description, ActionIcon icon)> AvaiableActions {get;}
    void OnClick_MakeAction();
}