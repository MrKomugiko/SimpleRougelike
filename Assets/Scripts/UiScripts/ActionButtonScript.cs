using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ActionButtonScript : MonoBehaviour
{

    public List<Sprite> IconSpriteList;
    [SerializeField] public GameObject SelectionBorder_Object;
    [SerializeField] public Image ButtonIcon_IMG;
    [SerializeField] private GameObject DescriptionContainer_Object;
    [SerializeField] public TextMeshProUGUI Description_TMP;
    [SerializeField] private Button Icon_Button;
    [SerializeField] private Button Description_Button;
    [SerializeField] public ActionSwitchController controller;
    [SerializeField] private bool _iSSELECTED = false;
    [SerializeField] public TextMeshProUGUI ItemCounter_TMP;

    private Action myMainIconAction = null;

    public bool ISSELECTED { 
        get => _iSSELECTED; 
        set 
        {
             _iSSELECTED = value; 
            if(value == true)
            {
                // TODO: tutaj bedzie trzeba wrócić, zeby nie wyczyscilo przycisku przypisanego do itemka
                Icon_Button.onClick.RemoveAllListeners();
                Icon_Button.onClick.AddListener(()=>controller.ResetToDefault()); // wróć      
                Icon_Button.onClick.AddListener(
                    ()=> {
                        if(EquipmentScript.AssignationItemToQuickSlotIsActive)
                        {
                            // Debug.Log("QuickAction is selected, after press it again , and when assignation item tmode is active, its turn off");

                            // Debug.LogWarning(Description_TMP.text+" == Tap to Add Item = "+Description_TMP.text.Contains("Tap to Add Item"));
                            // if(Description_TMP.text.Contains("Tap to Add Item"))
                            // {
                            //     Description_TMP.SetText("Tap to Cancel");
                            // }
                            
                            EquipmentScript.QuitFromQuickbarSelectionMode();
                        }
                    } 
                ); // wróć      
            }
            else 
            {   
                // gdy nie jest zaznaczony, przypisuje mu sie ostatnio dodaną akcje
                if(myMainIconAction !=null)
                {
                    Icon_Button.onClick.RemoveAllListeners();
                    Icon_Button.onClick.AddListener(()=>myMainIconAction());           
                }
            }
        }
    }


    private void Awake() 
    {
        controller = GetComponentInParent<ActionSwitchController>();
        Description_Button.targetGraphic =controller.transform.Find("Description_Background").GetComponent<Image>();
    }
    public void ShowDescription()
    {   
        ISSELECTED = true;
        SelectionBorder_Object.SetActive(true);
        DescriptionContainer_Object.SetActive(true);
    }
    public void HideDescription()
    {   
        ISSELECTED = false;
        SelectionBorder_Object.SetActive(false);
        DescriptionContainer_Object.SetActive(false);
    }
    public void ConfigureIconButtonClick(Action action,  ActionIcon icon)
    {
        Icon_Button.onClick.RemoveAllListeners();
        myMainIconAction = action;
        Icon_Button.onClick.AddListener(()=>action());;
        ButtonIcon_IMG.sprite = IconSpriteList.First(n=>n.name == "ICON_"+icon.ToString());
    }
    public void ConfigureDescriptionButtonClick(Action action, string description, bool singleAction = true, string actionNameString = "")
    { 
      //  print("przypisanie "+actionNameString);
        Description_Button.onClick.RemoveAllListeners();
        this.gameObject.name = "ACTION_"+description;
      //  Debug.LogWarning(description);
        Description_Button.onClick.AddListener(()=>
        {
           // print("wykonuje akcje");
            action();
            if(singleAction)
                CloseAndRemoveHighlightBorder();
            else
                ReAssignActionToDescriptionButton(action, actionNameString);
        });
       // Debug.LogWarning(description);
        Description_TMP.SetText(description);

     }
    private void ReAssignActionToDescriptionButton(Action action, string actionNameString)
    {
       // print("ReAssignActionToDescriptionButton, wykonanie: "+actionNameString);
        string text = Description_TMP.text;
        if(text.Contains("Show"))
        {
            text = text.Replace("Show", "Hide");
        }
        else if(text.Contains("Hide"))
        {
            text = text.Replace("Hide", "Show");
        }
        Description_TMP.SetText(text);

        if(Description_TMP.text.Contains("Empty Slot."))
        {
            Description_TMP.SetText("select new item");
            action();
            return;
        }
        Description_Button.onClick.RemoveAllListeners();
        Description_Button.onClick.AddListener(()=>
        {
            action();
            ReAssignActionToDescriptionButton(action, "repeat");
        });
    }
    
    private void CloseAndRemoveHighlightBorder()
    {
      //  print("CloseAndRemoveHighlightBorder");
        ISSELECTED = false;
        NotificationManger.HighlightElementSwitch(controller.notificationParent);
        controller.notificationParent.PossibleActions.SetActive(false);
    }

    internal void UpdateItemCounter(string countLeft)
    {
        //print("update item counter");
        ItemCounter_TMP.SetText(countLeft);
        if(Int32.Parse(countLeft) >1)
            ItemCounter_TMP.gameObject.SetActive(true);
        else
            ItemCounter_TMP.gameObject.SetActive(false);
    }
}
