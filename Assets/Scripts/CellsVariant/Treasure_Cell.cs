using System;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Cell : ISpecialTile, IValuable, ISelectable
{

    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public int ID {get;}
    public int GoldValue { get; set; }
    public GameObject Border { get; set; }
    public bool IsHighlighted { get; set; }
    public GameObject Icon_Sprite {get;set;}

    public List<(Action action, string description,ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string description,ActionIcon icon, bool singleAction)>();

    public IChest chest {get;} = null;
    public Treasure_Cell(CellScript parent, TreasureData _data)
    {
        this.ParentCell     =       parent;
        this.ID             =       _data.ID;
        this.Name           =       _data.TreasureName;
        this.Type           =       _data.Type;
        this.Icon_Sprite    =       _data.Icon_Sprite;
        this.GoldValue      =       _data.Value;

        if(_data.ListOfContainingItem.Count > 0)
        {
            chest = new Chest(source:this,_data.ListOfContainingItem);
        }

        AvaiableActions.Add((  ()=>{
            bool result;
            Pick(out result);
            if(result == false)
                {
                    NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Info, "Cannot pick, item is too far.");
                }
            } ,"Collect Only",ActionIcon.Pick,
            true));
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
            GameManager.instance.AddGold(chest==null?GoldValue:chest.TotalValue);
            ParentCell.MoveTo();
        }
    }
    public void Pick(out bool status)
    {
        if(GridManager.DistanceCheck(this))
        {
            if (Border != null)
            GameObject.Destroy(Border.gameObject);

            Debug.Log("pick");

            GameManager.instance.AddGold(chest==null?GoldValue:chest.TotalValue);

            ParentCell.SpecialTile = null;
            var currentPosition = ParentCell.CurrentPosition;
            GridManager.CellGridTable[ParentCell.CurrentPosition].SetCell(currentPosition);
            status = true;
        }
        else
        {
            status = false;
        }
    }
    public void RemoveBorder()
    {
        IsHighlighted = false;
        if (Border != null)
        {
            GameObject.Destroy(Border.gameObject);
        }
    }
}
