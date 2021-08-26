using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle_Cell : ISpecialTile
{

    #region core
    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; private set; } = TileTypes.wall;
    public string Name { get; set; }
    public string Icon_Url { get; set; }
    #endregion
   
    #region Obstacle-specific
        public List<(Action action, string description,ActionIcon icon)> AvaiableActions { get; private set;} = new List<(Action action, string descriptio,ActionIcon iconn)>();
    #endregion
   

    public Obstacle_Cell(CellScript parent, string name, string icon_Url)
    {
        this.ParentCell = parent;
        this.Name = name;
        this.Type = TileTypes.treasure;
        this.Icon_Url = icon_Url;

        this.ParentCell.IsWalkable = false;
        //Debug.Log("pomyslnie utworzono pole typu Wall o nazwie"+icon_Url);;
    }
    public void OnClick_MakeAction()
    {
        Debug.Log($"click scianę");
    }
}
