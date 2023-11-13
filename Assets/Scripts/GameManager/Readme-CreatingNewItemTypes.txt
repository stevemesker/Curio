Hey future me:

so I mighta made the item types stupid... soz
so right now if you want to make a new item type on the spreadsheet you have to do the following -

-add the type to the Item scriptable object class

-go into the item generator script and add the new type to the for loops in the method public void GenerateItems()

-add it also to the for loop on the gamemanager script under method 
public GameObject spawnItemID(int ID)