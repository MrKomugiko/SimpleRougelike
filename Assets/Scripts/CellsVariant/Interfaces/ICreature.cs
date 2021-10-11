using System.Collections.Generic;
using UnityEngine;
using static Chest;

internal interface ICreature : ISpecialTile, ISelectable, ILivingThing
{
    int Level { get; set; }
    int ExperiencePoints {get;set;}
    TreasureData lootID { get; }
    int TurnsRequiredToMakeAction { get; }
    int TurnsElapsedCounter { get; set; }
    bool ISReadyToMakeAction { get; }
    void ChangeIntoTreasureObject(TreasureData _data, List<ItemPack> extraLootContent);
    bool TryAttack(CellScript target);
    bool TryMove(CellScript target);
}