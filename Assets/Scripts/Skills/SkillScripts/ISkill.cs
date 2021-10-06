using System;
using UnityEngine;

public interface ISkill
{
    void Select();
    void Execute(Monster_Cell target);
}