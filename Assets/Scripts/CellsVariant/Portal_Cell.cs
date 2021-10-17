using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DungeonRoomScript;

public class Portal_Cell : ISpecialTile
{
    public int portalId;
    public string CurrentStatus = "open";
    public Portal_Cell(CellScript parentCell, PortalData data, Room enter, Room exit)
    {
        portalId = data.ID;
        CurrentStatus = data.PortalStatus;
        ParentCell = parentCell;
        Name = data.name;
        Icon_Sprite = data.Icon_Sprite;
        Type = parentCell.Type;
        parentCell.isWalkable = false;

        var portalObject = GameObject.Instantiate(Icon_Sprite, ParentCell.transform);
        ParentCell.Trash.Add(portalObject);

        enterLocation = enter;
        exitLocation = exit;
    }

    public Room enterLocation;
    public Room exitLocation;

    public CellScript ParentCell { get; private set; }
    public string Name {get; set;} = "Portal";
    public GameObject Icon_Sprite {get; set;}
    public TileTypes Type {get; set;} = TileTypes.portal;

    public List<(Action action, string description, ActionIcon icon, bool singleAction)> AvaiableActions {get;set;} = null;

    public void OnClick_MakeAction()
    {
        Debug.Log("przenieś gracza na 2gi koniec teleportu");
        if(DungeonManager.instance.CurrentLocation.ToString() == enterLocation.position)
        {
            DungeonManager.instance.TeleportTo(from: enterLocation ,to:exitLocation);
            return;
        }
        if(DungeonManager.instance.CurrentLocation.ToString() == exitLocation.position)
        {
            DungeonManager.instance.TeleportTo(from: exitLocation ,to:enterLocation);
            return;
        }
    }

    public object SaveAndGetCellProgressData()
    {
        PortalBackupData savedValues = new PortalBackupData(portalId,CurrentStatus,enterLocation.position,exitLocation.position);
        return savedValues;
    }
}
