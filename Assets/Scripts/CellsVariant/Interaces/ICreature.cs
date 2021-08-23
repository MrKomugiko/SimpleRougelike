internal interface ICreature : ISpecialTile, ISelectable
{
    int ExperiencePoints {get;set;}
    int HealthPoints {get;set;}
    bool IsAlive {get;}
    int lootID { get; }

    void TakeDamage(int value, string source);
    void ChangeIntoTreasureObject(string corpse_Url, object lootID);
    bool TryMove(CellScript target);
    bool TryAttack(CellScript target);

}