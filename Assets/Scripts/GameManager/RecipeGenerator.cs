using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class RecipeGenerator : MonoBehaviour
{
    public ItemDatabase masterList;
    public TextAsset recipeData; //loads a csv for generating recipes
    public string CraftingBenchSaveLocation;//where the bench will be saved
    public string RecipeSaveLocation;

    public string benchName;//name of the crafting bench used when creating the scriptable object

    [Button("Generate Bench")]
    public void createRecipe()
    {
        string[] data = recipeData.text.Split(new char[] { '\n' });
        string[] row;



        print("Creating Crafting Bench Dataset For Bench " + data[0].Split(new char[] { ',' })[1]);
        benchName = data[0].Split(new char[] { ',' })[1] + "_SO";

        //Debug.LogWarning("Creating " + ((data.Length - 1) / 5) + " recipes...");
        for (int q = 0; q < (data.Length - 1) / 5; q++)
        {
            //Debug.LogWarning("Working on data for group " + q);
            //row = data[q].Split(new char[] { ',' });
            //print(data.Length);

            string recipeName = data[q * 5 + 1].Split(new char[] { ',' })[1];
            //Debug.LogError(recipeName + " is the name of this recipe");

            List<Item> ItemSOInput = new List<Item>();
            List<Item> ItemSOOutput = new List<Item>();
            List<int> outputAmount = new List<int>();
            List<Item> ItemAux = new List<Item>();

            for (int i = 2; i < 6; i++) //goes through each section in the csv
            {
                int num = new int();
                List<Item> sortedData = new List<Item>();
                List<int> sortedNums = new List<int>();

                row = data[q * 5 + i].Split(new char[] { ',' });


                for (int j = 1; j < row.Length; j++) //break out the rows/columns
                {
                    if (row[j] != "" && row[j] != " ")
                    {
                        int.TryParse(row[j], out num); //changes string numbers into real numbers

                        if (i - 1 == 3) //tests to see if we're dealing with spawning amount because we just need the int for that
                        {
                            sortedNums.Add(num);
                        }
                        else
                        {
                            sortedData.Add(masterList.itemData[num]); //sorts through master list using the ID of the item and returns the actual item
                        }
                    }
                    else //rest of the cells are empty, at least it assumes that. Could have issues with human input but for now just be careful
                    {
                        j = row.Length;
                    }
                }

                switch (i - 1) //add the data to the appropriate field
                {
                    case 1: ItemSOInput = sortedData; break;
                    case 2: ItemSOOutput = sortedData; break;
                    case 3: outputAmount = sortedNums; break;
                    case 4: ItemAux = sortedData; break;
                }
            }

            Debug.Log("Generating Recipes");
            GenerateRecipeData(ItemSOInput, ItemSOOutput, outputAmount, ItemAux, recipeName);
        }


    }

    void GenerateBenchData()
    {
        CraftingBench_SO bench = ScriptableObject.CreateInstance<CraftingBench_SO>();
    }

    [Button("Generate Recipes")]
    Recipes_SO GenerateRecipeData(List<Item> ItemSOInput, List<Item> ItemSOOutput, List<int> ItemSOOutputAmount, List<Item> ItemSOItemAux, string recipeName)
    {
        Recipes_SO recipe = ScriptableObject.CreateInstance<Recipes_SO>();

        //assign variables to new recipe
        recipe.ItemSOInput = ItemSOInput;
        recipe.ItemSOOutput = ItemSOOutput;
        recipe.outputAmount = ItemSOOutputAmount;
        recipe.ItemAux = ItemSOItemAux;

        string fileName = recipeName;
        fileName = fileName.Substring(0,fileName.Length-1); //fsr when I export the csv it adds a space to the end of the file which gives an error. Very uncool google sheets

        if (RecipeSaveLocation == "") { Debug.LogError("ERROR! NO SAVE FILE LOCATION FOUND!"); return null; }

        UnityEditor.AssetDatabase.CreateAsset(recipe, RecipeSaveLocation + fileName + ".asset"); //saves the file

        return recipe;
    }
}
