using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Shop : MonoBehaviour
{
    //Script that handles shop functionality
    [HorizontalGroup("Split")]
    [Min(4)] public int shopWidth = 12;
    [HorizontalGroup("Split")]
    [Min(4)] public int shopHeight = 12;

    //public List<GameObject> shopInventory;

    [Button("Initialize")]

    public void init()
    {
        //shopInventory = new GameObject[shopWidth, shopHeight];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<iPawn>() != null)
        {
            other.transform.parent = gameObject.transform;
            other.GetComponent<iPawn>().EnterShop(this);
            Debug.Log(other.transform.name + " has entered the shop...");
            return;
        }

        if (other.GetComponent<iPickUp>() != null)
        {
            Debug.Log("item or crafting bench or wall has been added");
            return;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<iPawn>() != null)
        {
            other.transform.parent = null;
            other.GetComponent<iPawn>().EnterShop(null);
            Debug.LogWarning(other.transform.name + " has left the shop...");
            return;
        }
    }

    [Button("Set Shop Size")]
    public void ShopSetSize(int width, int height)
    {
        shopWidth = width;
        shopHeight = height;

        BoxCollider volume = gameObject.GetComponent<BoxCollider>();

        volume.size = new Vector3(shopWidth, 5f, shopHeight);
        volume.center = new Vector3(shopWidth / 2, 5f/2 - .25f, shopHeight / 2);

    }

    public bool addItem(GameObject item, Vector3 location, Quaternion rotation)
    {
        print("poo poo pee pee");
        item.transform.parent = transform;
        item.transform.localPosition = location;
        item.transform.rotation = rotation;
        return false;
    }

    /*
    [Button("Place Object")]
    public bool PlaceObject(GameObject source, int column, int row)
    {
        if (shopInventory[column, row] == null)
        {
            source.transform.parent = gameObject.transform;
            source.transform.localPosition = new Vector3(column, transform.position.y, row);
            shopInventory[column, row] = source;
            return true;
        }
        else Debug.LogWarning("Could not add " + source.name + " because shop floor is occupied at location: [" + column + "," + row + "]: " + shopInventory[column, row].name);
        return false;
    }
    */
}
