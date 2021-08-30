using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public Image ItemIMG;
    public int ItemID { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public string Description { get; set; }
    public string Rarity { get; set; }
    public string Type { get; set; }

}
