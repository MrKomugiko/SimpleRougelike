using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManger : MonoBehaviour
{
    public static NotificationManger instance;
    private void Awake() {
        instance = this;
    }
    private void Start() {
        InvokeRepeating("RefreshNotifications",0,.1f);
    }
    [SerializeField] GameObject NotificationPrefab;

    [SerializeField] int maxSize = 5;

    [SerializeField] public List<NotificationScript> NotificationList = new List<NotificationScript>();


    public static void CreateNewNotificationElement(ISelectable cellRelated)
    {
        if(NotificationManger.instance.NotificationList.Where(c=>(c.BaseCell ==  (cellRelated as ISpecialTile).ParentCell)).Any())
        {
            print("znajduje sie duplikat");
            return; // nie dodawaj tego samego
        }

        GameObject notificationObject = Instantiate(NotificationManger.instance.NotificationPrefab,NotificationManger.instance.transform);
        NotificationScript notification = notificationObject.GetComponentInChildren<NotificationScript>();
        NotificationManger.instance.NotificationList.Add(notification);
        notificationObject.name = "Notification"+NotificationManger.instance.NotificationList.Count;
        notification.BaseCell = (cellRelated as ISpecialTile).ParentCell;
        print("dodanie basecell do notification script:"+ (cellRelated as ISpecialTile).Icon_Url);
        notificationObject.GetComponent<Button>().onClick.AddListener(()=>HighlightElement(notification));
    }
    private static void HighlightElement(NotificationScript notification)
    {
        if(notification.BaseCell.SpecialTile is ISelectable == false) return;

        ISelectable selectableCell = (notification.BaseCell.SpecialTile as ISelectable);
        if(notification.SelectBorder.GetComponent<Image>().enabled)
        {
            notification.SelectBorder.GetComponent<Image>().enabled = false;
            selectableCell.IsHighlighted = false;

            HideBorder(selectableCell, 0f); // ukrycie natychmiast
        }
        else
        {
            notification.SelectBorder.GetComponent<Image>().enabled = true;
            selectableCell.IsHighlighted = true;
           ShowBorder(selectableCell,Color.green);
        }
    }
    private void RefreshNotifications() {   
        NotificationList.RemoveAll(n=>n==null);
        int count = NotificationList.Count;
        for (int i = 0; i < count; i++)
        {
            NotificationScript notification = NotificationList[i];
            notification.RefreshData();     

            // check if is in range
            if(notification.IsInRange(GameManager.Player_CELL))
                notification.transform.parent.gameObject.SetActive(true);         
            else
                notification.transform.parent.gameObject.SetActive(false);

             // check if deleted
            if(notification.BaseCell.SpecialTile == null)
            {
                Debug.LogWarning("usuwanie elementu notification");
                Destroy(notification.transform.parent.gameObject);
                NotificationList[i] = null;
            }
        }
    }

    // static public HashSet<ISelectable> SelectableOnMap = new HashSet<ISelectable>();
    // static public void RefreshSelectableList(ISelectable cell)
    // {
    //     if(SelectableOnMap.Contains(cell)) return;

    //     SelectableOnMap.Add(cell);
    //     print("dodanie elementu do listy, lista zawiera"+SelectableOnMap.Count);
    // }

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

    internal static void RemoveFromNotification(ISpecialTile cell)
    {
        
    }
}

