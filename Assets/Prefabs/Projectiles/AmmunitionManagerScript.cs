using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AmmunitionItem;
using static Chest;

public class AmmunitionManagerScript : MonoBehaviour
{
    public ItemSlot AmmoSlot;

    private void OnEnable() {
        RefreshAmmoCounter();
    }
    
    private void OnDisable() {
         AmmoList.SetActive(false);
    }

    [SerializeField] GameObject AmmoList;
    public void OnClick_ShowAmmoList()
    {
        AmmoList.SetActive(!AmmoList.activeSelf);
    }

    public static AmmunitionItem CurrentDefault = null;
    public static void SetAmmoItemAsDefault(AmmunitionItem selectedAmmoItem)
    {
        CurrentDefault = selectedAmmoItem;
        Debug.Log("zmiana defaultowej amunicji na" + CurrentDefault.name);
       PlayerManager.instance._EquipedItems.AmmoManager.RefreshAmmoCounter();
    }

    public void RefreshAmmoCounter() {
        var weaponSlot = EquipmentScript.GetPlayerEqSlotByEqtype(EquipmentType.PrimaryWeapon);
        if(weaponSlot.ITEM.Count > 0)
        {
            var weapon = weaponSlot.ITEM.item as EquipmentItem;
            if(weapon.EquipmentSpecificType == EquipmentSpecifiedType.Bow)
            {
                var ammoinBackPack = PlayerManager.instance._mainBackpack.GetSumListAvailableAmmunition();

                var ammoForBows = ammoinBackPack.Where(a=>(a.item as AmmunitionItem)._Type == AmmunitionItem.AmmunitionType.StandardArrow).ToList();

                if(ammoinBackPack.Count > 0)
                {
                    EnableAmmoCounter(true);
                    // AmmoSlot.AddNewItemToSlot(ammoinBackPack.First());
                    AmmoSlot.ITEM = ammoinBackPack.First(a=>(a.item as AmmunitionItem ) == GetCurrentSelectedAmmunition(AmmunitionType.StandardArrow) );
                    AmmoSlot.UpdateItemAmount(0);
                // Debug.Log("refresh ammo counter on player profile");
                    return;
                }
                else
                {
                    // 0 szt amunicji pokazac jakies info zamiast pustego pola
                }
            }
        }
        EnableAmmoCounter(false); // nie ma zalozonego łuku nie pokazuj info o amunicji 
    }

    public void EnableAmmoCounter(bool value)
    {
        AmmoSlot.gameObject.SetActive(value);
    }

    internal void OpenAmmoSelectionWindow()
    {
        Debug.Log("włączanie okna wyboru domyslnej amunicji");
    }

    public static Dictionary<AmmunitionItem, int> CurrentAvailableAmmo = new Dictionary<AmmunitionItem, int>();
    public static bool CheckAmmunitionCount(AmmunitionType specificAmmoType = 0, int ammoValue = 0)
    {
        if(ammoValue == 0)
        {
            //Debug.Log("skill nie wymaga uzycia amunicji");
            return true;
        }

        if(specificAmmoType != 0)
        {
            if(CurrentAvailableAmmo.Any(a=>a.Key._Type == specificAmmoType && a.Value >= ammoValue))
            {
                // Debug.Log("W PLECAKU ZNAJDUJE SIE WYBRANA AMUNICJA W MINUMALNEJ POTRZEBNEJ DO UZYCIA ILOSCI");
                return true; // 
            }
           // Debug.Log("brak wymaganej ilości konkretnej amunicji");
            return false;
        }
        else
        {
            if(CurrentAvailableAmmo.Any(a=>a.Value >= ammoValue))
            {
                //Debug.Log("jest na stanie jakakolwiek amunicja o wymaganej iloscic");
                return true; //  
            }
           // Debug.Log("Brak wymaganej ilosci amunicji");
            return false;  
        }
    }
    public static void RefreshAmmoDatafromBackPack()
    {
        //Debug.Log("ammo info refreshed");
        CurrentAvailableAmmo.Clear();

        foreach(var ammopack in PlayerManager.instance._mainBackpack.GetSumListAvailableAmmunition())
        {
            CurrentAvailableAmmo.Add(ammopack.item as AmmunitionItem, ammopack.Count);
        }
        // PlayerManager.instance._EquipedItems.AmmoManager.RefreshAmmoCounter();
        GameManager.instance.attackSelectorPopup.RefreshSkillsRequirmentCheck();
    
    }
    internal static AmmunitionItem GetCurrentSelectedAmmunition(AmmunitionType ammoType)
    {
        RefreshAmmoDatafromBackPack();

        if(CurrentDefault == null)
        {
            // przypisanie startowego defaultoweego wyboru
            CurrentDefault = CurrentAvailableAmmo.First(ammo=>ammo.Key._Type == ammoType).Key;
        }

        // sprawdzenie czy istnieje w plecaku 
        if(CurrentAvailableAmmo.ContainsKey(CurrentDefault))
        {
            return CurrentDefault;
        }
        else
        {
            // przypisanie startowego defaultoweego wyboru gdy ten wybrany przez gracza sie skończy
             CurrentDefault = CurrentAvailableAmmo.First(ammo=>ammo.Key._Type == ammoType).Key;
        }

        return CurrentDefault;
    }
}
