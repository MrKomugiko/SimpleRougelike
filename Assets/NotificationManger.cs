using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class NotificationManger : MonoBehaviour
{    
    public static NotificationManger instance;
    [SerializeField] GameObject AlertPrefab;
    [SerializeField] GameObject NotificationPrefab;
    [SerializeField] int maxSize = 5; // TODO: 
    [SerializeField] public List<NotificationScript> NotificationList = new List<NotificationScript>();
    
    
    private void Awake() 
    {
        instance = this;
    }
    private void Start() 
    {
        InvokeRepeating("RefreshNotifications",0,.1f);
    }
    private void RefreshNotifications() {   
        NotificationList.RemoveAll(n=>n==null);
        int count = NotificationList.Count;
        for (int i = 0; i < count; i++)
        {
            NotificationScript notification = NotificationList[i];
            notification.RefreshData();     

            // check if is in range
            if(notification.IsVisibleOnNotificationList(GameManager.Player_CELL))
                notification.transform.parent.gameObject.SetActive(true);         
            else
                notification.transform.parent.gameObject.SetActive(false);

             // check if deleted
            if(notification.BaseCell.SpecialTile == null)
            {
                //Debug.LogWarning("usuwanie elementu notification");
                Destroy(notification.transform.parent.gameObject);
                NotificationList[i] = null;
            }
        }
    }
    
    public static void CreateNewNotificationElement(ISelectable cellRelated)
    {
        if(NotificationManger.instance.NotificationList.Where(c=>(c.BaseCell ==  (cellRelated as ISpecialTile).ParentCell)).Any())
            return; // nie dodawaj tego samego 

        GameObject notificationObject = Instantiate(NotificationManger.instance.NotificationPrefab,NotificationManger.instance.transform);
        NotificationScript notification = notificationObject.GetComponentInChildren<NotificationScript>();
        NotificationManger.instance.NotificationList.Add(notification);
        notificationObject.name = "Notification"+NotificationManger.instance.NotificationList.Count;
        notification.BaseCell = (cellRelated as ISpecialTile).ParentCell;
        // print("dodanie basecell do notification script:"+ (cellRelated as ISpecialTile).Icon_Url);
        notificationObject.GetComponent<Button>().onClick.AddListener(()=>
            {
                HighlightElementSwitch(notification);
                notification.PossibleActions.SetActive(!notification.PossibleActions.activeSelf);
                if(notification.PossibleActions.activeSelf == true) 
                {   
                    print("reset to default, okno possible actions jest nieaktywne");
                    notification.PossibleActions.GetComponent<ActionSwitchController>().ResetToDefault();
                }
            }
        );
        
    }
    public static void HighlightElementSwitch(NotificationScript notification, bool? state = null)
    {
        print("highlight");
        ISelectable selectableCell = (notification.BaseCell.SpecialTile as ISelectable);
        if(state == null)
        {
            if(notification.BaseCell.SpecialTile is ISelectable == false) return;

            if(notification.SelectBorder.GetComponent<Image>().enabled)
                turnON(notification);
            else
                turnOFF(notification);
        }
        else if(state == true)
            turnON(notification);
        else
            turnOFF(notification);

        void turnON(NotificationScript notification)
        {
            notification.SelectBorder.GetComponent<Image>().enabled = false;
            selectableCell.IsHighlighted = false;

            HideBorder(selectableCell, 0f); // ukrycie natychmiast
        }

        void turnOFF(NotificationScript notification)
        {
            notification.SelectBorder.GetComponent<Image>().enabled = true;
            selectableCell.IsHighlighted = true;
            ShowBorder(selectableCell, Color.green);
        }
    }
    public static void ShowBorder(ISelectable cell, Color32 color)
    {
        if (cell.Border == null) 
        {
            cell.Border = GameObject.Instantiate(GameManager.instance.SelectionBorderPrefab, (cell as ISpecialTile).ParentCell.transform);
        }
        cell.Border.GetComponent<Image>().color = color;  
       // NotificationManger.CreateNewNotificationElement((cell as ISpecialTile));
    
    }
    public static void HideBorder(ISelectable cell, float timeDelay)
    {
        if(cell == null) return;
        if (cell.Border == null) return;

        if(cell.IsHighlighted == true)
        { 
            GameManager.instance.ExecuteCoroutine(ChangeBorder(timeDelay, Color.red));
        }
        else
        {

            GameObject.Destroy(cell.Border, timeDelay);
        }

        IEnumerator ChangeBorder(float time, Color32 color)
        {
            cell.Border.GetComponent<Image>().color = color;
            yield return new WaitForSeconds(time);
            cell.Border.GetComponent<Image>().color = Color.green;    
        }
    }
   
    public static void TriggerActionNotification(ISelectable cellInvokingAlert, AlertCategory category)
    {
        // 1 spawn  alertu w kontenerze ui np hp, exp
        // 2 spawn  alertu wewnątrz elementu notyfication
        // 3 highlight related cell

        NotificationScript Invoker_Notification = instance.NotificationList.FirstOrDefault(n=>n.BaseCell.SpecialTile as ISelectable == cellInvokingAlert);

        if(Invoker_Notification == null) return;

        ISpecialTile Invoker_BaseCell = Invoker_Notification.BaseCell.SpecialTile;
        
        switch(category)
        {
            case AlertCategory.Attack:
            Debug.LogWarning("MONSTER ATTACK NOTIFICATION");

                int damageValue = (Invoker_BaseCell as ICreature).Damage *-1;
                Color32 color = Color.red;
                Debug.LogWarning(Invoker_BaseCell.Name +" wykonuje "+category.ToString());

                GameObject Alert = instance.AlertPrefab;
                Alert.GetComponent<AlertScript>().Color = color;

                AlertScript existingNotificationAlert = Invoker_Notification.transform.parent.GetComponentInChildren<AlertScript>();
                if(existingNotificationAlert != null)
                {
                    Destroy(existingNotificationAlert);
                }
                var newNotificationOverlay = Instantiate(Alert, Invoker_Notification.transform.parent.gameObject.transform);
                    newNotificationOverlay.transform.SetAsLastSibling();
                    newNotificationOverlay.GetComponent<AlertScript>().text.SetText($"Dealt {-damageValue} DMG.");

                ShowBorder(Invoker_BaseCell as ISelectable,color);
                HideBorder(Invoker_BaseCell as ISelectable,1f);

                AlertScript existingHPAlert = GameObject.Find("HealthTab").transform.Find("HP").GetComponentInChildren<AlertScript>();
                if(existingHPAlert == null)
                {
                    var healthSectionOverlay = Instantiate(Alert, GameObject.Find("HealthTab").transform.Find("HP").transform);
                    healthSectionOverlay.transform.SetAsLastSibling();
                    AlertScript newAlert = healthSectionOverlay.GetComponent<AlertScript>();
                    newAlert.TextValue = damageValue.ToString();
                    newAlert.text.text = newAlert.TextValue;
                    if(damageValue>0)
                        newAlert.Color = Color.green;
                }
                else
                {
                    int currentValue = Int32.Parse(existingHPAlert.TextValue);
                    currentValue += damageValue;
                    existingHPAlert.TextValue = currentValue.ToString();
                    existingHPAlert.text.text = existingHPAlert.TextValue;
                    if(currentValue>0)
                        existingHPAlert.Color = Color.green;
                       
                }
            break;


            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

            case AlertCategory.Loot:
            Debug.LogWarning("LOOT NOTIFICATION");

                color = Color.yellow;

                Alert = instance.AlertPrefab;
                Alert.GetComponent<AlertScript>().Color = color;

                 newNotificationOverlay = Instantiate(Alert, Invoker_Notification.transform.parent.gameObject.transform);
                    newNotificationOverlay.transform.SetAsLastSibling();
                    newNotificationOverlay.GetComponent<AlertScript>().text.SetText($"Monster defeated !");
                 

                ShowBorder(Invoker_BaseCell as ISelectable,color);
                HideBorder(Invoker_BaseCell as ISelectable,1f);

            break;

             //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

            case AlertCategory.PlayerAttack:
            Debug.LogWarning("PLAYER NOTIFICATION");
                damageValue = 1; // TODO: PLAYER SPECIAL CALSS !!!!!!!!!!!!
                color = Color.yellow;

                // existingAlert = Invoker_Notification.transform.parent.gameObject.transform.GetComponentInChildren<AlertScript>();
                // if(existingAlert == null)
                // {
                    Alert = instance.AlertPrefab;
                    Alert.GetComponent<AlertScript>().Color = color;

                    newNotificationOverlay = Instantiate(Alert, Invoker_Notification.transform.parent.gameObject.transform);
                    newNotificationOverlay.transform.SetAsLastSibling();
                    newNotificationOverlay.GetComponent<AlertScript>().text.SetText($"Attacked by {damageValue} DMG !");
                // }
                // else
                // {
                    
                //}
                ShowBorder(Invoker_BaseCell as ISelectable,color);
                HideBorder(Invoker_BaseCell as ISelectable,1f);
    
            
            break;
        }


    }
}

