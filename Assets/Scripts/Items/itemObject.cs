using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class itemObject : MonoBehaviour, iPickUp, iStorable, iRotate
{
    public Item itemStats;
    
    public int Cost; //holds the current cost of the item with all modifiers
    public int MaxStackSize; //maximum items that can stack on this
    public int CurrentStackSize; //current number of items stacked on this

    [SerializeField]
    private GameObject artLocation; //location of where art will be spawned

    public void spawned()
    {
        //function that handles when the item is spawned
        Cost = itemStats.baseCost;
        MaxStackSize = itemStats.StackSize;
        if (MaxStackSize != 0) CurrentStackSize = 1;

        GameObject artInst = Instantiate(itemStats.art, artLocation.transform.position, artLocation.transform.localRotation);
        artInst.transform.parent = artLocation.transform;
    }


    public GameObject pickup(GameObject source)
    {
        /*
        Debug.Log("Transfering " + gameObject.name + " to the hand of " + source.name);
        transform.parent = source.transform;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        //Grabbed.GetComponent<ItemGeneric>().DropListen();
        */
        GetComponent<Collider>().enabled = false;
        return gameObject;
    }

    public bool drop(Vector3 dropLocation, Quaternion dropRotation)
    {
        transform.parent = null;
        transform.position = dropLocation;
        transform.localRotation = dropRotation;
        GetComponent<Collider>().enabled = true;
        return true;
    }

    public bool Storable(GameObject container)
    {
        if (container.GetComponent<iContainer>().container(gameObject))
        {

            return true;
        }
        else
        {
            print("WAAAAAAAAH!");
            return false;
        }
    }

    public bool Rotate(int direction)
    {
        transform.Rotate(0, 90*direction, 0, Space.Self);
        return true;
    }

    public bool multiplace(Vector3 source)
    {
        return false;
    }

    public List<Vector3> GroupCoordinates()
    {
        List<Vector3> temp = new List<Vector3>();
        temp.Add(Vector3.zero);
        return temp;
    }
    public List<Vector3> GroupPerimiterCoordinates()
    {
        //return null;
        List<Vector3> temp = new List<Vector3>();
        temp.Add(Vector3.zero);
        return temp;
    }
}
