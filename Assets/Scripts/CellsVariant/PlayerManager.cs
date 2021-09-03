using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerManager: MonoBehaviour
{

    public TextMeshProUGUI GoldCounter_TMP;
    public TextMeshProUGUI ExperienceCounter_TMP;
    public TextMeshProUGUI HealthCounter_TMP;
    public int Level = 1;
    public int Gold = 0;
    public int Experience = 0;
    public int Strength = 1;
    public int Inteligence = 1;
    public int Dexterity = 1;
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public EquipmentScript _EquipedItems;
    public static PlayerManager instance;
    public NotificationScript _notificationScript;
    public ActionSwitchController _actionController;

    public void SetPlayerManager(Player_Cell parentCell)
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
    

    public void AddGold(int value)
    {
        int oldGoldvalue = Int32.Parse(GoldCounter_TMP.text);
        Gold = oldGoldvalue + value;
        GoldCounter_TMP.SetText(Gold.ToString());
        NotificationManger.AddValueTo_Gold_Notification(value);
    }
}