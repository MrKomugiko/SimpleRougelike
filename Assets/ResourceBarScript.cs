using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBarScript : MonoBehaviour
{
    public float MaxWidth;
    public RectTransform rectTransform;
    [SerializeField] TextMeshProUGUI LabelTMP;



    public void UpdateBar(int currentValue,int MaxValue)
    {
        print("current value:"+currentValue+" maxvalue:"+MaxValue);
        float percentFillvalue = (float)currentValue / Math.Max(MaxValue,1);
        LabelTMP.SetText($"{currentValue}/{MaxValue}");
        float fillValue = MaxWidth-(MaxWidth*(1-percentFillvalue));
        print(name+" "+(100-(percentFillvalue*100)+"% = "+fillValue));
        rectTransform.sizeDelta = new Vector2(fillValue,rectTransform.sizeDelta.y);
    }   
}
