using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager gm;
    public ItemDatabase masterItemList;
    public GameObject itemTemplate; //base game object all items use
    public GameObject uiTarget; //prefab of the ui target
    

    private List<GameObject> prefabList; //a bunch of possible prefabs to be spawned based on the type of item being requested of the game manager

    private void Awake()
    {
        if (gm != null)
        {
            if (gm != this)
            {
                Destroy(this);
            }
        }
        else gm = this;

    }

    [Button("sspawnItem")]
    public GameObject spawnObjectID(int ID)
    {
        //returns an appropriate item prefab with the proper scriptable objects attached

        GameObject newItem = Instantiate(itemTemplate);

        if (ID <= masterItemList.itemData.Count)
        {
            Item spawnedItem = masterItemList.itemData[ID];
            print("Now spawning item " + spawnedItem.name);
            newItem.GetComponent<itemObject>().itemStats = spawnedItem;
            newItem.name = spawnedItem.name;
            newItem.GetComponent<itemObject>().spawned();
            return newItem;
        }
        else Debug.LogError("ERROR! Requesting for item object ID that is not part of master item list. How did you even do that? Returning Null...");
        return null;
    }

    public Item getItemID(int ID)
    {
        //returns just an item scriptable object given the ID from the master database
        if (ID <= masterItemList.itemData.Count)
        {
            return masterItemList.itemData[ID];
        }

        return null;
    }

    public GameObject SpawnTargeter()
    {
        GameObject temp = Instantiate(uiTarget);
        return temp;
    }
}
