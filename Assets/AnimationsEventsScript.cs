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

        AttackAnimationsFinished = false;
        AnimationInProgress = false;
    }
    
}
