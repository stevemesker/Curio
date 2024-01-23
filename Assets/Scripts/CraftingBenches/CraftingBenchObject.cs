using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

[SelectionBase]
public class CraftingBenchObject : MonoBehaviour,iPickUp, iRotate
{
    //notes: 
    //should probably make a private GameObject list for all the input locators so I don't have to keep calling getcomponent whenever I check inventory

    public CraftingBench_SO craftingBrain; //holds data important to this type of crafting bench
    public List<GameObject> CraftingVolumes; //primary list for crafting
    public List<GameObject> AuxVolumes; //primary list for aux inputs
    public bool destroyOnInput; //some items are destroyed 
    public List<Vector3> PrimaryLocators; //used for placing crafting bench in shop
    public List<Vector3> PerimeterLocators; //used for placing crafting bench in shop

    public GameObject carryArt; //object that is displayed when object is being carried by pawn
    public GameObject primaryArt; //points to the art used when object is on the floor in a shop

    private List<Item> heldCraft; //used to remember what was previously crafted to make crafting more efficinet
    private float rotator; //used to hold value of what rotation the crafting bench should be placed with

    //public List<Vector2> volumePlacers; //used to decide if crafting bench can be placed

    #region Crafting
    [Button ("test full crafting")]
    public void Craft()
    {
        //method that attempts to convert held inventory into crafted items

        List <recipeList> tempRecipes = new List <recipeList>();//stores all the recipes found in the dictionary
        List<recipeUnpacked> adjRecipes;//stores the unpacked list of possible recipes from the dictionary
        
        List<Item> inventory = new List<Item>(); //total inventory of held items gained from the crafting volumes
        List<Item> adjInputItem = new List<Item>(); //combines similar items to prevent pulling duplicate lists

        inventory = getInventory(CraftingVolumes);
        if (inventory.Count == 0) return;
        adjInputItem = InputSorter(inventory);

        for (int i = 0; i < adjInputItem.Count; i++)
        {
            recipeList temp = searchRecipe(adjInputItem[i]);
            if (temp != null) tempRecipes.Add(temp); 
        }

        adjRecipes = new List<recipeUnpacked>(recipeUnpack(tempRecipes)); //unpack dictionary recipe lists to lists of recipes

        //take the inventory and see if any of the recipes match it
        recipeUnpacked Spawn = new recipeUnpacked();

        Spawn = SpawningItem(adjRecipes, inventory);
        if (Spawn == null) return;

        consumeRecipe(Spawn.inputRecipe);
        addItem(Spawn.outputRecipe);
    }


    recipeUnpacked SpawningItem(List<recipeUnpacked> inputRecipes, List<Item> inventory)
    {
        //takes the list of items "inventory" and sorts through list of recipes to find the correct output (if any) to spawn
        recipeUnpacked recipeBestFit = new recipeUnpacked(); //used to hold current contender for most probable recipe
        int failcount = 0;
        for (int recipeFocus = 0; recipeFocus < inputRecipes.Count; recipeFocus++) //go through each recipe
        {
            List<Item> InvCopy = new List<Item>(inventory);
            List<Item> rCopy = new List<Item>(inputRecipes[recipeFocus].inputRecipe);
            for (int i = 0; i < InvCopy.Count; i++) //go through each inventory item
            {
                for (int j = 0; j < rCopy.Count; j++) //go through each recipe input item
                {
                    if (InvCopy[i] == rCopy[j])
                    {
                        //Debug.Log("Found a match with " + InvCopy[i].name + " and " + rCopy[j] + "in recipe #" + recipeFocus);
                        InvCopy.RemoveAt(i);
                        rCopy.RemoveAt(j);
                        j = rCopy.Count;
                        i--;
                    }
                }
            }
            if (InvCopy.Count + rCopy.Count == 0)
            {
                Debug.LogWarning("Found a perfect match for recipe #" + recipeFocus);
                return inputRecipes[recipeFocus];
            }
            else if (rCopy.Count == 0)
            {
                recipeBestFit = inputRecipes[recipeFocus];
            }
            else failcount+= 1;
        }
        if (failcount == inputRecipes.Count) return null; //no valid recipes were found
        return recipeBestFit;
    }
    
    void addItem(List<Item> input)
    {
        for (int i = 0; i < input.Count; i++)
        {
            GameObject temp = GameManager.gm.spawnObjectID(input[i].id);
            if (dispenseItem(temp) == false) Destroy(temp);
        }
    }
    
    #endregion

    #region Inventory Management
    List<Item> InputSorter(List<Item> inv)
    {
        //gets a list of unique items in primaryItemContainer
        List<Item> tempItem = new List<Item>();

        for (int i = 0; i < inv.Count; i++)
        {
                if (!tempItem.Contains(inv[i])) 
                {
                    //Debug.Log("Did not find " + inv[i].name);
                    tempItem.Add(inv[i]); 
                }
        }
        //Debug.Log("Found " + tempItem.Count + " unique items");
        return tempItem;
    }

    recipeUnpacked unpackRecipes(Recipes_SO input)
    {
        recipeUnpacked temp = new recipeUnpacked();
        temp.inputRecipe = input.ItemSOInput;
        temp.outputRecipe = input.ItemSOOutput;
        return temp;
    }

    List<Item> getInventory(List<GameObject> inv)
    {
        List<Item> temp = new List<Item>();
        for (int i = 0; i < inv.Count; i++)
        {
            if (CraftingVolumes[i].GetComponent<CraftingVolume>().inputLocator.transform.childCount > 0)
            {
                temp.Add(CraftingVolumes[i].GetComponent<CraftingVolume>().getItemInv());
            }
        }
        return temp;
    }

    List<recipeUnpacked> recipeUnpack(List<recipeList> input)
    {
        //method that takes imported dictionary recipe lists and unpacks them into individual recipes
        if (input.Count == 0) return null;

        List<recipeUnpacked> tempList = new List<recipeUnpacked>();

        for (int i = 0; i < input.Count; i++)
        {
            for (int j = 0; j < input[i].dictRecipeList.Count; j++)
            {
                tempList.Add(unpackRecipes(input[i].dictRecipeList[j]));
            }
        }
        //Debug.Log("Unpacked " + tempList.Count + " recipes...");

        //return null;
        return tempList;
    }

    void consumeRecipe(List<Item> input)
    {
        //finds items within crafting volumes and removes their contents based on the recipe input
        for (int i = 0; i < input.Count; i++)
        {
            for (int j = 0; j < CraftingVolumes.Count; j++)
            {
                if (CraftingVolumes[j].GetComponent<CraftingVolume>().getItemInv() == input[i])
                {
                    Debug.LogWarning("Found match with item " + input[i].name + " at crafting volumes position " + j);
                    CraftingVolumes[j].GetComponent<CraftingVolume>().clearInv();
                    j = CraftingVolumes.Count;
                }
            }
        }
    }

    public bool dispenseItem (GameObject input)
    {
        for (int i = 0; i < CraftingVolumes.Count; i++)
        {
            if (CraftingVolumes[i].GetComponent<CraftingVolume>().getItemInv() == null)
            {
                
                CraftingVolumes[i].GetComponent<iContainer>().container(input);
                
                Debug.LogWarning("Success! Crafting Volume #" + i + " has a free space!");
                return true;
            }

            Debug.LogError("Crafting volume #" + i + " was not free. It contains " + CraftingVolumes[i].GetComponent<CraftingVolume>().getItemInv().name);
        }
        Debug.LogError("No crafting volumes were free...");
        return false;
    }

    #endregion

    #region Spawning Items

    void spawnCraftedItem(List<Item> sItem)
    {
        Debug.Log("crafting and dispensing...");
        for (int i = 0; i < sItem.Count; i++)
        {
            GameObject spanwedItem = GameManager.gm.spawnObjectID(sItem[i].id);
            if (dispenseItem(spanwedItem) == false) Destroy(spanwedItem);
        }
    }

    public recipeList searchRecipe (Item input)
    {
        return null;
        //recipeList list = craftingBrain.matcher(input);
        //return list;
    }
    #endregion //Spawning Items

    #region Interaction
    public GameObject pickup(GameObject source)
    {
        Debug.Log("Transfering " + gameObject.name + " to the hand of " + source.name);
        transform.parent = source.transform;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        toggleColliders();

        if (carryArt != null)
        {
            carryArt.SetActive(true);
            primaryArt.SetActive(false);
        }
        //GetComponent<Collider>().enabled = false; we'll need to disable collider for children later
        /*Grabbed.GetComponent<ItemGeneric>().DropListen();*/

        return gameObject;
    }

    public bool drop(Vector3 dropLocation, Quaternion dropRotation)
    {
        //print(dropRotation);
        transform.parent = null;
        transform.position = dropLocation;

        //if (Player.player.shop != null) transform.Rotate(0,rotator, 0, Space.Self);
        if (Player.player.shop != null) transform.rotation = Quaternion.Euler(0, rotator, 0);
        else transform.localRotation = dropRotation;
        //transform.rotation = Quaternion.Euler(0, rotator, 0);
        toggleColliders();

        if (carryArt != null)
        {
            carryArt.SetActive(false);
            primaryArt.SetActive(true);
        }
        //GetComponent<Collider>().enabled = true; we'll need to disable collider for children later
        return true;
    }

    void toggleColliders()
    {
        for (int i = 0; i < CraftingVolumes.Count; i++)
        {
            CraftingVolumes[i].GetComponent<Collider>().enabled = !CraftingVolumes[i].GetComponent<Collider>().enabled;
        }
    }

    public bool multiplace(Vector3 source)
    {
        return false;
    }

    public List<Vector3> GroupCoordinates()
    {
        List<Vector3> temp = new List<Vector3>(PrimaryLocators);

        for (int i = 0; i < PrimaryLocators.Count; i++)
        {
            temp[i] = Quaternion.Euler(0, rotator, 0) * PrimaryLocators[i];
            temp[i] = new Vector3(Mathf.Round(temp[i].x), Mathf.Round(temp[i].y), Mathf.Round(temp[i].z));
            print(temp[i]);
        }
        
        return temp;

        //return PrimaryLocators;
    }

    public List<Vector3> GroupPerimiterCoordinates()
    {
        List<Vector3> temp = new List<Vector3>(PerimeterLocators);

        for (int i = 0; i < PerimeterLocators.Count; i++)
        {
            temp[i] = Quaternion.Euler(0, rotator, 0) * PerimeterLocators[i];
            temp[i] = new Vector3(Mathf.Round(temp[i].x), Mathf.Round(temp[i].y), Mathf.Round(temp[i].z));
        }

        return temp;
    }

    public bool Rotate(int direction)
    {
        
        rotator += 90 * direction;
        if (rotator >= 360) rotator = 0;
        if (rotator < 0) rotator += 360;

        if (Player.player.shop == null) transform.Rotate(0, 90 * direction, 0, Space.Self);
        //print(rotator);
        return true;
    }

    public void renderChange(string renderLayer)
    {
        throw new System.NotImplementedException();
    }

    #endregion


}

public class recipeUnpacked
{
    public List<Item> inputRecipe;
    public List<Item> outputRecipe;
}
