using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_ItemDatabase", menuName = "New Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> itemData;

}
