using UnityEngine;

internal interface ICreature : ISpecialTile, ISelectable, ILivingThing
{
    int Level { get; set; }
    int ExperiencePoints {get;set;}
    TreasureData lootID { get; }
    int TurnsRequiredToMakeAction { get; }
    int TurnsElapsedCounter { get; set; }
    bool ISReadyToMakeAction { get; }
    void ChangeIntoTreasureObject(TreasureData _data);
    bool TryAttack(CellScript target);
    bool TryMove(CellScript target);
}