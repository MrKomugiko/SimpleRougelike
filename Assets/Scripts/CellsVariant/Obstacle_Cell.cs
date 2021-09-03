using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle_Cell : ISpecialTile
{

    #region core
    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; set; } = TileTypes.wall;
    public string Name { get; set; }
    
    public GameObject Icon_Sprite { get; set; }
    

    #endregion
   
    #region Obstacle-specific
        public List<(Action action, string description,ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string descriptio,ActionIcon iconn, bool singleAction)>();
    #endregion


    public Obstacle_Cell(CellScript parent, string name, string icon_Url)
    {
        this.ParentCell = parent;
        this.Name = name;
        this.Type = TileTypes.treasure;
        this.Icon_Sprite = GameManager.instance.WALLSPRITE;

        this.ParentCell.IsWalkable = false;
        //Debug.Log("pomyslnie utworzono pole typu Wall o nazwie"+icon_Url);;
    
        var bombObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(bombObject);
    }
    public void OnClick_MakeAction()
    {
        Debug.Log($"click scianę");
    }
}
