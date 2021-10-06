using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsEventsScript : MonoBehaviour
{

    public bool AttackAnimationsFinished = true;

    public bool AnimationInProgress = false;

    public void AttackAnimationFinished()
    {
       // Debug.LogWarning("Attack start");
        if(AnimationInProgress == false)
        {
            AnimationInProgress = true;
            AttackAnimationsFinished = true;
            SkillsManager.ProjectileReleased = false;
        }
    }

    public void AttackAnimationStarted()
    {
        SkillsManager.Hit1ImpactTrigger = false;
        SkillsManager.Hit2ImpactTrigger = false;
        AttackAnimationsFinished = false;
        AnimationInProgress = false;
    }

    public void Attack_Hit_1()
    {
        SkillsManager.Hit1ImpactTrigger = true;
    }

    public void ShootProjectile()
    {
        SkillsManager.ProjectileReleased = true;
    }
}
