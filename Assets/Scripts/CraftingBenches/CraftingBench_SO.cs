using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New_CraftingBench", menuName = "Item/Crafting Bench")]
public class CraftingBench_SO : SerializedScriptableObject
{
    //public variables
    public List<Recipes_SO> recipes; //input of all the recipes the crafting bench can have. Gets emptied out after building dictionaries
    [SerializeField]public Dictionary<Item, recipeList> dictRecipeList; //master list of recipes for this crafting bench

    //private variables
    private recipeList Groups; //groups recipes based on their first input


    [Button("Build Dictionaries")]
    public void buildDictionaries()
    {
        //fuction to load recipes into a dictionary since I can't see dictionaries in editor. I guaruntee there's a better way to do this but it does work
        Debug.Log("Building dictionary...");
        dictRecipeList = new Dictionary<Item, recipeList>(); //resets our primary dictionary list of all Recipe_SOs

        while (recipes.Count > 0)
        {
            //sets a temp group to hold the recipes during sorting
            Groups = new recipeList();
            Groups.dictRecipeList = new List<Recipes_SO>();
            Groups.dictRecipeList.Add(recipes[0]);

            recipes.RemoveAt(0);

            for (int j = 0; j < recipes.Count; j++)
            {
                if (recipes[j].ItemSOInput[0] == Groups.dictRecipeList[0].ItemSOInput[0])
                {
                    Groups.dictRecipeList.Add(recipes[j]);
                    recipes.RemoveAt(j);
                }
            }

            dictRecipeList.Add(Groups.dictRecipeList[0].ItemSOInput[0], Groups);
        }

    }

    [Button("Test for dictionary")]
    public void testDictionary()
    {
        //tests to make sure dictionary exists
        if (dictRecipeList != null)
        {
            Debug.LogWarning(dictRecipeList.Count);
        }
        else
        {
            Debug.LogError(dictRecipeList);
        }
    }


    public virtual List<Recipes_SO> matcher(List<Item> indexer)
    {
        //function that returns the group of recipes possible for an item given the item index

        List<Recipes_SO> tempMatcher = new List<Recipes_SO>(); // holds the list of recipes to be returned
        recipeList puller = new recipeList(); //pulled recipe from the dictionary

        for (int i = 0; i < indexer.Count; i++)
        {
            if (dictRecipeList.TryGetValue(indexer[i], out puller))
            {
                tempMatcher.AddRange(puller.dictRecipeList); // Add each individual recipe to tempMatcher
            }
        }

        return tempMatcher; // Return the populated list of Recipes_SO
        
    }


    
}

public class recipeList
{
    public List<Recipes_SO> dictRecipeList;
}
