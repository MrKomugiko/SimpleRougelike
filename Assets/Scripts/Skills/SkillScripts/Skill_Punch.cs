using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="Skill_Punch",menuName="GameData/Skill/Punch")]
public class Skill_Punch : SkillBase, ISkill
{
    public Skill_Punch()
    {
        CurrentDamageMultiplifer = BaseDamageMultiplifer;
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
        PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();

        SkillsManager.Hit1ImpactTrigger = false;
        SkillsManager.Hit2ImpactTrigger = false;

        AssignSkillAnimations(target.ParentCell.CurrentPosition);
        SkillsManager.SkillAnimationFinished = false;
        GameManager.instance.StartCoroutine(ProcessSkillRoutine(target));

        GameObject.Find("ActionsPopUp").GetComponent<SelectionPopupController>().ClearCenteredNode();
        GameObject.Find("ActionsPopUp").GetComponent<SelectionPopupController>().gameObject.SetActive(false);
        PlayerManager.instance.CurrentStamina-=base.StaminaCost;

        base.ResetCooldown();
    }

    private void AssignSkillAnimations(Vector2Int targetCoord)
    { 
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

    private IEnumerator ProcessSkillRoutine(Monster_Cell target)
    {
         int _damage; bool _isCritical;
         if(RestrictedByPrimaryWeaponType == EquipmentSpecifiedType.NoRestriction)
         {
            PlayerManager.instance.CalculateAttackHit_NOWEAPON(out _damage, out _isCritical); 
         }
         else
         {
            PlayerManager.instance.CalculateAttackHit(out _damage, out _isCritical);
         }
        _damage = Mathf.RoundToInt(base.CurrentDamageMultiplifer*_damage);

        yield return new WaitUntil(()=>SkillsManager.Hit1ImpactTrigger == true);
            target.TakeDamage(_damage, "Attacked by player",_isCritical);

        PlayerManager.instance.AtackAnimationInProgress = false;
        SkillsManager.SelectedAttackSkill = null;;
        SkillsManager.SkillAnimationFinished = true;
        GameManager.instance.PlayerAttacked = true;
        yield break;
    }
    private void ShowAvailableTargets()
    {
        PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();
        PlayerManager.instance.MovmentValidator.SpawnMarksOnGrid();
        PlayerManager.instance.MovmentValidator.HighlightValidAttackGridGlobal(base.Range);
    }
}