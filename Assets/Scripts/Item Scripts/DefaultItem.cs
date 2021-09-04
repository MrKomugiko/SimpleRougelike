using UnityEngine;

[CreateAssetMenu(fileName="New Default Item",menuName="GameData/Item/Default")]
public class DefaultItem : ItemData
{
    public void Awake() 
    {
        ItemCoreSettings.Type = ItemType.Default;
        ItemCoreSettings.Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = false;

    }
}

