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
    public int Value;
    public TileTypes Type = TileTypes.treasure;
    public bool IsWalkable = true;

    [SerializeField] public List<ItemData> PossibleLootItems;
    
    public  List<ItemPack> GetRandomizeLootPacks()
    {
        List<ItemPack> ListOfContainingItem = new List<ItemPack>();

      //  Debug.Log("Randomize");
        ListOfContainingItem = new List<ItemPack>();
        // 1. random item count
        int itemInChest = Random.Range(0,PossibleLootItems.Count+1);
      //  Debug.Log($"itemsInChest = "+itemInChest);
        // loop in random indexes 
        HashSet<int> randomIndexes = new HashSet<int>();
       // Debug.Log($"start : randomIndexes count = {randomIndexes.Count}");

        for(int i = randomIndexes.Count; i< itemInChest; )
        {
            randomIndexes.Add(Random.Range(0,PossibleLootItems.Count));
            i = randomIndexes.Count;
       //     Debug.Log("index: "+randomIndexes.Last());
        }
       // Debug.Log($"end : randomIndexes count = {randomIndexes.Count}");

        foreach(var index in randomIndexes)
        {
            ListOfContainingItem.Add(
                new ItemPack
                {
                    count = Random.Range(PossibleLootItems[index].DropSettings.minCount,PossibleLootItems[index].DropSettings.maxCount+1),
                    item = PossibleLootItems[index]
                }
            );
        }

        return ListOfContainingItem;
    }
}



