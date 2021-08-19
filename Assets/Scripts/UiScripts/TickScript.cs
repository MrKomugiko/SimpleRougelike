using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TickScript : MonoBehaviour
{
    internal CellScript parent;
    [SerializeField] GameObject tickHodler;
    public int TickMaxValue = 5;
    public int CurrentTickValue = 0;
    [SerializeField] private List<Image> ticks = new List<Image>();
    public void ShowTickBar()
    {
        tickHodler.SetActive(true);
    }
    public void HideickBar()
    {
        tickHodler.SetActive(false);
    }
    [ContextMenu("Add Tick")]
    public void AddTickTest()
    {
        AddTick(1);
    }
    public void AddTick(int value = 0)
    {
        for(var i = 0; i<value; i++)
        {
            CurrentTickValue++;
            if(ticks.Count < CurrentTickValue)
            {
                Destroy(parent.Trash
                        .Where(t=>t.name=="Canvas_ActivationProgressBar(Clone)")
                        .First());
                return;    
            } 

            ticks[CurrentTickValue-1].color = Color.red;
        }
    }
}
