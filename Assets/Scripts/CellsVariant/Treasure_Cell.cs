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
    public int GoldValue { get; set; }
    public GameObject Border { get; set; }
    public bool IsHighlighted { get; set; }

    public List<(Action action, string description,ActionIcon icon)> AvaiableActions { get; private set;} = new List<(Action action, string description,ActionIcon icon)>();
    #endregion


    public Treasure_Cell(CellScript parent, string name, string icon_Url, int goldValue)
    {
        this.ParentCell = parent;
        this.Name = name;
        this.Type = TileTypes.treasure;
        this.Icon_Url = icon_Url;
        this.GoldValue = goldValue;

        //Debug.Log("pomyslnie utworzono pole typu treasure o nazwie"+icon_Url);
        // TODO: jakos bym to musial wynieść z klasy treasure do obsługi wątków dla powiadomień
        AvaiableActions.Add((
            ()=>{
            bool result;
            Pick(out result);
            if(result == false)
                {
                    NotificationManger.TriggerActionNotification(this,NotificationManger.AlertCategory.Info, "Cannot pick, item is too far.");
                }
            }
            ,"Collect Only",ActionIcon.Pick));
        NotificationManger.CreateNewNotificationElement(this);
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
