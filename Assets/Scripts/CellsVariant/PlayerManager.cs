using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager
{
    
    static public int Level = 1;
    static public int Strength = 1;
    static public int Inteligence = 1;
    static public int Dexterity = 1;
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public static PlayerManager instance;
    internal NotificationScript _notificationScript;

    public ActionSwitchController _actionController;

    public PlayerManager(Player_Cell parentCell)
    {
        _playerCell = parentCell;
        _mainBackpack = GameObject.Find("Content_EquipmentTab").GetComponent<EquipmentScript>();
        instance = this;
    }
    public void Reset_QuickSlotToDefault(int quickslotID)
    {
        Debug.Log("reset to default action button id: "+quickslotID);
        Action defaultAction = () => 
        {
            Debug.Log($"uruchomienie [EquipmentScript.AssignItemToActionSlot({quickslotID})]");
            EquipmentScript.AssignItemToActionSlot(quickslotID);
        };
        _actionController.actionButtonsList[quickslotID].ButtonIcon_IMG.sprite = _actionController.actionButtonsList[quickslotID].IconSpriteList
            .First(n=>n.name == "ICON_"+ActionIcon.Empty.ToString());
            
        _actionController.actionButtonsList[quickslotID].ConfigureDescriptionButtonClick(
                    action: ()=>defaultAction(),
                    description: "Empty Slot.",
                    singleAction: false,
                    actionNameString: "reset to default"
                );
        EquipmentScript.AssignationItemToQuickSlotIsActive = false;
        EquipmentScript.CurrentSelectedActionButton = null;
    }
    public void Reset_QuickSlotToDefault(ActionButtonScript btn)
    {
        int quickslotID = _actionController.actionButtonsList.IndexOf(btn);
        Action defaultAction = () => 
        {
            EquipmentScript.AssignItemToActionSlot(quickslotID);
        };
        _actionController.actionButtonsList[quickslotID].ButtonIcon_IMG.sprite = _actionController.actionButtonsList[quickslotID].IconSpriteList
            .First(n=>n.name == "ICON_"+ActionIcon.Empty.ToString());
            
        _actionController.actionButtonsList[quickslotID].ConfigureDescriptionButtonClick(
                    action: ()=>defaultAction(),
                    description: "Empty Slot.",
                    singleAction: false,
                    actionNameString: "reset to default"
                );
        EquipmentScript.AssignationItemToQuickSlotIsActive = false;
        EquipmentScript.CurrentSelectedActionButton = null;
    }
    public void QuickSlot_CustomActionForButton(int SlotID)
    {
        Debug.LogWarning("Button 0 method");
    }
    
}