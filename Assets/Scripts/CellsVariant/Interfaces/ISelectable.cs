using UnityEngine;

public interface ISelectable
{
    GameObject Border {get;set;}

    bool IsHighlighted {get;set;}

    void RemoveBorder();
    // void ShowOnNotificationIfInRange();


}