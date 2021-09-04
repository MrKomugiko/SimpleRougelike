using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsWindow : MonoBehaviour
{
    public ItemSlot ParentSlot {get;private set;}
    private ItemData DATA { get; set;}
    [SerializeField] TextMeshProUGUI ItemTitleName_TMP;
    [SerializeField] Image ItemIcon_IMG;
    [SerializeField] TextMeshProUGUI Description_TMP;
    
    [SerializeField] GameObject StatsSection;
    [SerializeField] GameObject BasicStat_Prefab;
    [SerializeField] GameObject SpecialStat_Prefab;
    [SerializeField] GameObject EmptySpace_Prefab;

    [SerializeField] string RequirmentFail_colorHex;
    [SerializeField] string RequirmentGood_colorHex;
    [SerializeField] TextMeshProUGUI Requirments_TMP;

    List<GameObject> StatList = new List<GameObject>();

    [SerializeField] GameObject ButtonSection;
    [SerializeField] GameObject Button_Prefab;
    List<GameObject> ButtonsList = new List<GameObject>();

    
    private void OnEnable() {
          NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,hide:true));    
    }
    private void OnDisable() {
          NotificationManger.instance.NotificationList.ForEach(n=>NotificationManger.TemporaryHideBordersOnMap(n,hide:false));    
    }
    public void SelfConfigure(ItemData _data, ItemSlot slot)
    {
        if(DATA == _data) return;
        if(DATA != null) ResetToDefault();

        this.DATA = _data;
        this.ParentSlot = slot;
        // 0. Change item name
        ItemTitleName_TMP.SetText(DATA.ItemCoreSettings.Name+$" [{DATA.ItemCoreSettings.Rarity.ToString()}]");

        // 1 podmiana obrazka
        ItemIcon_IMG.sprite = DATA.ItemCoreSettings.Item_Sprite;

        // 2. dodanie opisu przedmiotu
        Description_TMP.SetText(DATA.ItemCoreSettings.Description);

        
        // Spawn statystyk
        // ostatnia pozycja jest value , przed nią spacja
        var emptystat = Instantiate(EmptySpace_Prefab,StatsSection.transform);
        StatList.Add(emptystat);

        var stat = Instantiate(BasicStat_Prefab,StatsSection.transform);
            stat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Value:");
            stat.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(DATA.ItemCoreSettings.GoldValue.ToString()+" Gold");
            stat.transform.SetAsLastSibling();
        StatList.Add(stat);

        // spawn przycisków
        GameObject button;
        if(DATA is IConsumable)
        {
            button = Instantiate(Button_Prefab, ButtonSection.transform);
                button.GetComponent<Button>().onClick.RemoveAllListeners();
                button.GetComponent<Button>().onClick.AddListener(
                    ()=>
                        {
                            (DATA as IConsumable).Use(ParentSlot.itemSlotID);
                            CheckButtons_ItemCount();
                        }
                    );
                button.GetComponentInChildren<TextMeshProUGUI>().SetText("Consume");
                button.name = "ConsumeButton";
                
            ButtonsList.Add(button);
        }
        if(DATA is EquipmentItem)
        {
            string newButtonName = ParentSlot.ParentStorage.StorageName == "Player"?"Unequip":"Equip";

            button = Instantiate(Button_Prefab, ButtonSection.transform);
                button.GetComponentInChildren<TextMeshProUGUI>().SetText(newButtonName);
                button.GetComponent<Button>().onClick.RemoveAllListeners();
                button.GetComponent<Button>().onClick.AddListener(
                    ()=>
                        {
                            if((DATA as EquipmentItem).Equip(ParentSlot))
                            {
                                ResetToDefault();
                               this.gameObject.SetActive(false);
                            }
                        }
                    );
                button.name = "EquipButton";
                
            ButtonsList.Add(button);
        }

        button = Instantiate(Button_Prefab, ButtonSection.transform);
            button.GetComponent<Button>().onClick.RemoveAllListeners();
               button.GetComponent<Button>().onClick.AddListener(
                    ()=>
                        {
                            DATA.Sell(ParentSlot);
                            CheckButtons_ItemCount();
                        }
                    );

            button.GetComponentInChildren<TextMeshProUGUI>().SetText("Sell [1x]");
            button.name = "SellButton";
            
        ButtonsList.Add(button);

        // konfiguracja wymagań
        string level = DATA.RequirmentsSettings.Level <= PlayerManager.instance.Level?$"<color={RequirmentGood_colorHex}>{DATA.RequirmentsSettings.Level}</color>":$"<color={RequirmentFail_colorHex}>{DATA.RequirmentsSettings.Level}</color>";
        string Strength = DATA.RequirmentsSettings.Strength <= PlayerManager.instance.Strength?$"<color={RequirmentGood_colorHex}>{DATA.RequirmentsSettings.Strength}</color>":$"<color={RequirmentFail_colorHex}>{DATA.RequirmentsSettings.Strength}</color>";
        string Inteligence = DATA.RequirmentsSettings.Inteligence <= PlayerManager.instance.Inteligence?$"<color={RequirmentGood_colorHex}>{DATA.RequirmentsSettings.Inteligence}</color>":$"<color={RequirmentFail_colorHex}>{DATA.RequirmentsSettings.Inteligence}</color>";
        string Dexterity = DATA.RequirmentsSettings.Dexterity <= PlayerManager.instance.Dexterity?$"<color={RequirmentGood_colorHex}>{DATA.RequirmentsSettings.Dexterity}</color>":$"<color={RequirmentFail_colorHex}>{DATA.RequirmentsSettings.Dexterity}</color>";

        string requirmentsString = $"Lvl:{level}  Str:{Strength}  Int:{Inteligence}  Dex:{Dexterity}";
        Requirments_TMP.SetText(requirmentsString);

    }

    public void CheckButtons_ItemCount()
    {
        foreach(var btn in ButtonsList)
        {
            if(PlayerManager.instance._mainBackpack.ItemSlots[ParentSlot.itemSlotID].ITEM.count == 0)
            {
                btn.GetComponent<Button>().interactable = false;
                btn.GetComponentInChildren<TextMeshProUGUI>().alpha = .75f;
            }
        }
    }

    public void ResetToDefault()
    {
        DATA = null;
        ParentSlot = null;
        ItemIcon_IMG.sprite = null;
        ItemTitleName_TMP.SetText("");
        Description_TMP.SetText("");
        StatList.ForEach(stat=>Destroy(stat.gameObject));
        StatList.Clear();
        Requirments_TMP.SetText("");
        ButtonsList.ForEach(btn=>Destroy(btn.gameObject));
        ButtonsList.Clear();
    }
}
