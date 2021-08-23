using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Cell : ISpecialTile, IValuable
{

    #region core
    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public string Icon_Url { get; set; }

    #endregion

    #region Treasure-specific
    public int GoldValue { get; set; }
    #endregion


    public Treasure_Cell(CellScript parent, string name, string icon_Url, int goldValue)
    {
        this.ParentCell = parent;
        this.Name = name;
        this.Type = TileTypes.treasure;
        this.Icon_Url = icon_Url;

        this.GoldValue = goldValue;

        Debug.Log("pomyslnie utworzono pole typu treasure o nazwie"+icon_Url);;
    }
    public void OnClick_MakeAction()
    {
        ParentCell.MoveTo();
        Pick();
    }

    public void Pick()
    {
        Debug.Log($"zbierasz {GoldValue} monet");
        GameManager.instance.AddGold(GoldValue);
    }

}
