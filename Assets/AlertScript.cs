using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlertScript : MonoBehaviour
{
    public Color32 Color 
    { 
        get => _color; 
        set 
        {
            print("color changed");
             _color = value; 
             Background.color = new Color32((byte)(value.r/3),(byte)(value.g/3),(byte)(value.b/3),175);
             Border.color = value;
        }
    }

    public string TextValue 
    { 
        get => textValue; 
        set 
        {   
            print("changed from "+textValue +" to "+value);
            textValue = value; 
            text.SetText(value);
        }
    }


    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] Image Background;
    [SerializeField] Image Border;
    private Color32 _color;

   [SerializeField]  private string textValue = "0";

   private void Start() {
       GameObject.Find("EventSystem").GetComponent<EventSystem>().enabled = false;
   }
    private void OnDestroy() {
       GameObject.Find("EventSystem").GetComponent<EventSystem>().enabled = true;
    }
}
