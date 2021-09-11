using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private void Awake() {
        instance = this;
    }
    
    [SerializeField] public ResourceBarScript Health_Bar;
    [SerializeField] public ResourceBarScript Energy_Bar;
    [SerializeField] public ResourceBarScript Stamina_Bar;
    [SerializeField] public ResourceBarScript Experience_Bar;
    [SerializeField] public TextMeshProUGUI Level_TMP;
    [SerializeField] public TextMeshProUGUI Gold_TMP;
    [SerializeField] public TextMeshProUGUI Diamonds_TMP;
}
