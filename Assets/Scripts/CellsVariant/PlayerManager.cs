using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager: MonoBehaviour
{
    public Image HelmetIMG;
    public Image ArmorIMG;
     [SerializeField] GameObject GraphicSwitchPrefab;
    [SerializeField] public PlayerEquipmentVisualSwitchScript GraphicSwitch;

    public TextMeshProUGUI GoldCounter_TMP;
    public TextMeshProUGUI ExperienceCounter_TMP;
    public TextMeshProUGUI HealthCounter_TMP;
    public int Level = 1, Gold = 0, Experience = 0, Strength = 1, Inteligence = 1, Dexterity = 1;
    public Player_Cell _playerCell;
    public EquipmentScript _mainBackpack;
    public EquipmentScript _EquipedItems;
    public static PlayerManager instance;
    public NotificationScript _notificationScript;
    public ActionSwitchController _actionController;

    public void SetPlayerManager(Player_Cell parentCell)
    {
        instance = this;
        _playerCell = parentCell;
        //_mainBackpack = GameObject.Find("Content_EquipmentTab").GetComponent<EquipmentScript>();

       var uicontroller = Instantiate(GraphicSwitchPrefab,_playerCell.playerSpriteObject.transform);
       GraphicSwitch = uicontroller.GetComponent<PlayerEquipmentVisualSwitchScript>();
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
    public void Restart_ClearStatsAndEquipedItems()
    {
        ArmorIMG.enabled = false;
        HelmetIMG.enabled = false;

        _EquipedItems.Reset_WipeOutDataAndImages();

        Level = 1; Gold = 0; Experience = 0; Strength = 1; Inteligence = 1; Dexterity = 1; 
        _EquipedItems.ItemSlots.ForEach(slot=>slot.ITEM = new Chest.ItemPack(0, null));
    }

}