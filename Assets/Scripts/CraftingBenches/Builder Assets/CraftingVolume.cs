using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingVolume : MonoBehaviour, iPickUp, iContainer, iInteract, iRotate
{
    [Header("Variables to be filled out")]

    [Tooltip("Place to store the item. Still need for crafting benches that turn input items invisible")]
    public GameObject inputLocator;

    [Tooltip("Primary controller for the volume")]
    public CraftingBenchObject brain;


    #region Crafting
    
    public Item getItemInv ()
    {
        if (inputLocator.transform.childCount != 0)
        {
            return inputLocator.transform.GetChild(0).GetComponent<itemObject>().itemStats;
        }
        return null;
    }


    public void clearInv()
    {
        //clears inventory when called
        Debug.Log("Clearing inventory");
        if (inputLocator.transform.GetChild(0) != null)
        {
            GameObject temp = inputLocator.transform.GetChild(0).gameObject;
            Debug.LogWarning("Clearing child " + inputLocator.transform.GetChild(0).name);
            temp.transform.parent = null;
            Destroy(temp);
            Debug.Log("Input inventory now has " + inputLocator.transform.childCount + " items under it.");
        }
    }
    #endregion

    #region Interactions
    public bool container(GameObject item)
    {
        
        if (inputLocator.transform.childCount == 0)
        {
            Debug.Log("boop");
            //there is a free space to accept an item
            //transfer item to input locator
            item.transform.parent = inputLocator.transform;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.GetComponent<Collider>().enabled = false;
            //tell brain an item has been added
            //brain.PrimaryItemAdded(inputLocator.transform.GetChild(0).GetComponent<itemObject>().itemStats);
            return true;
        }
        else
        {
            Debug.LogWarning("warning! Volume occupied by " + item.name);
            //there is already an item occupying. Check if sibling volumes can 
            if (brain.dispenseItem(item)) return true;
        }
        return false;
    }

    public bool AddInventory(GameObject source)
    {
        throw new System.NotImplementedException();
    }

    public bool RemoveInventory(GameObject source)
    {
        if (inputLocator.transform.GetChild(0) == null) return false;
        if (source.GetComponent<iContainer>().AddInventory(inputLocator.transform.GetChild(0).gameObject))
        {
            return true;
        }
        return false;
        
    }

    
    public bool drop(Vector3 dropLocation, Quaternion dropRotation)
    {
        //send up to crafting brain
        if (brain.drop(dropLocation, dropRotation)) return true;
        return false;
    }

    public GameObject pickup(GameObject source)
    {
        //send up to crafting brain
        if (brain.pickup(source)) return brain.gameObject;
        return null;
    }

    public bool multiplace(Vector3 source)
    {
        return false;
    }

    public bool Interact(GameObject Source)
    {
        brain.Craft(); //probably will move this once we get proper interactions
        //Debug.Log("Now crafting");
        return true;
    }

    public List<Vector3> GroupCoordinates()
    {
        return null;
    }

    public List<Vector3> GroupPerimiterCoordinates()
    {
        return null;
    }
    public bool Rotate(int direction)
    {
        brain.Rotate(direction);
        return true;
    }

    #endregion //Interactions
}
