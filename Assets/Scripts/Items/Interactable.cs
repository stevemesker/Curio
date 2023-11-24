using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    

}

public interface iRotate
{
    //rotates an object a specified direction
    bool Rotate(int direction);
}

public interface iPickUp
{
    //Picks up the item. Item is always stored in the hand "source"
    GameObject pickup(GameObject source);

    //Drops the held item at location "dropLocation" with a local rotation of "dropRotation"
    bool drop(Vector3 dropLocation, Quaternion dropRotation);

    //handles if the item needs multi placement, typically used for walls
    bool multiplace(Vector3 source);

    //handles placing things on the grid by telling the targeter what spaces it will take up
    List<Vector3> GroupCoordinates();

    //handles placing things on the grid by telling the targeter what spaces it will take up
    List<Vector3> GroupPerimiterCoordinates();

    
}

public interface iHologram
{
    //handles changing the object into a hologram
    void renderChange(string renderLayer);
}

public interface iGather
{
    //gatherable stuff uses this
    bool gather();
}

public interface iContainer
{
    //finds out if the container can fit the item "item"
    //this can be a no if there is no room or if item doesn't match otehr qualifications
    bool container(GameObject item);

    bool AddInventory(GameObject source);

    bool RemoveInventory(GameObject source); //source lets the function know who to handoff the item to
}

public interface iStorable
{
    //finds out if the item can be stored within a container "container"
    //this can be a no if it isn't a container or if it has no more room in the container
    bool Storable(GameObject container);
}

public interface iInteract
{
    bool Interact(GameObject source);
}

public interface iPawn
{
    void Pawn(GameObject source);
    bool EnterShop(Shop source);
}

public interface iWall
{
    //used for wall pieces to identify eachother
    GameObject FindWall(GameObject source);
}