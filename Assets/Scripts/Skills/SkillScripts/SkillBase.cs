
using System;
using UnityEngine;

[CreateAssetMenu(fileName="New Skill",menuName="GameData/Skill")]
public class SkillBase : SkillData
{   
    public ISkill SkillLogic;
    public float CurrentDamageMultiplifer;
    [SerializeField] private bool _isEnoughtResourcesToUse;
    [SerializeField] private int _cooldownLeftToBeReady;
    public int CooldownLeftToBeReady 
    { 
        get => _cooldownLeftToBeReady; 
        set
        {
            _cooldownLeftToBeReady = value<0?0:value; 
        }
    }
    public int SkillAnimationLayer;

    public virtual bool IsEnoughtResourcesToUse {
        get
        {   
            // Check for ammunitions of correct type and available to use
            var staminaCheck = (PlayerManager.instance.CurrentStamina >= this.StaminaCost);
            return _isEnoughtResourcesToUse = staminaCheck;
        }    
    }
    public bool ReadyToUse => CooldownLeftToBeReady==0?true:false;

    public void TickCooldown(int timeReduction = 1)
    {
        CooldownLeftToBeReady-=timeReduction;
    }
    public void ResetCooldown()
    {
        _cooldownLeftToBeReady = base.Cooldown;
    }

}