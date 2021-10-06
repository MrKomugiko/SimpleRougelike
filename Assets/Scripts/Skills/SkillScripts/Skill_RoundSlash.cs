using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="Skill_RoundSlash",menuName="GameData/Skill/RoundSlash")]
public class Skill_RoundSlash : SkillBase, ISkill
{
    public List<Vector2Int> AreaTargetsCheckVectors = new List<Vector2Int>()
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public Skill_RoundSlash()
    {
        base.SkillLogic = this;
    }

    public void Select()
    {
        if(base.isCategoryType) return;

        ShowAvailableTargets();
        SkillsManager.SelectedAttackSkill = Execute;
    }

    public void Execute(Monster_Cell target)
    {
        SkillsManager.Hit1ImpactTrigger = false;

        AssignSkillAnimations(target.ParentCell.CurrentPosition);
        //lock turn routine
        SkillsManager.SkillAnimationFinished = false;

        GameManager.instance.StartCoroutine(ProcessSkillRoutine());

        // after select skill - hide popup and reset centered skill
        GameObject.Find("ActionsPopUp").GetComponent<SelectionPopupController>().ClearCenteredNode();
        GameObject.Find("ActionsPopUp").GetComponent<SelectionPopupController>().gameObject.SetActive(false);
        PlayerManager.instance.CurrentStamina-=base.StaminaCost;

        // reset skill's cooldown time 
        base.ResetCooldown();
    }

    private void AssignSkillAnimations(Vector2Int targetCoord)
    { 

        // Ten skil bedzie mial 1 animacja obrotu, kierunek to tylko kolejnosc od ktorego zacznie atakowac, do rozkminki TODO:
        Vector2Int direction = PlayerManager.instance._playerCell.ParentCell.CurrentPosition - targetCoord;

        if(direction.y < 0)
            CustomEventManager.PlayerAnimator.Play("Player_Attack_upanim",base.SkillAnimationLayer);
        else if(direction.y > 0)
            CustomEventManager.PlayerAnimator.Play("Player_Attack_downanim",base.SkillAnimationLayer);
        else if(direction.x > 0)
            CustomEventManager.PlayerAnimator.Play("Player_Attack_leftanim",base.SkillAnimationLayer);
        else if(direction.x < 0)
            CustomEventManager.PlayerAnimator.Play("Player_Attack_rightanim",base.SkillAnimationLayer);
    }

    private IEnumerator ProcessSkillRoutine()
    {
         int _damage; bool _isCritical;
        PlayerManager.instance.CalculateAttackHit(out _damage, out _isCritical);
        _damage = Mathf.RoundToInt(base.DamageMultiplifer*_damage);

        int currentTargetIndex = 0;
        while(true)
        {
            yield return new WaitUntil(()=>SkillsManager.Hit1ImpactTrigger == true);
            if(Targets[currentTargetIndex] != null)
                (Targets[currentTargetIndex].SpecialTile as Monster_Cell).TakeDamage(_damage, "Attacked by player",_isCritical);
            
            SkillsManager.Hit1ImpactTrigger = false;

            currentTargetIndex++;

            if(currentTargetIndex>=Targets.Count) break;
        }

        PlayerManager.instance.AtackAnimationInProgress = false;
        SkillsManager.SelectedAttackSkill = null;
        SkillsManager.SkillAnimationFinished = true;
        GameManager.instance.PlayerAttacked = true;
        yield break;
    }

    private List<CellScript> Targets = new List<CellScript>();
    private void ShowAvailableTargets()
    {
        PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();
        PlayerManager.instance.MovmentValidator.SpawnMarksOnGrid();
        
        Targets = PlayerManager.instance.MovmentValidator.HighlightValidAttackGridNearPlayer(AreaTargetsCheckVectors).targets;
        Debug.Log("roudskill attack, mobs in skill attack range"+ Targets.Count);
    }
}