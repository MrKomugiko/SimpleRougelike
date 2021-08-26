public partial class NotificationManger
{
    public enum AlertCategory
    {
        Default = 0,
        Info,               // np potwor ruszył sie, czeka ?
        Gold,               // Podnoszenie golda
        Exp,                // zdobycie doświadczenia
        Health,             // zyskanie
        Attack,             // zostanie zaatakowanym
        Player,             // level up gracza ?
        Spawn,              // pojawienie sie czegos ciekawego na mapie
        Loot,               // drop z monsterkow
        PlayerAttack,       // ręczny atak gracza
        ExplosionDamage     // Eplozja np bomby ;d
    }
}

