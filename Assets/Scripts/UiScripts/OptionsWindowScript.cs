using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsWindowScript : MonoBehaviour
{

    [SerializeField] InputField STR;
    [SerializeField] InputField INT;
    [SerializeField] InputField DEX;
    [SerializeField] InputField VIT;
    [SerializeField] InputField LVL;

    [SerializeField] Toggle SwapToggle;
    [SerializeField] Toggle SlideToggle;


    private void OnEnable() {
    //     STR.text = PlayerManager.instance.Strength.ToString();
    //     INT.text = PlayerManager.instance.Inteligence.ToString();
    //     DEX.text = PlayerManager.instance.Dexterity.ToString();
    //     VIT.text = PlayerManager.instance.Vitality.ToString();
    //     LVL.text = PlayerManager.instance.Level.ToString();

    //    SwapToggle.isOn = GameManager.instance.SwapTilesAsDefault;
    //    SlideToggle.isOn = GameManager.instance.SlideAsDefault;
    }
    public void Save()
    {
        
        // PlayerManager.instance.AddResource("STR",Int32.Parse(STR.text)-PlayerManager.instance.Strength,true);
        // PlayerManager.instance.AddResource("INT",Int32.Parse(INT.text)-PlayerManager.instance.Inteligence,true);
        // PlayerManager.instance.AddResource("DEX",Int32.Parse(DEX.text)-PlayerManager.instance.Dexterity,true);
        // PlayerManager.instance.AddResource("VIT",Int32.Parse(VIT.text)-PlayerManager.instance.Vitality,true);

        // PlayerManager.instance.Level = Int32.Parse(LVL.text);


        // GameManager.instance.SwapTilesAsDefault = SwapToggle.isOn;
        // GameManager.instance.SlideAsDefault = SlideToggle.isOn;


        // this.gameObject.SetActive(false);

    }
    
    


}
