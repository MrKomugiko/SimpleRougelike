internal interface IEnemy : ISpecialTile
{
    int ExperiencePoints {get;set;}
    int HealthPoints {get;set;}
    bool IsAlive {get;}
    int lootID { get; }

    void TakeDamage(int value, string source);
    void ChangeToTreasureObject(string corpse_Url, object lootID);
}