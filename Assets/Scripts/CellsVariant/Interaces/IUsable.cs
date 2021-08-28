using System;
using UnityEngine;

public interface IUsable
{
    //[Obsolete("przeniesienie sie ze stringa na gameobject")] string Effect_Url {get;set;}
    GameObject Effect_Sprite  { get; }
    bool IsReadyToUse { get; }
    bool IsUsed { get; set; }

    void Use();
}