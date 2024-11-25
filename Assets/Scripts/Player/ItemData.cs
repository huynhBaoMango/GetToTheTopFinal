using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    public Sprite itemIcon;
    public string itemName;
    public float itemPrice;
    public float healthBoost; // Th�m thu?c t�nh n�y
}