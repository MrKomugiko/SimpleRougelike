using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    public static SkillNode CurrentSelectedSkill;
    public static SkillNode ROOT_SKILLTREE;

    public List<SkillData> AllSkills = new List<SkillData>();
    private void Start() {
        // GenerateExampleSkillTree();
        GenerateSkillTree();
    }
    // public static List<SkillNode> GenerateExampleSkillTree()
    // {        
    //     Debug.Log("generate example skill tree");

    //     ROOT_SKILLTREE = new SkillNode(0,"ROOT",parent:null);

    //     ROOT_SKILLTREE.Childs.AddRange(  
    //         new List<SkillNode>(){
    //             (new SkillNode(0,"EXIT",parent:null)),
    //             (new SkillNode(1,"Attack",parent:null)),
    //             (new SkillNode(2,"Ranged",parent:null)),
    //             (new SkillNode(3,"Magic",parent:null)),
    //             (new SkillNode(4,"Special",parent:null))
    //         }
    //     );
        
    //     ROOT_SKILLTREE["Special"].IsLocked = true;
    //     ROOT_SKILLTREE[1].AddChildsRange(new List<string>(){"Slash","Kick","Strong Attacks","Special Attacks"});
    //         ROOT_SKILLTREE[1]["Kick"].IsLocked = true;
    //         ROOT_SKILLTREE[1]["Strong Attacks"].AddChildsRange(new List<string>(){"Wind Slash","Piercing stab","Guillotine"});
    //             ROOT_SKILLTREE[1]["Strong Attacks"]["Guillotine"].IsLocked = true;
    //         ROOT_SKILLTREE[1]["Special Attacks"].AddChildsRange(new List<string>(){"Backstab teleport","Dash attack"});
    //             ROOT_SKILLTREE[1]["Strong Attacks"]["Wind Slash"].IsLocked = true;

    //     ROOT_SKILLTREE[2].AddChildsRange(new List<string>(){"Shoot","Headshoot","Long Distance","Special Attacks"});
    //         ROOT_SKILLTREE[2]["Headshoot"].IsLocked = true;
    //         ROOT_SKILLTREE[2]["Long Distance"].AddChildsRange(new List<string>(){"Piercing shoot","Rock throw"});
    //             ROOT_SKILLTREE[2]["Long Distance"]["Piercing shoot"].IsLocked = true;
    //         ROOT_SKILLTREE[2]["Special Attacks"].AddChildsRange(new List<string>(){"Arrows rain","Holy arrow","Explosive arrow"});
    //             ROOT_SKILLTREE[2]["Special Attacks"]["Arrows rain"].IsLocked = true;
    //             ROOT_SKILLTREE[2]["Special Attacks"]["Holy arrow"].IsLocked = true;

    //     return ROOT_SKILLTREE.Childs;
    // }

    [ContextMenu("Generate sckill tree for available skills")]
    public  List<SkillNode> GenerateSkillTree()
    {        
        ROOT_SKILLTREE = new SkillNode(0,"ROOT",parent:null,null);

        ROOT_SKILLTREE.Childs.Add((new SkillNode(0,"EXIT",parent:null,null)));
        var AvailableskillsList = new List<SkillNode>();
        foreach(var skill in AllSkills)
        {
            Debug.LogWarning("object name = "+skill.name);
            if(skill.ParentName=="ROOT")
            {
                ROOT_SKILLTREE.Childs.Add(new SkillNode(ROOT_SKILLTREE.Childs.Count, skill.Name, parent:null,skill));
                continue;
            }
            else
            {
                Debug.Log(skill.ParentName);
                List<string> path = skill.ParentName.Split(char.Parse("/")).ToList();

                SkillNode parentNode = ROOT_SKILLTREE;
                foreach(var node in path)
                {
                    if(node == "ROOT")  
                    {
                        Debug.Log("skip ROOT branch, go to his child immediatly");
                        continue;
                    }

                    if(parentNode.Childs.Where(n=>n.Node_Name == node).Any())
                    {
                        parentNode = parentNode.Childs.Where(n=>n.Node_Name == node).First();
                        Debug.Log("zmiana przeszukiwanego noda na: "+parentNode.Node_Name);
                    }
                }
                parentNode.AddChild((name:skill.Name,data:skill));
            }
            
        };


        // ROOT_SKILLTREE.Childs.AddRange(  
        //     new List<SkillNode>(){
        //         // (new SkillNode(0,"EXIT",parent:null)),
        //         (new SkillNode(1,"Attack",parent:null)),
        //         (new SkillNode(2,"Ranged",parent:null)),
        //         (new SkillNode(3,"Magic",parent:null)),
        //         (new SkillNode(4,"Special",parent:null))
        //     }
        // );
        
        // ROOT_SKILLTREE["Special"].IsLocked = true;
        // ROOT_SKILLTREE[1].AddChildsRange(new List<string>(){"Slash","Kick","Strong Attacks","Special Attacks"});
        //     ROOT_SKILLTREE[1]["Kick"].IsLocked = true;
        //     ROOT_SKILLTREE[1]["Strong Attacks"].AddChildsRange(new List<string>(){"Wind Slash","Piercing stab","Guillotine"});
        //         ROOT_SKILLTREE[1]["Strong Attacks"]["Guillotine"].IsLocked = true;
        //     ROOT_SKILLTREE[1]["Special Attacks"].AddChildsRange(new List<string>(){"Backstab teleport","Dash attack"});
        //         ROOT_SKILLTREE[1]["Strong Attacks"]["Wind Slash"].IsLocked = true;

        // ROOT_SKILLTREE[2].AddChildsRange(new List<string>(){"Shoot","Headshoot","Long Distance","Special Attacks"});
        //     ROOT_SKILLTREE[2]["Headshoot"].IsLocked = true;
        //     ROOT_SKILLTREE[2]["Long Distance"].AddChildsRange(new List<string>(){"Piercing shoot","Rock throw"});
        //         ROOT_SKILLTREE[2]["Long Distance"]["Piercing shoot"].IsLocked = true;
        //     ROOT_SKILLTREE[2]["Special Attacks"].AddChildsRange(new List<string>(){"Arrows rain","Holy arrow","Explosive arrow"});
        //         ROOT_SKILLTREE[2]["Special Attacks"]["Arrows rain"].IsLocked = true;
        //         ROOT_SKILLTREE[2]["Special Attacks"]["Holy arrow"].IsLocked = true;

        return ROOT_SKILLTREE.Childs;
    }
}
