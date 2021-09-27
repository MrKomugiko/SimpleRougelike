using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabsSectionScript : MonoBehaviour
{
    [SerializeField] private GameObject CharacterTab_Notification;
    [SerializeField] private GameObject EquipmentTab_Notification;
    [SerializeField] private GameObject SpellBookTab_Notification;

    public void Visit(string tabName)
    {
        switch(tabName)
        {
            case "CharacterTab":
                CharacterTab_Notification.SetActive(false);
            return;

            case "EquipmentTab":
                EquipmentTab_Notification.SetActive(false);
            return;

            case "SpellBookTab":
                SpellBookTab_Notification.SetActive(false);
            return;
        }
    }

    public void ShowNotificationOnTab(TabNames tabName)
    {
    
        switch(tabName.ToString())
        {
            case "CharacterTab":
                CharacterTab_Notification.SetActive(true);
            return;

            case "EquipmentTab":
                EquipmentTab_Notification.SetActive(true);
            return;

            case "SpellBookTab":
                SpellBookTab_Notification.SetActive(true);
            return;
        }
    }

}
    public enum TabNames
    {
        CharacterTab,
        EquipmentTab,
        SpellBookTab

    }
