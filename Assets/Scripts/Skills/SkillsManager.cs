using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            if(value == null)
            {
                PlayerManager.instance.MovmentValidator.DestroyAllGridObjects();
                Debug.Log("Odświeżenie okna skili - aktualnie wybrany skill i jego podświetlenie dostępnych celów została zrestartowana");
            }
            _selectedAttackSkill = value; 
        }
    }

    private void Start() 
    {
        GenerateFullSkillTree();
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
