using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Chest;

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

    public TreasureBackupData TreasureData_Backup_DATA;

    public IChest chest {get;} = null;
    public Treasure_Cell(CellScript parent, TreasureData _data, TreasureBackupData _restoredData = null)
    {
        this.ParentCell             =       parent;
        this.ID                     =       _data.ID;
        this.Name                   =       _data.TreasureName;
        this.Type                   =       _data.Type;
        this.Icon_Sprite            =       _data.Icon_Sprite;
        this.GoldValue              =       _data.GuarantedGoldReward;
        this.RandomGeneratedLoot    =       _restoredData==null?_data.GetRandomizeLootPacks():_restoredData.RestoredItemsContent;

        if(_restoredData == null)
        {
            var goldReward = RandomGeneratedLoot.Where(item=>item.item is GoldItem).FirstOrDefault();
            if(goldReward == null)
            {
                RandomGeneratedLoot.Add(new ItemPack(GoldValue,_data.PossibleLootItems.Where(item=>item is GoldItem).First()));
            }
        }

        var treasureObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        
        if(RandomGeneratedLoot.Count==1)
        {
            treasureObject.GetComponent<SpriteRenderer>().sprite = RandomGeneratedLoot[0].item.ItemCoreSettings.Item_Sprite;
        }
        else
        {
            treasureObject.GetComponent<SpriteRenderer>().sprite = this.Icon_Sprite.GetComponent<SpriteRenderer>().sprite;
            chest = new Chest(source:this,RandomGeneratedLoot);
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
        
        ParentCell.Trash.Add(treasureObject);
        RemoveFromMapIfChesIsEmpty();
    }
    
    public object SaveAndGetCellProgressData()
    {   
        if(chest!= null)
        {
            TreasureBackupData savedValues = new TreasureBackupData(this.ID, chest.ContentItems);
            return savedValues;
        }

        return new TreasureBackupData(this.ID, RandomGeneratedLoot);
    }

    public void OnClick_MakeAction()
    {
        Vector2Int direction = GameManager.Player_CELL.CurrentPosition - this.ParentCell.CurrentPosition;

        if(direction.x == 0)
            GameManager.LastPlayerDirection = direction.y<0?"Back":"Front";
        
        if(direction.y == 0)
            GameManager.LastPlayerDirection = direction.x<0?"Right":"Left";

        if(chest != null){
            if(chest.ContentItems.Count>0){
                chest.GenerateChestLootWindowPopulatedWithItems(chest,chest.ContentItems);
            }
        }
        else
            MoveAndPick();       
    }
    public void MoveAndPick()
    {
        bool status;
        Pick(out status);
        if(status == true)
        {
            ParentCell.MoveTo();
        }
    }
    public void RemoveFromMapIfChesIsEmpty()
    {
        if(chest == null) return;
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
