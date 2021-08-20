using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Cell : ISpecialTile
{
    public CellScript ParentCell { get; private set; }
    public TileTypes Type { get; private set; } 
    public string Name { get; set; }
    public string Effect_Url { get; set; }
    public string Icon_Url { get; set; }

    public int GoldValue { get; set; }
    public bool Active { get; set ; } = false;

    public bool IsReadyToUse => true;

    public Treasure_Cell(CellScript parent, string name, string icon_Url, int goldValue, string effect_Url = "")
    {
        this.ParentCell = parent;
        this.Name = name;
        this.Type = TileTypes.treasure;
        this.Effect_Url = effect_Url;
        this.Icon_Url = icon_Url;

        this.GoldValue = goldValue;

        Debug.Log("pomyslnie utworzono pole typu treasure");
    }
    public void MakeAction()
    {
        Debug.Log($"zbierasz {GoldValue} monet");
        GameManager.instance.AddGold(GoldValue);
        ParentCell.MoveTo();
    }


}
