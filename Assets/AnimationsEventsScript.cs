using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsEventsScript : MonoBehaviour
{

    public bool AttackAnimationsFinished = true;

    public bool AnimationInProgress = false;
    public void AttackAnimationFinished()
    {
        if(AllProjectilesReachTarget == true)
        {
            Debug.LogWarning("AttackAnimationFinished");
            if(AnimationInProgress == false)
            {
                AnimationInProgress = true;
                AttackAnimationsFinished = true;
                SkillsManager.ProjectileReleased = false;
            }
        }
    }

    public IEnumerator WaitForAllProjetileHit()
    {   
        Debug.Log("czekanie az wszystkie pociski dosięgną celu");
        yield return new WaitUntil(()=>AllProjectilesReachTarget);
        AttackAnimationFinished();
    }
    public void AttackAnimationStarted()
    {
        Debug.Log("animacja wystartowala");
       
        SkillsManager.Hit1ImpactTrigger = false;
        SkillsManager.Hit2ImpactTrigger = false;
        AttackAnimationsFinished = false;
        AnimationInProgress = false;
        AllProjectilesReachTarget = false;
    }

    public void Attack_Hit_1()
    {
        //Debug.Log("trafienie ");
        SkillsManager.Hit1ImpactTrigger = true;
    }

    public void ShootProjectile()
    {
       // Debug.Log("wystrzal pocisku");
        SkillsManager.ProjectileReleased = true;
    }

    public bool AllProjectilesReachTarget = true;

}
