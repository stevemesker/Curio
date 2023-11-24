using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallInteriorObject : MonoBehaviour, iPickUp, iWall
{
    public GameObject carryArt;
    public GameObject primaryArt;

    public GameObject pickup(GameObject source)
    {
        Debug.Log("Transfering " + gameObject.name + " to the hand of " + source.name);

        ////////future stuff////////
        //Make sure there's nothing attached to the wall
        //
        /////end future stuff//////

        GameObject temp = Instantiate(carryArt, transform.position, Quaternion.identity);
        temp.GetComponent<WallCarryObject>().wallArt = gameObject.GetComponent<WallObject>().defaultArt;

        //update all neighbors as primary
        gameObject.GetComponent<WallObject>().RemoveWall();

        Destroy(gameObject);
        
        return temp.GetComponent<iPickUp>().pickup(source);
    }


    public bool drop(Vector3 dropLocation, Quaternion dropRotation)
    {
        throw new System.NotImplementedException();
    }

    public List<Vector3> GroupCoordinates()
    {
        return null;
    }

    public List<Vector3> GroupPerimiterCoordinates()
    {
        return null;
    }

    public bool multiplace(Vector3 source)
    {
        return false;
    }

    public GameObject FindWall(GameObject source)
    {
        return gameObject;
    }

    public void renderChange(string renderLayer)
    {
        throw new System.NotImplementedException();
    }
}
