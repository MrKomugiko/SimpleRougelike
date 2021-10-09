using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SkillData : ScriptableObject
{
    public string Name = "Skill Name";
    public float BaseDamageMultiplifer = 1f;
    public int StaminaCost = 1;
    public int Cooldown = 1;
    public string ParentName = "ROOT/...";
    public Sprite SkillIcon;
    public bool isCategoryType  = false;
    public bool isLocked = true;
    public bool isObtained = true; // kupywanie skili 
    public int Range = 1;

    // Requirments
    [Header("Requirments Section")]
    public int MinPlayerLevel = 0;
    public EquipmentSpecifiedType RestrictedByPrimaryWeaponType = EquipmentSpecifiedType.Sword;

    public bool CheckRequirmentsToEnableSkill()
    {
        bool weaponValidationRequired = true;

        if(MinPlayerLevel > PlayerManager.instance.STATS.Level) 
            return false;  // gracz ma za niski poziom

        if(RestrictedByPrimaryWeaponType == EquipmentSpecifiedType.NoRestriction)
        {
            Debug.Log($"skill [{Name}] nie wymaga zadnej broni do uzycia");
            weaponValidationRequired = false;   // sprawdzanie pod kątem rodzaju zalozonej broni nie wymagane
        }
            
        if(weaponValidationRequired)
        {
            if(PlayerManager.instance._EquipedItems.GetEquipmentItemFromSlotType(EquipmentType.PrimaryWeapon) != null)
            {    
                if(RestrictedByPrimaryWeaponType == PlayerManager.instance._EquipedItems.GetEquipmentItemFromSlotType(EquipmentType.PrimaryWeapon).EquipmentSpecificType) 
                {   
                    Debug.Log("gracz ma zalozony wlasciwy item do uzycia skila "+Name);
                    return true;
                }
                else
                {
                    Debug.Log("skill wymaga innego rodzaju broni głownej do poprawnego dzialania "+Name);
                    return false; // 
                }
            }
            else
            {
                Debug.Log($"gracz nie ma broni a mimo to skil [{Name}] ma jakies ograniczeni ewymagajace uzycia jakiejkolwiek broni");
                return false; 
            }
        }
        
        
        return true;
    }

    public void SetLockedStatusBasedOnPlayerLevel()
    {   
        if(MinPlayerLevel <= PlayerManager.instance.STATS.Level) 
            isLocked = false;
        else
            isLocked = true;
    }
}
