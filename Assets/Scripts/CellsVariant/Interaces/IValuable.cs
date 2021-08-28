using UnityEngine;

internal interface IValuable
{
    int GoldValue { get; set; }
    GameObject Icon_Sprite { get; set; }
    int ID { get; }

    void Pick(out bool result);
}