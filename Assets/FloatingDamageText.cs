using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingDamageText : MonoBehaviour
{

    [SerializeField] GameObject RegularAttack;
    [SerializeField] GameObject CriticalAttack;
    [SerializeField] GameObject DodgedAttack;
    [SerializeField] GameObject BlockedAttack;

    public Image ICON;
    public TextMeshProUGUI VALUE;

    public void AttackType(string type)
    {
        RegularAttack.SetActive(false);
        CriticalAttack.SetActive(false);
        DodgedAttack.SetActive(false);
        BlockedAttack.SetActive(false);

        switch(type)
        {
            case "regular":
                RegularAttack.SetActive(true);
                VALUE = this.GetComponentInChildren<TextMeshProUGUI>();
                break;
            
            case "critical":
                CriticalAttack.SetActive(true);
                VALUE = this.GetComponentInChildren<TextMeshProUGUI>();
                ICON = this.GetComponentInChildren<Image>();
                break;
            
            case "dodged":
                DodgedAttack.SetActive(true);
                VALUE = this.GetComponentInChildren<TextMeshProUGUI>();
                break;
            
            case "blocked":
                BlockedAttack.SetActive(true);
                VALUE = this.GetComponentInChildren<TextMeshProUGUI>();
                ICON = this.GetComponentInChildren<Image>();
                break;
        }
    }
}
