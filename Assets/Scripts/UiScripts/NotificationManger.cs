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

            if(notification.IsVisibleOnNotificationList(GameManager.Player_CELL))
                notification.transform.parent.gameObject.SetActive(true);         
            else
                notification.transform.parent.gameObject.SetActive(false);

            if(notification.BaseCell.SpecialTile == null)
            {
                Destroy(notification.transform.parent.gameObject);
                NotificationList[i] = null;
            }
        }
    }
    
     public static void CreatePlayerNotificationElement(ISelectable cellRelated)
    {
       // print("CreatePlayerNotificationElement");
    
         var existingnotification = NotificationManger.instance.NotificationList.Where(c=>(c.BaseCell ==  (cellRelated as ISpecialTile).ParentCell)).FirstOrDefault();
            //int oldHierarhyPosition = 1;
            if(existingnotification != null)
                {
                  //  oldHierarhyPosition = existingnotification.transform.parent.GetSiblingIndex();
                    Destroy(existingnotification.gameObject.transform.parent.gameObject);
                //    var x = existingnotification.PossibleActions.GetComponent<ActionSwitchController>();
                //    x.Configure((cellRelated as ISpecialTile));
                //     return; // nie dodawaj tego samego 
                }

        GameObject notificationObject = Instantiate(NotificationManger.instance.NotificationPrefab,NotificationManger.instance.transform);
        notificationObject.gameObject.name = "PlayerNotifications";
        notificationObject.transform.SetAsFirstSibling();

        NotificationScript notification = notificationObject.GetComponentInChildren<NotificationScript>();
        NotificationManger.instance.NotificationList.Add(notification);
        notification.BaseCell = PlayerManager.instance._playerCell.ParentCell;

        PlayerManager.instance._actionController = notification.PossibleActions.GetComponent<ActionSwitchController>();
        notificationObject.GetComponent<Button>().onClick.AddListener(()=>
            {
                HighlightElementSwitch(notification);
                notification.PossibleActions.SetActive(!notification.PossibleActions.activeSelf);
                // if(notification.PossibleActions.activeSelf == true) 
                // {   
                //    PlayerManager.instance._actionController.ResetToDefault();

                // }

                
                  //  Debug.LogWarning("only load item in quicslot dont restart."); 
               
                  //  Debug.LogWarning("config. załadowanie wczesniej przypisanych itemkow do quickslota");
                    var quick_0 = GameManager.instance.PLAYER_PROGRESS_DATA.ItemAssignedToAuicslot_0;
                    var quick_1 = GameManager.instance.PLAYER_PROGRESS_DATA.ItemAssignedToAuicslot_1;
                    var quick_2 = GameManager.instance.PLAYER_PROGRESS_DATA.ItemAssignedToAuicslot_2;
                    var quick_3 = GameManager.instance.PLAYER_PROGRESS_DATA.ItemAssignedToAuicslot_3;
                    var quick_4 = GameManager.instance.PLAYER_PROGRESS_DATA.ItemAssignedToAuicslot_4;

                    if(quick_0 != null)
                    {
                        if(string.IsNullOrEmpty(quick_0.ScriptableObjectName) == false)
                        {
                           // Debug.LogError($"wrzucanie itemka z slotu [{quick_0.SlotID}] do quicslotu nr0");
                            PlayerManager.instance._mainBackpack.ItemSlots[quick_0.SlotID].AssignToQuickSlot(0);
                        }
                    }
                    if(quick_1 != null)
                    {
                        if(string.IsNullOrEmpty(quick_1.ScriptableObjectName) == false)
                        {
                           // Debug.LogError($"wrzucanie itemka z slotu [{quick_1.SlotID}] do quicslotu nr1");
                            PlayerManager.instance._mainBackpack.ItemSlots[quick_1.SlotID].AssignToQuickSlot(1);
                        }
                    }
                    if(quick_2 != null)
                    {
                        if(string.IsNullOrEmpty(quick_2.ScriptableObjectName) == false)
                        {
                           // Debug.LogError($"wrzucanie itemka z slotu [{quick_2.SlotID}] do quicslotu nr2");
                            PlayerManager.instance._mainBackpack.ItemSlots[quick_2.SlotID].AssignToQuickSlot(2);
                        }
                    }
                    if(quick_3 != null)
                    {
                        if(string.IsNullOrEmpty(quick_3.ScriptableObjectName) == false)
                        {
                           // Debug.LogError($"wrzucanie itemka z slotu [{quick_3.SlotID}] do quicslotu nr3");
                            PlayerManager.instance._mainBackpack.ItemSlots[quick_3.SlotID].AssignToQuickSlot(3);
                        }
                    }
                    if(quick_4 != null)
                    {
                        if(string.IsNullOrEmpty(quick_4.ScriptableObjectName) == false)
                        {
                           // Debug.LogError($"wrzucanie itemka z slotu [{quick_4.SlotID}] do quicslotu nr4");
                            PlayerManager.instance._mainBackpack.ItemSlots[quick_4.SlotID].AssignToQuickSlot(4);
                        }
                    }
            }
        );
        PlayerManager.instance._notificationScript = notification;

        existingnotification = NotificationManger.instance.NotificationList.Where(n=>n == notification).First();
        notification.PossibleActions.GetComponent<ActionSwitchController>().ConfigurePlayerButtons((cellRelated as ISpecialTile), "default");
       // Debug.Log("dodanie canvas'a do quickslota gracza");
        var canvas = existingnotification.gameObject.AddComponent<Canvas>();
        NotificationManger.instance.StartCoroutine(NotificationManger.instance.DoWhenActiveInHierarchy(canvas.gameObject, 
        ()=>{
            canvas.overrideSorting = true;
            canvas.sortingOrder = 21;}
            ));
        //Debug.LogError(canvas.gameObject.activeInHierarchy);
        existingnotification.gameObject.AddComponent<GraphicRaycaster>();

    }
    public IEnumerator DoWhenActiveInHierarchy(GameObject o, Action action)
    {
        yield return new WaitUntil(()=>o.activeInHierarchy);
        action();
    }
    public static void CreateNewNotificationElement(ISelectable cellRelated)
    {
        try
        {
            var existingnotification = NotificationManger.instance.NotificationList.Where(c=>(c.BaseCell ==  (cellRelated as ISpecialTile).ParentCell)).FirstOrDefault();
            int oldHierarhyPosition = 1;
            if(existingnotification != null)
                {
                    oldHierarhyPosition = existingnotification.transform.parent.GetSiblingIndex();
                    Destroy(existingnotification.gameObject.transform.parent.gameObject);
                //    var x = existingnotification.PossibleActions.GetComponent<ActionSwitchController>();
                //    x.Configure((cellRelated as ISpecialTile));
                //     return; // nie dodawaj tego samego 
                }
            GameObject notificationObject = Instantiate(NotificationManger.instance.NotificationPrefab,NotificationManger.instance.transform);
            notificationObject.transform.SetSiblingIndex(oldHierarhyPosition);
            notificationObject.transform.SetAsLastSibling();
            NotificationScript notification = notificationObject.GetComponentInChildren<NotificationScript>();
            NotificationManger.instance.NotificationList.Add(notification);
            notificationObject.name = "Notification"+NotificationManger.instance.NotificationList.Count;
            notification.BaseCell = (cellRelated as ISpecialTile).ParentCell;

            notificationObject.GetComponent<Button>().onClick.AddListener(()=>
                {
                    HighlightElementSwitch(notification);
                    notification.PossibleActions.SetActive(!notification.PossibleActions.activeSelf);
                    if(notification.PossibleActions.activeSelf == true) 
                    {   
                        // print("reset to default, okno possible actions jest nieaktywne");
                       // print("click");
                        notification.PossibleActions.GetComponent<ActionSwitchController>().ResetToDefault();
        

                
                    }
               }
            );
             
        }
        catch (System.Exception)
        {
            
           // Debug.LogError("blad"+ex.Message);
        }
        
    }
    public static void HighlightElementSwitch(NotificationScript notification, bool? state = null)
    {
        ISelectable selectableCell = (notification.BaseCell.SpecialTile as ISelectable);
        if(state == null)
        {
            if(notification.BaseCell.SpecialTile is ISelectable == false) return;

            if(notification.SelectBorder.GetComponent<Image>().enabled)
                turnOFF(notification);
            else
                turnON(notification);
        }
        else if(state == true)
            turnON(notification);
        else if(state == false)
            turnOFF(notification);

        void turnON(NotificationScript notification)
        {
            notification.SelectBorder.GetComponent<Image>().enabled = true;
            selectableCell.IsHighlighted = true;
            ShowBorder(selectableCell, Color.green);
        }

        void turnOFF(NotificationScript notification)
        {
            notification.SelectBorder.GetComponent<Image>().enabled = false;
            selectableCell.IsHighlighted = false;

            HideBorder(selectableCell, 0f); // ukrycie natychmiast
        }
    }
    public static void TemporaryHideBordersOnMap(NotificationScript notification, bool hide)
    {
        ISelectable selectableCell = (notification.BaseCell.SpecialTile as ISelectable);
        if(selectableCell == null)
        {
            
            // Debug.LogError("O CO CHODZI ?");
            return;
        } 

        if(hide == true)
            hideModeTurnON(notification);
        else if(hide == false)
            hideModeTurnOFF(notification);

        void hideModeTurnON(NotificationScript notification)
        {
            if(selectableCell.Border == null) return;;
            selectableCell.Border.GetComponent<Image>().enabled = false;
        }

        void hideModeTurnOFF(NotificationScript notification)
        {
            if(selectableCell.Border == null) return;;
            selectableCell.Border.GetComponent<Image>().enabled = true;
        }
    }
    public static void ShowBorder(ISelectable cell, Color32 color)
    {
        if(cell == null) return;
        if (cell.Border == null) 
        {
            cell.Border = GameObject.Instantiate(GameManager.instance.SelectionBorderPrefab, (cell as ISpecialTile).ParentCell.transform);
        }
        cell.Border.GetComponent<Image>().color = color;  
    }
    public static List<GameObject> HighlightAreaWithTemporaryBorders(List<Vector2Int> _area, Color32 _color)
    {
        _color = new Color32(_color.r, _color.g, _color.b, 150);
        List<GameObject> temporaryBorderOverlays = new List<GameObject>();
        foreach(var cell in _area)
        {
            var border = GameObject.Instantiate(GameManager.instance.SelectionBorderPrefab,GridManager.CellGridTable[cell].transform);
            border.GetComponent<Image>().color = _color;
            border.GetComponent<Button>().enabled = false;
            border.transform.localScale = new Vector3(.94f,.94f,.94f);
            temporaryBorderOverlays.Add(border);
        }
        return temporaryBorderOverlays;
    }
    public static void RemoveTemporaryBordersObjectsFromArea(List<GameObject> _area)
    {
        foreach(var temporaryBorder in _area)
        {   
            ActionSwitchController.Destroy(temporaryBorder);
        }
    }

    public static void HideBorder(ISelectable cell, float timeDelay)
    {
        if(cell == null) return;
        if (cell.Border == null) return;

        if(cell.IsHighlighted == true)
        { 
            GameManager.instance.StartCoroutine(ChangeBorder(timeDelay, Color.red));
        }
        else
        {

            GameObject.Destroy(cell.Border, timeDelay);
        }

        IEnumerator ChangeBorder(float time, Color32 color)
        {
            cell.Border.GetComponent<Image>().color = color;
            yield return new WaitForSeconds(time);
            if(cell.Border != null)
                cell.Border.GetComponent<Image>().color = Color.green;    
        }
    }
    public static void TriggerActionNotification(ISelectable INVOKER, AlertCategory CATEGORY,string message = "", ISpecialTile TARGET_BaseCEll = null)
    {try
    {
        NotificationScript Invoker_BaseCell_Notification = instance.NotificationList.FirstOrDefault(n=>n.BaseCell.SpecialTile as ISelectable == INVOKER);
      
        NotificationScript Target_BaseCell_Notification = instance.NotificationList.FirstOrDefault(n=>n.BaseCell.SpecialTile == TARGET_BaseCEll);

        if(Invoker_BaseCell_Notification == null && CATEGORY != AlertCategory.ExplosionDamage) return;

        ISpecialTile INVOKER_BaseCell = Invoker_BaseCell_Notification.BaseCell.SpecialTile;
        
        switch(CATEGORY)
        {
            case AlertCategory.Attack:
                Configure_Attack_Notification(
                    invoker_BaseCell:INVOKER_BaseCell, 
                    invoker_Notification:Invoker_BaseCell_Notification
                );
                break;

            case AlertCategory.Loot:
                Configure_Loot_Notification(
                    invoker_BaseCell:INVOKER_BaseCell, 
                    invoker_Notification:Invoker_BaseCell_Notification
                );
                break;

            case AlertCategory.PlayerAttack:
                Configure_PlayerAttack_Notification(
                    invoker_BaseCell:INVOKER_BaseCell, 
                    invoker_Notification:Invoker_BaseCell_Notification
                ); 
                break;

            case AlertCategory.ExplosionDamage:
                Configure_ExplosionDamage_Notification(
                    invoker_BaseCell:INVOKER_BaseCell,                      // <- zeb ypobrać siłe wybuchu i ją potem wpisac na powiadomieniu
                    invoker_Notification:Invoker_BaseCell_Notification,     // TODO: <- raczej nie przyda sie dla bomby
                    target_BaseCell:TARGET_BaseCEll, 
                    target_Notification:Target_BaseCell_Notification        // <- target czyli player,monster w tym przypadku
                    ); 
                break;

             case AlertCategory.Info:
                Configure_Info_Notification(
                    invoker_BaseCell:INVOKER_BaseCell, 
                    invoker_Notification:Invoker_BaseCell_Notification, 
                    message
                );
                break;
        }
    }
    catch (System.Exception)
    {
        
       // Debug.LogError(ex.Message);
    }
       
    }
    private static void Configure_Info_Notification(ISpecialTile invoker_BaseCell, NotificationScript invoker_Notification, string message)
    {
        // Przypisanie odpowiedniego koloru ramce.
        Color32 color = Color.white;
        GameObject Alert = instance.AlertPrefab;
        Alert.GetComponent<AlertScript>().Color = color;

        // usunięcie ewentualnego duplikatu wcześniejszym powiadomieniem na tej samej karcie.
        AlertScript existingNotificationAlert = invoker_Notification.transform.parent.GetComponentInChildren<AlertScript>();
        if (existingNotificationAlert != null)
            Destroy(existingNotificationAlert);

        // Spawn obiektu powiadomienia wyłącznie na panelu list i umieszczenie go nad innymi, z wierzchu.
        var newNotificationOverlay = Instantiate(Alert, invoker_Notification.transform.parent.gameObject.transform);
        newNotificationOverlay.transform.SetAsLastSibling();
        newNotificationOverlay.GetComponent<AlertScript>().text.SetText(message);

        // Spawn borderka na mapie i usuniecie po 1s.
        ShowBorder(invoker_BaseCell as ISelectable, color);
        HideBorder(invoker_BaseCell as ISelectable, .5f);
    }
    private static void Configure_ExplosionDamage_Notification(ISpecialTile invoker_BaseCell, NotificationScript invoker_Notification,ISpecialTile target_BaseCell, NotificationScript target_Notification)
    {
         // Pobranie ataku jaki posiada przeciwnik.
        if(target_BaseCell is ILivingThing == false)
        {
          //  Debug.LogError($"cos nie tak dla {Invoker_BaseCell.ParentCell.name} ",context:Invoker_BaseCell.ParentCell.gameObject);
            return;
        }
        int damageValue = (invoker_BaseCell as Bomb_Cell).BombDamage * -1;

        // Przypisanie odpowiedniego koloru ramce.
        Color32 color = Color.magenta;
        GameObject Alert = instance.AlertPrefab;
        Alert.GetComponent<AlertScript>().Color = color;

        // usunięcie ewentualnego duplikatu wcześniejszym powiadomieniem na tej samej karcie.
        AlertScript existingNotificationAlert = target_Notification.transform.parent.GetComponentInChildren<AlertScript>();
        if (existingNotificationAlert != null)
            Destroy(existingNotificationAlert);

        // Spawn obiektu powiadomienia wyłącznie na panelu list i umieszczenie go nad innymi, z wierzchu.
        var newNotificationOverlay = Instantiate(Alert, target_Notification.transform.parent.gameObject.transform);
        newNotificationOverlay.transform.SetAsLastSibling();
        newNotificationOverlay.GetComponent<AlertScript>().text.SetText($"Explosion damaged: {-damageValue} DMG.");

        // Spawn borderka na mapie i usuniecie po 1s.
        ShowBorder(target_BaseCell as ISelectable, color);
        HideBorder(target_BaseCell as ISelectable, .5f);

        if (target_BaseCell.Type != TileTypes.player) return;
        // sekcja głównych statystyk: HP , jeżeli obiekty sie stakują, przes usunięciem duplikatu, dodaj jego wartość i zostaw tylko jednego z sumą wcześniejszych
        AddValueTo_Health_Notification(damageValue);

    }
    private static void Configure_PlayerAttack_Notification(ISpecialTile invoker_BaseCell, NotificationScript invoker_Notification)
    {
       // Debug.LogWarning("PLAYER NOTIFICATION");
        float damageValue =  (invoker_BaseCell as ILivingThing).Damage;
        Color32 color = Color.yellow;

        GameObject Alert = instance.AlertPrefab;
        Alert.GetComponent<AlertScript>().Color = color;

        GameObject newNotificationOverlay = Instantiate(Alert, invoker_Notification.transform.parent.gameObject.transform);
        newNotificationOverlay.transform.SetAsLastSibling();

        ShowBorder(invoker_BaseCell as ISelectable,color);
        HideBorder(invoker_BaseCell as ISelectable,.5f);
    }
    private static void Configure_Loot_Notification(ISpecialTile invoker_BaseCell, NotificationScript invoker_Notification)
    {
        // przypisanie odpowiedniego koloru ramce
        Color32 color = Color.yellow;
        GameObject Alert = instance.AlertPrefab;
        Alert.GetComponent<AlertScript>().Color = color;

        // usunięcie ewentualnego duplikatu wcześniejszym powiadomieniem na tej samej karcie.
        AlertScript existingNotificationAlert = invoker_Notification.transform.parent.GetComponentInChildren<AlertScript>();
        if(existingNotificationAlert != null)
            Destroy(existingNotificationAlert);
        
        // Spawn obiektu powiadomienia wyłącznie na panelu list i umieszczenie go nad innymi, z wierzchu.
        GameObject newNotificationOverlay = Instantiate(Alert, invoker_Notification.transform.parent.gameObject.transform);
        newNotificationOverlay.transform.SetAsLastSibling();
        newNotificationOverlay.GetComponent<AlertScript>().text.SetText($"Monster defeated !"); 

        // Spawn borderka na mapie i usuniecie po 1s.
        ShowBorder(invoker_BaseCell as ISelectable,color);
        HideBorder(invoker_BaseCell as ISelectable,.5f);
    }
    private static void Configure_Attack_Notification(ISpecialTile invoker_BaseCell, NotificationScript invoker_Notification)
    {
        // Pobranie ataku jaki posiada przeciwnik.
        float damageValue = (invoker_BaseCell as ICreature).Damage *-1;

        // Przypisanie odpowiedniego koloru ramce.
        Color32 color = Color.red;
        GameObject Alert = instance.AlertPrefab;
        Alert.GetComponent<AlertScript>().Color = color;

        // usunięcie ewentualnego duplikatu wcześniejszym powiadomieniem na tej samej karcie.
        AlertScript existingNotificationAlert = invoker_Notification.transform.parent.GetComponentInChildren<AlertScript>();
        if(existingNotificationAlert != null)
            Destroy(existingNotificationAlert);
        
        // Spawn obiektu powiadomienia wyłącznie na panelu list i umieszczenie go nad innymi, z wierzchu.
        var newNotificationOverlay = Instantiate(Alert, invoker_Notification.transform.parent.gameObject.transform);
            newNotificationOverlay.transform.SetAsLastSibling();
            newNotificationOverlay.GetComponent<AlertScript>().text.SetText($"Dealt {-damageValue} DMG.");

        // Spawn borderka na mapie i usuniecie po 1s.
        ShowBorder(invoker_BaseCell as ISelectable,color);
        HideBorder(invoker_BaseCell as ISelectable,.5f);

        AddValueTo_Health_Notification(Mathf.RoundToInt(damageValue));
    }

    // -------------------- [Player related UI notification] --------------------
    public static void AddValueTo_Health_Notification(int damageValue)
    {
        // AlertScript notificationToModife = null;
        // int currentValue = 0;
        // AlertScript existingHPAlert = GameObject.Find("HealthTab").transform.Find("HP").GetComponentInChildren<AlertScript>();
        // if (existingHPAlert == null)
        // {
        //     // jest to pierwsze powiadomienie, dodaj go, ustaw na wierzchu, i zapisz bierzącą wartość 
        //     //      unikalne dla HP, jeżeli wartość będzie powyżej 0, kolor z czerwonego zmieni sie na zielony.
        //     var healthSectionOverlay = Instantiate(instance.AlertPrefab, GameObject.Find("HealthTab").transform.Find("HP").transform);
        //     healthSectionOverlay.transform.SetAsLastSibling();
        //     notificationToModife = healthSectionOverlay.GetComponent<AlertScript>();
        //     currentValue = damageValue;
        // }
        // else
        // {
        //     notificationToModife = existingHPAlert;
        //     currentValue = Int32.Parse(existingHPAlert.TextValue);
        //     currentValue += damageValue;
        // }

        // // Przypisanie zsumowanej wartośc na polu HP.
        // notificationToModife.TextValue = currentValue.ToString();
        // notificationToModife.text.text = notificationToModife.TextValue;
        // if (currentValue > 0)
        // {
        //     notificationToModife.text.text = "+"+notificationToModife.TextValue;
        //     notificationToModife.Color = Color.green;
        // }
    }
    public static void AddValueTo_Gold_Notification(int goldValue)
    {
        //print("AddValueTo_Gold_Notification");
        // AlertScript notificationToModife = null;
        // int currentValue = 0;
        // AlertScript existingGoldAlert = GameObject.Find("GoldTab").transform.Find("GOLD").GetComponentInChildren<AlertScript>();
        // if (existingGoldAlert == null)
        // {
        //     // jest to pierwsze powiadomienie, dodaj go, ustaw na wierzchu, i zapisz bierzącą wartość 
        //     //      unikalne dla HP, jeżeli wartość będzie powyżej 0, kolor z czerwonego zmieni sie na zielony.
        //     var healthSectionOverlay = Instantiate(instance.AlertPrefab, GameObject.Find("GoldTab").transform.Find("GOLD").transform);
        //     healthSectionOverlay.transform.SetAsLastSibling();
        //     notificationToModife = healthSectionOverlay.GetComponent<AlertScript>();
        //     currentValue = goldValue;
        // }
        // else
        // {
        //     notificationToModife = existingGoldAlert;
        //     currentValue = Int32.Parse(existingGoldAlert.TextValue);
        //     currentValue += goldValue;
        // }

        // // Przypisanie zsumowanej wartośc na polu HP.
        // notificationToModife.Color = Color.yellow;
        // notificationToModife.TextValue = currentValue.ToString();
        // notificationToModife.text.text = notificationToModife.TextValue;
        // if (currentValue < 0)
        //     notificationToModife.Color = Color.red;
    }

}

