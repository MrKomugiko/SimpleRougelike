using System.Collections.Generic;

public class MonsterBackupData
{
    public int HealthPoints;
    public int MonsterDataID;
    public MonsterBackupData(int _monsterDataID, int _healthPoints)
    {
        this.MonsterDataID = _monsterDataID;
        this.HealthPoints = _healthPoints;
    }
}

public class TreasureBackupData
{   
    public int TreasureDataID;
    public List<Chest.ItemPack> RestoredItemsContent;

    public TreasureBackupData(int _treasureDataID, List<Chest.ItemPack> _restoredItemsContent)
    {
        TreasureDataID = _treasureDataID;
        RestoredItemsContent = _restoredItemsContent;
    }
}