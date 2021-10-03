
using UnityEngine;

[CreateAssetMenu(fileName="New Skill",menuName="GameData/Skill")]
public class SkillBase : SkillData
{   
    public ISkill SkillLogic;
    [SerializeField] private int _cooldownLeftToBeReady;
    public int CooldownLeftToBeReady 
    { 
        get => _cooldownLeftToBeReady; 
        set
        {
            _cooldownLeftToBeReady = value<0?0:value; 
        }
    }
    public bool ReadyToUse => CooldownLeftToBeReady==0?true:false;
    [SerializeField] private bool _isEnoughtResourcesToUse;
    public bool IsEnoughtResourcesToUse
    {
        get
        {
           return _isEnoughtResourcesToUse = (PlayerManager.instance.CurrentStamina >= this.StaminaCost);
        }
    }

    public int SkillAnimationLayer;

    public void TickCooldown(int timeReduction = 1)
    {
        CooldownLeftToBeReady-=timeReduction;
    }
    public void ResetCooldown()
    {
        _cooldownLeftToBeReady = base.Cooldown;
    }

}