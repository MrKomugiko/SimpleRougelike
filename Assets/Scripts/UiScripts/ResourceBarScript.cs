using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBarScript : MonoBehaviour
{
    public Image fillProgressBarIMG;
    [SerializeField] TextMeshProUGUI LabelTMP;
    public void UpdateBar(float currentValue,int MaxValue)
    {
        float percentFillvalue = (float)currentValue / Math.Max(MaxValue,1);
        LabelTMP.SetText($"{currentValue.ToString("N2")}/{MaxValue}");
        
        fillProgressBarIMG.fillAmount = percentFillvalue;
    }   
}
