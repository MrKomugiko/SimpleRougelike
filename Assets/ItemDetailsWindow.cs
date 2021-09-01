using System.Collections;
using System.Collections.Generic;
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

    

    public void SelfConfigure(ItemData _data, ItemSlot slot)
    {
        if(DATA == _data) return;
        if(DATA != null) ResetToDefault();

        this.DATA = _data;
        this.ParentSlot = slot;
        // 0. Change item name
        ItemTitleName_TMP.SetText(DATA.Name+$" [{DATA.Rarity.ToString()}]");

        // 1 podmiana obrazka
        ItemIcon_IMG.sprite = DATA.Item_Sprite;

        // 2. dodanie opisu przedmiotu
        Description_TMP.SetText(DATA.Description);

        
        // Spawn statystyk
        // ostatnia pozycja jest value , przed nią spacja
        var emptystat = Instantiate(EmptySpace_Prefab,StatsSection.transform);
        StatList.Add(emptystat);

        var stat = Instantiate(BasicStat_Prefab,StatsSection.transform);
            stat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Value:");
            stat.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(DATA.Value.ToString()+"g");
            stat.transform.SetAsLastSibling();
        StatList.Add(stat);

        // spawn przycisków
        if(DATA is IConsumable)
        {
            var button = Instantiate(Button_Prefab, ButtonSection.transform);
                button.GetComponent<Button>().onClick.RemoveAllListeners();
                button.GetComponent<Button>().onClick.AddListener(()=>(DATA as IConsumable).Use(ParentSlot.IndexID));
            ButtonsList.Add(button);
        }

        // konfiguracja wymagań
        string level = DATA.Level <= PlayerManager.Level?$"<color={RequirmentGood_colorHex}>{DATA.Level}</color>":$"<color={RequirmentFail_colorHex}>{DATA.Level}</color>";
        string Strength = DATA.Strength <= PlayerManager.Strength?$"<color={RequirmentGood_colorHex}>{DATA.Strength}</color>":$"<color={RequirmentFail_colorHex}>{DATA.Strength}</color>";
        string Inteligence = DATA.Inteligence <= PlayerManager.Inteligence?$"<color={RequirmentGood_colorHex}>{DATA.Inteligence}</color>":$"<color={RequirmentFail_colorHex}>{DATA.Inteligence}</color>";
        string Dexterity = DATA.Dexterity <= PlayerManager.Dexterity?$"<color={RequirmentGood_colorHex}>{DATA.Dexterity}</color>":$"<color={RequirmentFail_colorHex}>{DATA.Dexterity}</color>";

        string requirmentsString = $"Lvl:{level}  Str:{Strength}  Int:{Inteligence}  Dex:{Dexterity}";
        Requirments_TMP.SetText(requirmentsString);

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
