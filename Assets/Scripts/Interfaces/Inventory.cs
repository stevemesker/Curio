using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IInventory
{
    //deals with adding an input item "Source" to whatever the inventory management is for a game object
    bool AddInventory(GameObject source);

    bool RemoveInventory(GameObject source);
}