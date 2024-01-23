using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Item", menuName = "Item/Recipe")]
public class Recipes_SO : ScriptableObject
{
    public List<Item> ItemSOInput;
    public List<Item> ItemSOOutput;
    public List<int> outputAmount;
    public List<Item> ItemAux;
}
