using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomEventManager : MonoBehaviour
{
    public static CustomEventManager instance;

    private void Awake() {
        instance = this;
    }
    public event EventHandler<string> OnMonsterDieEvent;
    // public event EventHandler<(CellScript parent, int damageTaken, bool criticalHit)> OnMonsterTakeDamageEvent;


    public void RegisterMonsterInEventManager(Monster_Cell monster_Cell)
    {
        // Debug.LogError("nowy mosnter: "+monster_Cell.Name);
        monster_Cell.OnMonsterDieEvent += DeathAnimation;
        monster_Cell.OnMonsterTakeDamageEvent += FloatingDamageValueAnimation;
    }

    private void FloatingDamageValueAnimation(object sender, (CellScript parent, int damageTaken, bool criticalHit, bool blockedHit, bool dodgedHit) e)
    {
     //   print("rozpoczecie animacji zadania obrazenia i unoszacej sie wartosci dmg");
        StartCoroutine(AnimateDamage(e.parent,e.damageTaken, e.criticalHit, e.blockedHit, e.dodgedHit));
    }

    private void DeathAnimation(object sender, string e)
    {
        //Debug.LogError(e);
        //Debug.LogError("animacja zgonu xd");
    }

    [SerializeField] GameObject floatingDamageTextPrefab;
    private IEnumerator AnimateDamage(CellScript parent,int damageTaken, bool criticalHit, bool blockedHit, bool dodgedHit)
    {
        var damageTextObject = Instantiate(floatingDamageTextPrefab,parent.transform);
        FloatingDamageText floatingText = damageTextObject.GetComponent<FloatingDamageText>();
        
        if(criticalHit) 
        {
            floatingText.AttackType("critical");
            floatingText.VALUE.SetText(damageTaken>0f?damageTaken.ToString():"");
        }
        else if(blockedHit) 
        {
            floatingText.AttackType("blocked");
            floatingText.VALUE.SetText(damageTaken>0f?damageTaken.ToString():"");
        }
        else if(dodgedHit) 
        {
            floatingText.AttackType("dodged");
            floatingText.VALUE.SetText("miss");
        }
        else 
        {
            floatingText.AttackType("regular");
            floatingText.VALUE.SetText(damageTaken>0f?damageTaken.ToString():"");
        }

        
        Vector3 startpos = damageTextObject.transform.localPosition;
        startpos.y += 65;
        Vector3 endpos = new Vector3(startpos.x,startpos.y+150);

        Color32 textColor = floatingText.VALUE.color;
        Color32 endColor = new Color32(textColor.r,textColor.g,textColor.b,175);
        for(float i = 0; i<=1; i+=.015f)
        {
            if(floatingText.ICON != null)
            {
                floatingText.ICON.color = Color32.Lerp(textColor,endColor,i);
            }
            floatingText.VALUE.color = Color32.Lerp(textColor,endColor,i);
            damageTextObject.transform.localPosition = Vector3.Lerp(startpos,endpos,i);

            yield return new WaitForFixedUpdate();
  
        }
        Destroy(damageTextObject);
    }

    internal void RegisterPlayerInEventManager(Player_Cell player_Cell)
    {
        //Debug.LogError("gracz zarejestrowany");
        // playerManager.OnPlayerDieEvent += DeathAnimation;
        player_Cell.OnPlayerTakeDamageEvent += FloatingDamageValueAnimation;
    }
}
