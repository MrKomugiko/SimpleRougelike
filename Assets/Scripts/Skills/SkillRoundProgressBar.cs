using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillRoundProgressBar : MonoBehaviour
{
    public Image fillProgressBarIMG;
    public void UpdateBar(float currentValue,int MaxValue)
    {
        float percentFillvalue = (float)currentValue / Math.Max(MaxValue,1);
        
        fillProgressBarIMG.fillAmount = percentFillvalue;
    }  
}
