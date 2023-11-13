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
        transform.parent = source.transform;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        //toggleColliders();

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
}
