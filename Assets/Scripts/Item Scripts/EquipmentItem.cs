using UnityEngine;

[CreateAssetMenu(fileName="New Equipment Item",menuName="GameData/Item/Equipment")]
public class EquipmentItem : ItemData
{
    public void Awake() 
    {
        Type = ItemType.Default;
        Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = false;

    }
}

