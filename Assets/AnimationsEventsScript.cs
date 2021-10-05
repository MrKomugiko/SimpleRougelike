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
        }
    }

    public void AttackAnimationStarted()
    {
       // Debug.LogWarning("Attack end");
        SkillsManager.Hit1ImpactTrigger = false;
        SkillsManager.Hit2ImpactTrigger = false;
        AttackAnimationsFinished = false;
        AnimationInProgress = false;

    }

    public void Attack_Hit_1()
    {
        Debug.Log("IMPACK POINT 1st hit");
        SkillsManager.Hit1ImpactTrigger = true;
    }

    
    public void Attack_Hit_2()
    {
        Debug.Log("IMPACK POINT 2nd hit");
        SkillsManager.Hit2ImpactTrigger = true;
        
    }
}
