using System;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Cell : ISpecialTile, IValuable, ISelectable
{

    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; set; } 
    public string Name { get; set; }
    public int ID {get;}
    public int GoldValue { get; set; }


    public GameObject Border { get; set; }
    public bool IsHighlighted { get; set; }
    public GameObject Icon_Sprite {get;set;}

    public List<(Action action, string description,ActionIcon icon, bool singleAction)> AvaiableActions { get; private set;} = new List<(Action action, string description,ActionIcon icon, bool singleAction)>();

    private List<Chest.ItemPack> RandomGeneratedLoot = new List<Chest.ItemPack>();
    public IChest chest {get;} = null;
    public Treasure_Cell(CellScript parent, TreasureData _data)
    {
        this.ParentCell     =       parent;
        this.ID             =       _data.ID;
        this.Name           =       _data.TreasureName;
        this.Type           =       _data.Type;
        this.Icon_Sprite    =       _data.Icon_Sprite;
        this.GoldValue      =       _data.Value;
        this.RandomGeneratedLoot = _data.GetRandomizeLootPacks();

        chest = new Chest(source:this,RandomGeneratedLoot);

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
        RemoveFromMapIfChesIsEmpty();
    }
    public void OnClick_MakeAction()
    {
        Vector2Int direction = GameManager.Player_CELL.CurrentPosition - this.ParentCell.CurrentPosition;
        Debug.Log(direction);

        if(direction.x == 0)
            GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
        
        if(direction.y == 0)
            GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";
        PlayerManager.instance.GraphicSwitch.UpdatePlayerGraphics();

        if(chest.ContentItems.Count>0){
            chest.GenerateChestLootWindowPopulatedWithItems(chest,chest.ContentItems);
        }
        else
            MoveAndPick();       
    }
    public void MoveAndPick()
    {
        bool status;
        Pick(out status);
        ParentCell.MoveTo();
    }
    public void RemoveFromMapIfChesIsEmpty()
    {
        if(chest.ContentItems.Count == 0)
        {
            if(GridManager.CellGridTable.ContainsKey(ParentCell.CurrentPosition))
            {
                if (Border != null)
                GameObject.Destroy(Border.gameObject);

                ParentCell.SpecialTile = null;
                var currentPosition = ParentCell.CurrentPosition;
                GridManager.CellGridTable[ParentCell.CurrentPosition].SetCell(currentPosition);
                return;
            }
        }
    }
    public void Pick(out bool status)
    {
        if(GridManager.DistanceCheck(this))
        {
            if (Border != null)
            GameObject.Destroy(Border.gameObject);

            PlayerManager.instance.AddGold(chest==null?GoldValue:chest.TotalValue);

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
