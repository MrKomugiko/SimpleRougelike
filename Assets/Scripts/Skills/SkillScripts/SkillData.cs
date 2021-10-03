using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SkillData : ScriptableObject
{
    public string Name = "Slash";
    public float DamageMultiplifer = 1f;
    public int StaminaCost = 1;
    public int Cooldown = 1;
    public int Range = 1;
    public string ParentName = null;
    public Sprite SkillIcon;
    public bool isCategoryType  = false;
    public bool isLocked = true;
    public bool isObtained = true;
}
