using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class ItemGenerator : MonoBehaviour
{
    public string itemLocation;
    public string itemName;

    public string itemSaveLocation;
    public ItemDatabase masterList;

    public List<ItemHolder> IHGenerated = new List<ItemHolder>();
    List<ItemHolder> ItemsHeld = new List<ItemHolder>();


    void sortData ()
    {
        ItemsHeld = new List<ItemHolder>();

        //function reads csv file and sorts it into proper variables

        TextAsset itemData = Resources.Load<TextAsset>("Curio_Master_Item_Sheet_Master_List");

        string[] data = itemData.text.Split(new char[] { '\n' });
        string[] row;

        for (int i = 1; i < data.Length; i++)
        {
            Debug.Log("creating Item scriptable object " + i);
            row = data[i].Split(new char[] { ',' });
            ItemHolder IH = new ItemHolder();
            
            int.TryParse(row[0], out IH.ID);
            IH.ItemName = row[1];
            int.TryParse(row[2], out IH.ItemCost);
            IH.ItemType = row[3];

            if (row[4] == "1") row[4] = "0"; //stops unnessecary stacking functionality
            int.TryParse(row[4], out IH.ItemStackSize);

            IH.itemArt = findArt(row[5]);
            IH.ItemNotes = row[6];

            ItemsHeld.Add(IH);
        }
    }

    [Button ("Generate Item Scriptable Objects")]
    public void GenerateItems()
    {
        masterList.itemData.Clear();
        if (itemSaveLocation == "")
        {
            Debug.LogError("WARNING: No save file location provided!");
            itemSaveLocation = "Assets/Scripts/GameManager/ZZItem Generator/EmergencyDump";
        }

        //function that generates final item SOs using ItemsHeld variable
        Debug.Log("Creating Scriptable Objects...");

        //sort text file
        sortData();

        for (int i = 0; i < ItemsHeld.Count; i++)
        {
            //loops to make all the items
            generateItem(i);
            
        }
    }

    void generateItem(int id)
    {
        //generates item scriptable object
        
        Item newScriptableObject = ScriptableObject.CreateInstance<Item>();
        newScriptableObject.id = ItemsHeld[id].ID;
        newScriptableObject.name = ItemsHeld[id].ItemName;
        newScriptableObject.baseCost = ItemsHeld[id].ItemCost;
        newScriptableObject.Notes = ItemsHeld[id].ItemNotes;
        newScriptableObject.art = ItemsHeld[id].itemArt;
        newScriptableObject.StackSize = ItemsHeld[id].ItemStackSize;

        newScriptableObject.type = (Item.itemType)System.Enum.Parse(typeof(Item.itemType), ItemsHeld[id].ItemType);
        
        UnityEditor.AssetDatabase.CreateAsset(newScriptableObject, itemSaveLocation + "/item" + newScriptableObject.id + "_" + newScriptableObject.name + ".asset");
        masterList.itemData.Add(newScriptableObject);
    }

    

    [Button ("test finder")]
    public GameObject findArt(string name)
    {
        //Finds the art given the file name in the spreadsheet. 
        //Note file name cannot be the last thing in the csv line because unity has troubles parsing that fsr
        GameObject art = (GameObject)AssetDatabase.LoadAssetAtPath(name, typeof(GameObject));
        return art;
    }
}
public class ItemHolder
{
    public int ID;
    public string ItemName;
    public int ItemCost;
    public string ItemType;
    public int ItemStackSize;
    public string ItemNotes;
    public GameObject itemArt;
}
