using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static Chest;
using static EquipmentScript;

[Serializable]
public class MonsterBackupData
{
    public int MonsterDataID;
    public int HealthPoints;
    public MonsterBackupData(int _monsterDataID, int _healthPoints)
    {
        this.MonsterDataID = _monsterDataID;
        this.HealthPoints = _healthPoints;
    }
}
[Serializable]
public class TreasureBackupData
{   
    public int TreasureDataID;

   [JsonIgnore] public List<Chest.ItemPack> RestoredItemsContent = new List<Chest.ItemPack>();

    public List<ItemBackupData> TreasureContent_Data = new List<ItemBackupData>();

    public TreasureBackupData(int _treasureDataID, List<Chest.ItemPack> _restoredItemsContent)
    {
        TreasureDataID = _treasureDataID;
        RestoredItemsContent = _restoredItemsContent;
        TreasureContent_Data = GenerateSerializableItemsBackupData();
        if(TreasureContent_Data.Count > 0)
        {
            LoadDataFromBackup();
        }
    }

    public void LoadDataFromBackup()
    {
        RestoredItemsContent = new List<ItemPack>();
        foreach(var data in TreasureContent_Data)
        {
            if(data.Count == 0) continue;
            var item = GameManager.ItemsDatabase.Where(i=>i.name == data.ScriptableObjectName).First();
            
            ItemPack loadedItempack = new ItemPack(data.Count, item);
            RestoredItemsContent.Add(loadedItempack);
        }
    }

    private List<ItemBackupData> GenerateSerializableItemsBackupData()
    {
        List<ItemBackupData> data_jsonproof = new List<ItemBackupData>();
        int index = 0;
        if(RestoredItemsContent != null)
        {     
            foreach(var item in RestoredItemsContent.Where(item=>item.Count >0))
            {
                data_jsonproof.Add(new ItemBackupData(index,item.Count,item.item.name));
                index++;
            }
        }
        return data_jsonproof;
    }
}

[Serializable]
public class BombBackupData
{
    public int BombId;

    public BombBackupData(int bombID)
    {
        BombId = bombID;
    }
}

[Serializable]
public class PortalBackupData
{   
    public int PortalID;
    public string PortalStatus = "open";
    public string StartLocation;
    public string EndLocation;

    public PortalBackupData(int portalID, string portalStatus, string startLocation, string endLocation)
    {
        PortalID = portalID;
        PortalStatus = portalStatus;
        StartLocation = startLocation;
        EndLocation = endLocation;
    }
}