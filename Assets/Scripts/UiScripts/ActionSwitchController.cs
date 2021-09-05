using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSwitchController : MonoBehaviour
{
    [SerializeField] public NotificationScript notificationParent;
    [SerializeField]  public List<ActionButtonScript> actionButtonsList = new List<ActionButtonScript>();
    [SerializeField] int ColorDimmingSteps {get;set;} = 4;
    private bool AnimationIsRunning = false;
    
    private void Start()
    {
        notificationParent = GetComponentInParent<NotificationScript>();
        Configure(notificationParent.BaseCell.SpecialTile);
    }
    public void OnClick_SelectActionIcon(ActionButtonScript selectedButton)
    {
        if(AnimationIsRunning) return;
        if(notificationParent.BaseCell.SpecialTile is Player_Cell)
        {
            if(EquipmentScript.AssignationItemToQuickSlotIsActive)
            {
                Debug.Log("quit from AssignationItemToQuickSlotIsActive mode");
                EquipmentScript.QuitFromQuickbarSelectionMode();
            }

        }
        AnimationIsRunning = true;
        StartCoroutine(AnimateSelection(selectedButton));
    }
    public void OnClick_ExecuteDescriptionActionAndClose(ActionButtonScript selectedButton)
    {
        if(AnimationIsRunning) return;
        AnimationIsRunning = true;
        StartCoroutine(AnimateDeselection(selectedButton));
    }
    public void ResetToDefault()
    {
        foreach(var button in actionButtonsList)
        {
            button.gameObject.SetActive(true);
            button.ButtonIcon_IMG.color = Color.white;
            button.ISSELECTED = false;
            button.HideDescription();
        }
    }
    
    public void ConfigurePlayerButtons(ISpecialTile cell,string actionNameString="")
    {
        int i =0;
        foreach(var button in actionButtonsList)
        {
            if(i<cell.AvaiableActions.Count)
            {
                button.gameObject.SetActive(true);
                button.ConfigureIconButtonClick(
                    action:()=>OnClick_SelectActionIcon(button),
                    cell.AvaiableActions[i].icon
                );
                button.ConfigureDescriptionButtonClick(
                    action: cell.AvaiableActions[i].action,
                    description: cell.AvaiableActions[i].description,
                    singleAction: cell.AvaiableActions[i].singleAction,
                    actionNameString: actionNameString);
            }
           // print(button.name);
            i++;
        }            
    }       
    public void Configure(ISpecialTile cell)
    {
        int i =0;
        ActionButtonScript[] temp = new ActionButtonScript[actionButtonsList.Count];
        actionButtonsList.CopyTo(temp);

        foreach(var button in temp)
        {
            if(cell.AvaiableActions == null) continue;
            if(i<cell.AvaiableActions.Count)
            {
                button.gameObject.SetActive(true);
                button.ConfigureIconButtonClick(
                    action:()=>OnClick_SelectActionIcon(button),
                    cell.AvaiableActions[i].icon
                );
                button.ConfigureDescriptionButtonClick(
                    action: cell.AvaiableActions[i].action,
                    description: cell.AvaiableActions[i].description,
                    singleAction: cell.AvaiableActions[i].singleAction
                );
            }
            else
            {
                actionButtonsList.Remove(button);
                Destroy(button.gameObject);
            }
            i++;
        }                   

    }

    public void Refresh(ISpecialTile cell)
    {
        if(cell is Player_Cell) return;

        Debug.LogWarning("Refresh opened window in ACTION BUTTONS");
        int i =0;
        ActionButtonScript[] temp = new ActionButtonScript[actionButtonsList.Count];
        actionButtonsList.CopyTo(temp);

        foreach(var button in temp)
        {
            if(cell.AvaiableActions == null) continue;
            if(i<cell.AvaiableActions.Count)
            {
              //  button.gameObject.SetActive(true);
                button.ConfigureIconButtonClick(
                    action:()=>OnClick_SelectActionIcon(button),
                    cell.AvaiableActions[i].icon
                );
                button.ConfigureDescriptionButtonClick(
                    action: cell.AvaiableActions[i].action,
                    description: cell.AvaiableActions[i].description,
                    singleAction: cell.AvaiableActions[i].singleAction
                );
            }
            else
            {

                actionButtonsList.Remove(button);
                Destroy(button.gameObject);
            }
            i++;
        }                   

    }
    private IEnumerator AnimateSelection(ActionButtonScript selectedButton)
    {
      //  print("AnimateSelection = change color");
        // make others semi transparent 
        float colorIncrementvalue = 1f/(float)ColorDimmingSteps;
        var endColor = new Color32(255,255,255,90);
        float currentIncrement = 0;
        actionButtonsList.Where(s=>s==selectedButton).FirstOrDefault().SelectionBorder_Object.SetActive(true);
        while(true)
        {
            if(currentIncrement >1)
                break;
                currentIncrement+=colorIncrementvalue;
            yield return new WaitForFixedUpdate();  
            foreach(var button in actionButtonsList)
            {   
                if(button == selectedButton) {
                    
                    continue;
                }
                //print(Color32.Lerp(Color.white,endColor,currentIncrement));
                button.ButtonIcon_IMG.color = Color32.Lerp(Color.white,endColor,currentIncrement);        
            }
        }

        foreach(var button in actionButtonsList)
        { 
            if(button == selectedButton) 
            {   
                button.ShowDescription();
                continue;
            }
            button.gameObject.SetActive(false);
        }
        
        AnimationIsRunning = false;
    }
    private IEnumerator AnimateDeselection(ActionButtonScript selectedButton)
    {
        
      //  ("AnimateDeselection => change color ");
        // make others semi transparent 
        float colorIncrementvalue = 1f/(float)ColorDimmingSteps;
        var startColor = new Color32(255,255,255,90);
        float currentIncrement = 0;
 
        foreach(var button in actionButtonsList)
        { 
            if(button == selectedButton) 
            {   
                button.HideDescription();
                continue;
            }
            button.gameObject.SetActive(true);
        }

        while(true)
        {
        //    print(currentIncrement);

            if(currentIncrement >1)
                break;
            
            currentIncrement+=colorIncrementvalue;
            yield return new WaitForFixedUpdate();

            foreach(var button in actionButtonsList)
            {
                if(button == selectedButton) continue;
             //   print(Color32.Lerp(startColor,Color.white,currentIncrement));
                button.ButtonIcon_IMG.color =  Color32.Lerp(startColor, Color.white,currentIncrement);        
            }            
        }
        AnimationIsRunning = false;
    }

}
