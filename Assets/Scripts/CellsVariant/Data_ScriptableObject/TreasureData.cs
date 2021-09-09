using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Chest;
using static Treasure_Cell;

[CreateAssetMenu(fileName="New Treasure",menuName="GameData/Treasure")]
public class TreasureData : ScriptableObject
{
    public int ID;
    public string TreasureName; 
    public GameObject Icon_Sprite;
    public int GuarantedGoldReward;
    public TileTypes Type = TileTypes.treasure;
    public bool IsWalkable = true;

    [SerializeField] public List<ItemData> PossibleLootItems;
    
    public  List<ItemPack> GetRandomizeLootPacks()
    {
        List<ItemPack> ListOfContainingItem = new List<ItemPack>();
        ListOfContainingItem = new List<ItemPack>();
        int itemInChest = Random.Range(0,PossibleLootItems.Count+1);
        HashSet<int> randomIndexes = new HashSet<int>();

        for(int i = randomIndexes.Count; i< itemInChest; )
        {
            randomIndexes.Add(Random.Range(0,PossibleLootItems.Count));
            i = randomIndexes.Count;
        }

        foreach(var index in randomIndexes)
        {
            ListOfContainingItem.Add(
                new ItemPack
               (
                    count:Random.Range(PossibleLootItems[index].DropSettings.minCount,PossibleLootItems[index].DropSettings.maxCount+1),
                    item:PossibleLootItems[index]
               )
            );
        }

        return ListOfContainingItem;
    }
}



