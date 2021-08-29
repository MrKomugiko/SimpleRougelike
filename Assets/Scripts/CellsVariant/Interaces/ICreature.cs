using UnityEngine;

internal interface ICreature : ISpecialTile, ISelectable, ILivingThing
{
    int ExperiencePoints {get;set;}
    int lootID { get; }
    int TurnsRequiredToMakeAction { get; }
    int TurnsElapsedCounter { get; set; }
    bool ISReadyToMakeAction { get; }
    void ChangeIntoTreasureObject(string corpse_Url, int lootID);
    bool TryAttack(CellScript target);
    bool TryMove(CellScript target);
}