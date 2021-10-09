using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AmmunitionItem;

public class SkillsManager : MonoBehaviour
{    
    public static bool SkillAnimationFinished = true;
    public static SkillNode CurrentSelectedSkill;
    public static SkillNode ROOT_SKILLTREE;
    private static Action<Monster_Cell> _selectedAttackSkill = null;

    public List<SkillBase> AllSkills = new List<SkillBase>();

    public static bool Hit1ImpactTrigger;
    public static bool Hit2ImpactTrigger;
    public static bool ProjectileReleased;

    public static Action<Monster_Cell> SelectedAttackSkill { 
        get => _selectedAttackSkill; 
        set
        {
            // if(value == null)
            // {
            //     PlayerManager.instance.MovmentValidator.HideAttackGrid();
            // }
            _selectedAttackSkill = value; 
        }
    }

    private void Start() 
    {
        GenerateFullSkillTree();
    }

    public static void RefreshAmmoDatafromBackPack()
    {
        Debug.Log("ammo info refreshed");
        CurrentAvailableAmmo.Clear();

        foreach(var ammopack in PlayerManager.instance._mainBackpack.GetSumListAvailableAmmunition())
        {
            CurrentAvailableAmmo.Add(ammopack.item as AmmunitionItem, ammopack.Count);
        }

        GameManager.instance.attackSelectorPopup.RefreshSkillsRequirmentCheck();
    
    }
    public  List<SkillNode> GenerateFullSkillTree()
    {        
        ROOT_SKILLTREE = new SkillNode(0,"ROOT",parent:null,null);

        ROOT_SKILLTREE.Childs.Add((new SkillNode(0,"EXIT",parent:null,null)));
        // var AvailableskillsList = new List<SkillNode>();
        foreach(var skill in AllSkills)
        {
            if(skill.ParentName=="ROOT")
            {
                ROOT_SKILLTREE.Childs.Add(new SkillNode(ROOT_SKILLTREE.Childs.Count, skill.Name, parent:null,skill));
                continue;
            }
            else
            {
                List<string> path = skill.ParentName.Split(char.Parse("/")).ToList();
                SkillNode parentNode = ROOT_SKILLTREE;
                foreach(var node in path)
                {
                    if(node == "ROOT")  
                    {
                        continue;
                    }

                    if(parentNode.Childs.Where(n=>n.Node_Name == node).Any())
                    {
                        parentNode = parentNode.Childs.Where(n=>n.Node_Name == node).First();
                    }
                }
                parentNode.AddChild((name:skill.Name,data:skill));
            }
        };

        return ROOT_SKILLTREE.Childs;
    }

    public static Dictionary<AmmunitionItem, int> CurrentAvailableAmmo = new Dictionary<AmmunitionItem, int>();
    public static bool CheckAmmunitionCount(AmmunitionType specificAmmoType = 0, int ammoValue = 0)
    {
        if(ammoValue == 0)
        {
            Debug.Log("skill nie wymaga uzycia amunicji");
            return true;
        }

        if(specificAmmoType != 0)
        {
            if(CurrentAvailableAmmo.Any(a=>a.Key._Type == specificAmmoType && a.Value >= ammoValue))
            {
                 Debug.Log("W PLECAKU ZNAJDUJE SIE WYBRANA AMUNICJA W MINUMALNEJ POTRZEBNEJ DO UZYCIA ILOSCI");
                return true; // 
            }
            Debug.Log("brak wymaganej ilości konkretnej amunicji");
            return false;
        }
        else
        {
            if(CurrentAvailableAmmo.Any(a=>a.Value >= ammoValue))
            {
                Debug.Log("jest na stanie jakakolwiek amunicja o wymaganej iloscic");
                return true; //  
            }
            Debug.Log("Brak wymaganej ilosci amunicji");
            return false;  
        }
    }

    public void TickSkillsCooldowns()
    {
        foreach(var skill in AllSkills)
        {
            skill.TickCooldown();
        }
    }

    public void SkillAnimationEnded()
    {
        SkillAnimationFinished = true;
    }
}
