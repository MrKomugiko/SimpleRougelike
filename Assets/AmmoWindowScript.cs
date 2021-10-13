using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoWindowScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> AmmoListObject;
    public static List<AmmoRecord> AmmoRecords = new List<AmmoRecord>();
    Dictionary<AmmunitionItem, int> AmmunitionCollection = new Dictionary<AmmunitionItem, int>();


    private void Awake()
    {
        InitializeDefaultListElements();
    }
    private void OnEnable()
    {
        AmmunitionCollection = AmmunitionManagerScript.CurrentAvailableAmmo;
        int ammoCollectiontypes = AmmunitionCollection.Count - 1;
        var empty = new KeyValuePair<AmmunitionItem, int>(null, 0);

        AmmoRecords[0].ConfigureRecord(AmmunitionCollection != null ? AmmunitionCollection.ElementAt(0) : empty);
        AmmoRecords[1].ConfigureRecord(ammoCollectiontypes > 0 ? AmmunitionCollection.ElementAt(1) : empty);
        AmmoRecords[2].ConfigureRecord(ammoCollectiontypes > 1 ? AmmunitionCollection.ElementAt(2) : empty);
    }
    private void RefreshList(List<KeyValuePair<AmmunitionItem, int>> sortedList)
    {
        int maxAmmoListIndex = AmmunitionCollection.Count - 1;
        var empty = new KeyValuePair<AmmunitionItem, int>(null, 0);

        AmmoRecords[0].ConfigureRecord(AmmunitionCollection != null ? sortedList.ElementAt(0) : empty);
        AmmoRecords[1].ConfigureRecord(maxAmmoListIndex > 0 ? sortedList.ElementAt(1) : empty);
        AmmoRecords[2].ConfigureRecord(maxAmmoListIndex > 1 ? sortedList.ElementAt(2) : empty);
    }
    private void InitializeDefaultListElements()
    {
        foreach (var record in AmmoListObject)
        {
            AmmoRecords.Add(new AmmoRecord(record));
        }
    }


    public string CurrentSortingCategory = "";
    public int SortingDirection = 1;

    public void SortBy(string categoryName)
    {
        if (CurrentSortingCategory == categoryName)
        {
            SortingDirection *= -1;
        }
        else if (CurrentSortingCategory != categoryName)
        {// wybrany inna zakladka do srtowania w defaulcie sortowanie rosnace
            SortingDirection = 1;
        }
        CurrentSortingCategory = categoryName;
        List<KeyValuePair<AmmunitionItem, int>> sortedList = new List<KeyValuePair<AmmunitionItem, int>>();
        AmmunitionCollection.ToList().ForEach(a => Debug.Log(a.Key.name));
        switch (categoryName)
        {
            case "Rarity":
                if (SortingDirection > 0)
                    sortedList = AmmunitionCollection.OrderByDescending(key => key.Key.ItemCoreSettings.Rarity).ToList();
                else
                    sortedList = AmmunitionCollection.OrderBy(key => key.Key.ItemCoreSettings.Rarity).ToList();
                break;

            case "Name":
                if (SortingDirection > 0)
                    sortedList = AmmunitionCollection.OrderByDescending(key => key.Key.ItemCoreSettings.Name).ToList();
                else
                    sortedList = AmmunitionCollection.OrderBy(key => key.Key.ItemCoreSettings.Name).ToList();
                break;

            case "Damage":
                if (SortingDirection > 0)
                    sortedList = AmmunitionCollection.OrderByDescending(key => key.Key.BaseDamageMultiplifer).ToList();
                else
                    sortedList = AmmunitionCollection.OrderBy(key => key.Key.BaseDamageMultiplifer).ToList();

                break;

            case "Quantity":
                if (SortingDirection > 0)
                    sortedList = AmmunitionCollection.OrderByDescending(key => key.Value).ToList();
                else
                    sortedList = AmmunitionCollection.OrderBy(key => key.Value).ToList();

                break;
        }

        MarkCurrentSortingTab();
        
        RefreshList(sortedList);
    }

    [SerializeField] List<TextMeshProUGUI> sortingTabsTMP = new List<TextMeshProUGUI>();
    private void MarkCurrentSortingTab()
    {
        foreach(var tab in sortingTabsTMP)
        {
            if(tab.text == CurrentSortingCategory)
            {
                tab.fontStyle = FontStyles.Bold;
                tab.fontStyle = FontStyles.Underline;
                tab.alpha = 1;
            }
            else
            {
                tab.fontStyle = FontStyles.Normal;
                tab.alpha = .5f;
            }
        }
    }

    public struct AmmoRecord
    {
        GameObject recordGameObject;
        Image RarityBackgroundColor_IMG;
        int ID;
        Image Icon_IMG;
        TextMeshProUGUI Name_TMP;
        TextMeshProUGUI Quantity_TMP;
        TextMeshProUGUI Damage_TMP;
        Button button;

        public AmmoRecord(GameObject AmmoRecordObject)
        {
            Debug.Log("dodanie wpisu");
            recordGameObject = AmmoRecordObject;
            ID = AmmoRecords.Count;
            Icon_IMG = AmmoRecordObject.transform.Find("AmmoIcon").GetComponent<Image>();
            Name_TMP = AmmoRecordObject.transform.Find("AmmoName").GetComponent<TextMeshProUGUI>();
            Quantity_TMP = AmmoRecordObject.transform.Find("LiczbaSztuk").GetComponent<TextMeshProUGUI>();
            Damage_TMP = AmmoRecordObject.transform.Find("DamageValue").GetComponent<TextMeshProUGUI>();
            RarityBackgroundColor_IMG = AmmoRecordObject.GetComponent<Image>();
            button = AmmoRecordObject.GetComponent<Button>();
        }

        public void ConfigureRecord(KeyValuePair<AmmunitionItem, int> data)
        {
            if (data.Key == null)
            {
                Debug.Log("null - bedzie trzeba pochowac wszysytko");
                recordGameObject.SetActive(false);
            }
            else
            {
                Debug.Log("są dane, mozna konfigurowac rekordy");
                recordGameObject.SetActive(true);
                Icon_IMG.sprite = data.Key.ItemCoreSettings.Item_Sprite;
                Name_TMP.SetText(data.Key.ItemCoreSettings.Name);
                Quantity_TMP.SetText(data.Value.ToString() + "pcs.");
                Damage_TMP.SetText(data.Key.BaseDamageMultiplifer + "x");
                RarityBackgroundColor_IMG.color = GetRarityColor(data.Key.ItemCoreSettings.Rarity);
                // background-rarity color

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => AmmunitionManagerScript.SetAmmoItemAsDefault(data.Key));
            }
        }

        private Color32 GetRarityColor(RarityTypes rarity)
        {
            switch (rarity)
            {
                case RarityTypes.Common:
                    return new Color32(192, 191, 191, 155);
                case RarityTypes.Rare:
                    return new Color32(36, 159, 75, 155);
                case RarityTypes.Epic:
                    return new Color32(35, 90, 165, 155);
                case RarityTypes.Legend:
                    return new Color32(192, 177, 39, 155);
                case RarityTypes.Ancient:
                    return new Color32(192, 34, 175, 155);

                default:
                    return new Color32(192, 191, 191, 155);
            }
        }
    }


}
