using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="Skill_Shoot",menuName="GameData/Skill/Shoot")]
public class Skill_Shoot : SkillBase, ISkill
{
    private List<Vector2Int> AreaTargetsCheckVectors = new List<Vector2Int>();
    public GameObject ProjectilePrefab;
    public bool FlyAboweWallsAndEnemiesEnabled = false;   // ignore walls on the way
    public bool PenetrationEnabled = false;     // ignore fact passing other mobs on way
    public bool MultipleTargetsEnabled = false; // allow attack every mob in shoot direction
    public Skill_Shoot()
    {
        
        // gdy opcja wiele celów jest dostępna, opcja penetracji jest wymagana
        PenetrationEnabled = MultipleTargetsEnabled==true?true:PenetrationEnabled;
         // 4 directions - basic

        //Debug.Log("przypisanie logiki skila shoot");
        base.SkillLogic = this;
    }

    public void Select()
    {
        if(base.isCategoryType) return;

        //Debug.Log($"Select skill to execute: {this.GetType()}");
        ShowAvailableTargets();
        SkillsManager.SelectedAttackSkill = Execute;
    }

    public void Execute(Monster_Cell target)
    {
        SkillsManager.Hit1ImpactTrigger = false;
        Vector2Int PlayerPosition = PlayerManager.instance._playerCell.ParentCell.CurrentPosition;
        Vector2 mainShootingDirection = GetDirectionVector(from:PlayerManager.instance._playerCell.ParentCell , target:target.ParentCell);

        
        var projectile = Instantiate(ProjectilePrefab, CustomEventManager.PlayerAnimator.GetComponent<AnimationsEventsScript>().transform);
        projectile.SetActive(false);
        projectile.name = "Projectile";
        // wstępna Konfiguracja pocisku
        ProjectileScript _projectileScript = projectile.GetComponent<ProjectileScript>();
        _projectileScript.mainGridWorldDestination = new Vector3(target.ParentCell.CurrentPosition.x,target.ParentCell.CurrentPosition.y,0);
        _projectileScript.movingDirection = Vector2Int.CeilToInt(mainShootingDirection);

        if(MultipleTargetsEnabled == false)
        {
            if(FlyAboweWallsAndEnemiesEnabled == false)
            {
                // wtakim wypadku oberwie tylko 1 mobek
                Targets = new List<CellScript>(){Targets.First()};
            }
            else
            {
                Targets.Clear();    // oberwie tylko ten ktory został wybrany
                Targets.Add(target.ParentCell);
            }
            _projectileScript.TargetsPositionsList.Add(target.ParentCell.CurrentPosition);
            Debug.Log("SINGLE _projectileScript.TargetsPositionsList = "+_projectileScript.TargetsPositionsList.Count);

        }
        else
        {
            List<CellScript> UpdatedTargetList = new List<CellScript>();
            // selekcja celów na trasie strzału na basie wybranego celu

            foreach(var possibleTarget in Targets)
            {
                Vector2Int checkingDirection = GetDirectionVector(from:PlayerManager.instance._playerCell.ParentCell , target:possibleTarget);

                if (possibleTarget == null) continue;

                if (mainShootingDirection == checkingDirection)
                {
                    UpdatedTargetList.Add(possibleTarget);
                }
            }
            Targets = UpdatedTargetList;
            _projectileScript.TargetsPositionsList.AddRange(Targets.Select(t=>(Vector2)t.CurrentPosition));
            Debug.Log("MULTI _projectileScript.TargetsPositionsList = "+_projectileScript.TargetsPositionsList.Count);

        }

        AssignSkillAnimations(target.ParentCell.CurrentPosition);
        //lock turn routine
        SkillsManager.SkillAnimationFinished = false;

        PlayerManager.instance.StartCoroutine(ProcessSkillRoutine(_projectileScript));

        // after select skill - hide popup and reset centered skill
        GameObject.Find("ActionsPopUp").GetComponent<SelectionPopupController>().ClearCenteredNode();
        GameObject.Find("ActionsPopUp").GetComponent<SelectionPopupController>().gameObject.SetActive(false);
        PlayerManager.instance.CurrentStamina-=base.StaminaCost;

        // reset skill's cooldown time 
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

    private IEnumerator ProcessSkillRoutine(ProjectileScript _projectile)
    {
         int _damage; bool _isCritical;
        PlayerManager.instance.CalculateAttackHit(out _damage, out _isCritical);
        _damage = Mathf.RoundToInt(base.DamageMultiplifer*_damage);
        yield return new WaitUntil(()=>SkillsManager.ProjectileReleased == true);
        _projectile.gameObject.SetActive(true);
        _projectile.ShootProjectile(shooterPosition: PlayerManager.instance._playerCell.ParentCell.CurrentPosition);
        Debug.Log("arrow released");

        //------------------------
        int currentTargetIndex = 0;
        while(true)
        {   
            // czekanie za info o trafieniach
            yield return new WaitUntil(()=>SkillsManager.Hit1ImpactTrigger == true);
            if(Targets.Count-1 >= currentTargetIndex)
                (Targets[currentTargetIndex].SpecialTile as Monster_Cell).TakeDamage(_damage, "Attacked by player",_isCritical);            
            
            SkillsManager.Hit1ImpactTrigger = false;

            currentTargetIndex++;

            if(currentTargetIndex>=Targets.Count) break;
        }

        PlayerManager.instance.AtackAnimationInProgress = false;
        SkillsManager.SelectedAttackSkill = null;;
        SkillsManager.SkillAnimationFinished = true;
        GameManager.instance.PlayerAttacked = true;
        yield break;
    }

    private List<CellScript> Targets = new List<CellScript>();
    private void ShowAvailableTargets()
    {
        AreaTargetsCheckVectors = GenerateVectors();

        PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();
        PlayerManager.instance.MovmentValidator.SpawnMarksOnGrid();
        
        Targets = PlayerManager.instance.MovmentValidator.HighlightValidAttackGridStraightProjectile(AreaTargetsCheckVectors,PenetrationEnabled,FlyAboweWallsAndEnemiesEnabled).targets;
    }

    private List<Vector2Int> GenerateVectors()
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