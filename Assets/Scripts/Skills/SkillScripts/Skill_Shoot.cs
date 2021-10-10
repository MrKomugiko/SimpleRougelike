using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AmmunitionItem;

[CreateAssetMenu(fileName="Skill_Shoot",menuName="GameData/Skill/Shoot")]
public class Skill_Shoot : SkillBase, ISkill
{
    public int AmmunitionsNeeded = 1;
    public AmmunitionType RequiredAmmoType = AmmunitionType.StandardArrow;
    public GameObject ProjectilePrefab;
    public bool FlyAboweWallsAndEnemiesEnabled = false;   // ignore walls on the way
    public bool PenetrationEnabled = false;     // ignore fact passing other mobs on way
    public bool MultipleTargetsEnabled = false; // allow attack every mob in shoot direction
    private List<Vector2Int> AreaTargetsCheckVectors = new List<Vector2Int>();
    private List<CellScript> ConfirmedTargets = new List<CellScript>();
    
    public Skill_Shoot()
    {
        PenetrationEnabled = MultipleTargetsEnabled==true?true:PenetrationEnabled;

        base.SkillLogic = this;
    }
    public override bool IsEnoughtResourcesToUse
    {
        get
        {
            var basecheck = base.IsEnoughtResourcesToUse;
            var ammoCheck = SkillsManager.CheckAmmunitionCount(RequiredAmmoType,AmmunitionsNeeded); 
            return (basecheck==true && ammoCheck) == true ? true : false;
        }
    }

    public void Select()
    {
        if(base.isCategoryType) return;
        ShowAvailableTargets();
        SkillsManager.SelectedAttackSkill = Execute;
    }

    private ProjectileScript ConfigureProjectileObject(Vector2 _mainTarget, List<Vector2> _allTargetsList, Vector2 originShootDirection)
    {
        Vector2Int PlayerPosition = PlayerManager.instance._playerCell.ParentCell.CurrentPosition;
        var projectile = Instantiate(ProjectilePrefab, CustomEventManager.PlayerAnimator.GetComponent<AnimationsEventsScript>().transform);
        projectile.SetActive(false);
        projectile.name = "Projectile";
        ProjectileScript _projectileScript = projectile.GetComponent<ProjectileScript>();
        _projectileScript.MainTarget_GridCoord = new Vector3(_mainTarget.x,_mainTarget.y,0);
        _projectileScript.movingDirection = Vector2Int.CeilToInt(originShootDirection);

        return _projectileScript;
    }
    public void Execute(Monster_Cell target)
    {
        SkillsManager.Hit1ImpactTrigger = false;
        SkillsManager.SkillAnimationFinished = false;
        AssignSkillAnimations(target.ParentCell.CurrentPosition);

        Vector2 OriginShootDirection = GetDirectionVector(from:PlayerManager.instance._playerCell.ParentCell , target:target.ParentCell);
        if(MultipleTargetsEnabled == false)
        {
            if(FlyAboweWallsAndEnemiesEnabled == false)
            {
                ConfirmedTargets = new List<CellScript>(){ConfirmedTargets.First()};
            }
            else
            {
                ConfirmedTargets.Clear(); 
                ConfirmedTargets.Add(target.ParentCell);
            }
        }
        else
        {
            List<CellScript> UpdatedTargetList = new List<CellScript>();
            foreach(var possibleTarget in ConfirmedTargets)
            {
                Vector2Int checkingDirection = GetDirectionVector(from:PlayerManager.instance._playerCell.ParentCell , target:possibleTarget);
                if (possibleTarget == null) continue;
                
                if (OriginShootDirection == checkingDirection)   
                {
                    UpdatedTargetList.Add(possibleTarget);
                    if(possibleTarget.CurrentPosition == target.ParentCell.CurrentPosition)
                    {
                        // nie przeszywamy przeciwników dalej za celemp
                        break;
                    }
                }
            }
            ConfirmedTargets = UpdatedTargetList;
        }

        ProjectileScript _projectileScript = ConfigureProjectileObject(target.ParentCell.CurrentPosition,ConfirmedTargets.Select(t=>(Vector2)t.CurrentPosition).ToList(), OriginShootDirection);
        _projectileScript.TargetsPositionsList = ConfirmedTargets.Select(t=>(Vector2)(t.CurrentPosition)).ToList();
        _projectileScript.LoadAmmoByType(skill:this, RequiredAmmoType);
        
        SkillsManager.ProjectileReleased = false;
        PlayerManager.instance.StartCoroutine(ProcessSkillRoutine(_projectileScript));

        
        GameManager.instance.attackSelectorPopup.ClearCenteredNode();
        GameManager.instance.attackSelectorPopup.gameObject.SetActive(false);
        
        PlayerManager.instance.CurrentStamina-=base.StaminaCost;

        SkillsManager.RefreshAmmoDatafromBackPack();

        base.ResetCooldown();
    }
    private static Vector2Int GetDirectionVector(CellScript from, CellScript target)
    {
        Vector2Int normalizedMovingDirection = Vector2Int.zero;
        var vector = target.CurrentPosition - from.CurrentPosition;
        if (vector.x > 0)
            normalizedMovingDirection.x = 1;
        else if (vector.x < 0)
            normalizedMovingDirection.x = -1;
        if (vector.y > 0)
            normalizedMovingDirection.y = 1;
        else if (vector.y < 0)
            normalizedMovingDirection.y = -1;
        return normalizedMovingDirection;
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
    private IEnumerator ProcessSkillRoutine(ProjectileScript _projectile)
    {
        int _damage; bool _isCritical;
        PlayerManager.instance.CalculateAttackHit(out _damage, out _isCritical);
        _damage = Mathf.RoundToInt(base.BaseDamageMultiplifer*_damage);
        yield return new WaitUntil(()=>SkillsManager.ProjectileReleased == true);
        _projectile.gameObject.SetActive(true);
        _projectile.ShootProjectile(shooterPosition: PlayerManager.instance._playerCell.ParentCell.CurrentPosition);

        int currentTargetIndex = 0;
        while(true)
        {   
            yield return new WaitUntil(()=>SkillsManager.Hit1ImpactTrigger == true);
            if(ConfirmedTargets.Count-1 >= currentTargetIndex)
                (ConfirmedTargets[currentTargetIndex].SpecialTile as Monster_Cell).TakeDamage(_damage*CurrentDamageMultiplifer, "Attacked by player",_isCritical);            
            
            SkillsManager.Hit1ImpactTrigger = false;
            currentTargetIndex++;
            if(currentTargetIndex>=ConfirmedTargets.Count) break;
        }

        PlayerManager.instance.AtackAnimationInProgress = false;
        SkillsManager.SelectedAttackSkill = null;;
        SkillsManager.SkillAnimationFinished = true;
        GameManager.instance.PlayerAttacked = true;
        yield break;
    }
    private void ShowAvailableTargets()
    {
        AreaTargetsCheckVectors = GenerateAreaVectors();

        PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();
        PlayerManager.instance.MovmentValidator.SpawnMarksOnGrid();
        
        ConfirmedTargets = PlayerManager.instance.MovmentValidator.HighlightValidAttackGridStraightProjectile(AreaTargetsCheckVectors,PenetrationEnabled,FlyAboweWallsAndEnemiesEnabled).targets;
    }
    private List<Vector2Int> GenerateAreaVectors()
    {
        List<Vector2Int> vectors = new List<Vector2Int>();
        for (int i = 1; i < base.Range; i++)
        {
            vectors.Add(Vector2Int.up*i);
            vectors.Add(Vector2Int.right*i);
            vectors.Add(Vector2Int.down*i);
            vectors.Add(Vector2Int.left*i);
        }
        return vectors;
    }


}