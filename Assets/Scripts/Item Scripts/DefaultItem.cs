using UnityEngine;

[CreateAssetMenu(fileName="New Default Item",menuName="GameData/Item/Default")]
public class DefaultItem : ItemData
{
    public void Awake() 
    {
        Type = ItemType.Default;
        Rarity = RarityTypes.Common;
        CanBeAssignToQuickActions = false;

    }
}

