using UnityEngine;

[CreateAssetMenu(fileName="New Default Item",menuName="GameData/Item/Gold")]
public class GoldItem : ItemData
{
    public void Awake() 
    {
        ItemCoreSettings.Type = ItemType.Gold;
        ItemCoreSettings.Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = false;

    }
}