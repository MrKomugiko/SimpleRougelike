using UnityEngine;

public interface ILivingThing
{
    int HealthPoints { get; set; }
    bool IsAlive { get; }
    float Damage { get; }
    GameObject Corpse_Sprite { get; }
    int MaxHealthPoints { get; }

    void TakeDamage(float value, string source);
    
    
}