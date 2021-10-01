
using UnityEngine;

[CreateAssetMenu(fileName="New Skill",menuName="GameData/Skill")]
public class SkillBase : SkillData
{
    public void Execute()
    {
        Debug.Log($"Execure skill: {base.Name}");
    }
}