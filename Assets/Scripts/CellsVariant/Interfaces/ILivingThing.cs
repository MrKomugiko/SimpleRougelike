using UnityEngine;

public interface ILivingThing
{
    int HealthPoints { get; set; }
    bool IsAlive { get; }
    int Damage { get; }
    GameObject Corpse_Sprite { get; }
    int MaxHealthPoints { get; }

    void TakeDamage(int value, string source);
    
    
}