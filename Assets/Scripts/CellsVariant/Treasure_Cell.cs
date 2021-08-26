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

    public List<(Action action, string description)> AvaiableActions { get; private set;} = new List<(Action action, string description)>();
    #endregion


    public Treasure_Cell(CellScript parent, string name, string icon_Url, int goldValue)
    {
        this.ParentCell = parent;
        this.Name = name;
        this.Type = TileTypes.treasure;
        this.Icon_Url = icon_Url;
        this.GoldValue = goldValue;

        //Debug.Log("pomyslnie utworzono pole typu treasure o nazwie"+icon_Url);
        AvaiableActions.Add((()=>Pick(),"Collect Only"));
        NotificationManger.CreateNewNotificationElement(this);
    }
    public void OnClick_MakeAction()
    {
        Pick();
        ParentCell.MoveTo();    //TODO: hmm

    }

    public void Pick()
    {
       if(Vector3Int.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition, (Vector3Int)this.ParentCell.CurrentPosition) < 1.1f)
       {    
           if(Border != null)
                GameObject.Destroy(Border.gameObject);
    
            Debug.Log("pick");
            GameManager.instance.AddGold(GoldValue);
            ParentCell.SpecialTile = null;
       }
       else
       {

        Debug.LogWarning("za daleko"+Vector3Int.Distance((Vector3Int)GameManager.Player_CELL.CurrentPosition, (Vector3Int)this.ParentCell.CurrentPosition));
       }
    }

}
