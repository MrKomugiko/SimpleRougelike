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
    [SerializeField] private TextMeshProUGUI Description_TMP;
    [SerializeField] private Button Icon_Button;
    [SerializeField] private Button Description_Button;
    [SerializeField] private ActionSwitchController controller;
    [SerializeField] private bool _iSSELECTED = false;
    private Action myMainIconAction = null;
    public bool ISSELECTED { 
        get => _iSSELECTED; 
        set 
        {
             _iSSELECTED = value; 
            if(value == true)
            {
                Icon_Button.onClick.RemoveAllListeners();
                Icon_Button.onClick.AddListener(()=>controller.ResetToDefault()); // wróć      
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
        myMainIconAction = action;
        Icon_Button.onClick.AddListener(()=>action());;
        ButtonIcon_IMG.sprite = IconSpriteList.First(n=>n.name == "ICON_"+icon.ToString());
    }
    public void ConfigureDescriptionButtonClick(Action action, string description)
    { 
        this.gameObject.name = "ACTION_"+description;
        Description_TMP.SetText(description);
        Description_Button.onClick.AddListener(()=>
        {
            print("wykonuje akcje");
            action();
            CloseAndRemoveHighlightBorder();
        });
    }
    
    private void CloseAndRemoveHighlightBorder()
    {
        ISSELECTED = false;
        NotificationManger.HighlightElementSwitch(controller.notificationParent);
        controller.notificationParent.PossibleActions.SetActive(false);
    }
}
