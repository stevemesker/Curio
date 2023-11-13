using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New_Item", menuName = "Item/Item")]
public class Item : ScriptableObject
{
    [Tooltip("Default name of the item, can be modified by the player later")]
    public new string name; //name of the item

    [Tooltip("Default item type, used for declaring the prefab that will hold this data when spawned")]
    public enum itemType { Item, MagicItem};
    public itemType type;

    [Tooltip("Art asset instanced when item is spawned with the item")]
    public GameObject art; //reference to the art

    [Min(0), Tooltip("Unique object ID for spawning and parcing recipes")]
    public int id; //item's ID for spawning and crafting

    [Min(0), Tooltip("Starting cost for all items. Modified by other external things")]
    public int baseCost; //the base cost for the item without modifiers

    [Min(0), Tooltip("Max number of this item that can be stacked into a pile")]
    public int StackSize;

    [Header("Designer Only (Not shown to players)")]

    [Multiline(lines:5)]
    public string Notes;

    private void OnEnable()
    {
        //if (OnItemSpawned == null) OnItemSpawned = new Action<int>();
    }

    //this is temporary
    public Action<int> OnItemSpawned;
    public void SpawnItem(int ID)
    {
        Debug.Log("Hey boss we gots ourselves a spawning situation");
        OnItemSpawned?.Invoke(ID);
    }


}
