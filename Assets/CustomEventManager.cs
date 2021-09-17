using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomEventManager : MonoBehaviour
{
    public static CustomEventManager instance;

    private void Awake() {
        instance = this;
    }
    public event EventHandler<string> OnMonsterDieEvent;
    public event EventHandler<(CellScript parent,int damageTaken)> OnMonsterTakeDamageEvent;


    public void RegisterMonsterInEventManager(Monster_Cell monster_Cell)
    {
//        Debug.LogError("nowy mosnter: "+monster_Cell.Name);
        monster_Cell.OnMonsterDieEvent += DeathAnimation;
        monster_Cell.OnMonsterTakeDamageEvent += FloatingDamageValueAnimation;
    }

    private void FloatingDamageValueAnimation(object sender, (CellScript parent, int damageTaken) e)
    {
     //   print("rozpoczecie animacji zadania obrazenia i unoszacej sie wartosci dmg");
        StartCoroutine(AnimateDamage(e.parent,e.damageTaken));
    }

    private void DeathAnimation(object sender, string e)
    {
        Debug.LogError(e);
        Debug.LogError("animacja zgonu xd");
    }

    [SerializeField] GameObject floatingDamageTextPrefab;
    private IEnumerator AnimateDamage(CellScript parent,int damageTaken)
    {
        var damageText = Instantiate(floatingDamageTextPrefab,parent.transform);
        TextMeshProUGUI textTMP = damageText.GetComponent<TextMeshProUGUI>();
        textTMP.SetText(damageTaken.ToString());
        Vector3 startpos = damageText.transform.localPosition;
        Vector3 endpos = new Vector3(startpos.x+25,startpos.y+125);

        for(float i = 0; i<=1; i+=.02f)
        {
            textTMP.color = Color32.Lerp(Color.red,Color.clear,i);
            textTMP.transform.localPosition = Vector3.Lerp(startpos,endpos,i);
            yield return new WaitForFixedUpdate();
        }
        Destroy(damageText);
    }
}
