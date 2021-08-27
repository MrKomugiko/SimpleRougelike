using System;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Cell : ISpecialTile, IValuable, ISelectable
{

    #region core
    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public string Icon_Url { get; set; }

    #endregion

    #region Treasure-specific
    public int ID {get;}
    public int GoldValue { get; set; }
    public GameObject Border { get; set; }
    public bool IsHighlighted { get; set; }
    public GameObject Icon_Sprite {get;set;}

    public List<(Action action, string description,ActionIcon icon)> AvaiableActions { get; private set;} = new List<(Action action, string description,ActionIcon icon)>();
    #endregion


    public Treasure_Cell(CellScript parent, TreasureData _data)
    {
        this.ParentCell     =       parent;
        this.ID             =       _data.ID;
        this.Name           =       _data.TreasureName;
        this.Type           =       _data.Type;
        this.Icon_Sprite    =       _data.Icon_Sprite;
        this.GoldValue      =       _data.Value;

        AvaiableActions.Add((  ()=>{
            bool result;
            Pick(out result);
            if(result == false)
                {
                    NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Info, "Cannot pick, item is too far.");
                }
            } ,"Collect Only",ActionIcon.Pick));
        NotificationManger.CreateNewNotificationElement(this);
    
        var treasureObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(treasureObject);
    }
    public void OnClick_MakeAction()
    {
        MoveAndPick();       
    }
    public void MoveAndPick()
    {
        if(Vector3Int.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition, (Vector3Int)this.ParentCell.CurrentPosition) < 1.1f)
        {
            GameManager.instance.AddGold(GoldValue);
                GoldValue = 0;
            ParentCell.MoveTo();
        }
    }
    public void Pick(out bool status)
    {

       if(Vector3Int.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition, (Vector3Int)this.ParentCell.CurrentPosition) < 1.1f)
       {    
           if(Border != null)
                GameObject.Destroy(Border.gameObject);
    
            Debug.Log("pick");
            GameManager.instance.AddGold(GoldValue);
                GoldValue = 0;
            
            ParentCell.SpecialTile = null;
            var currentPosition = ParentCell.CurrentPosition;
            GridManager.CellGridTable[ParentCell.CurrentPosition].SetCell(currentPosition);
            status = true;
       }
       else
       {
        status = false;
        Debug.LogWarning("za daleko"+Vector3Int.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition, (Vector3Int)this.ParentCell.CurrentPosition));
       }
    }
}
