using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WallComponent : MonoBehaviour, iPickUp, iWall
{
    [Tooltip("Game object this object reports to")]
    public GameObject topLevel;
    [Tooltip("Points to the art assets. Very important that for pillar pieces they are in the right order: 0) Basic 1) L Intersection 2) T Intersection 3) X Intersections. Can point to the same object for multiple positions")]
    public List <GameObject> art;

    private int junctionID; //stores the id so we don't instance a junction type if we don't have to

    public void SpawnArt(SO_Wall wallType)
    {
        //function that reads the wall scriptable object and assigns the proper art

    }

    [Button("Test update")]
    public void UpdatePillarType(int id, float rotation)
    {
        //function that swaps out pillar type based on ID input and rotates the pillar based on rotation
        /*
         * ----------
         * ID Key
         * ----------
         * 0: Inline pillar so give it basic pillar
         * 1: L Intersection
         * 2: T Intersection
         * 3: X Intersection
         * 4: whatever current junctionID is
         */
        if (id == junctionID) return; //no point updating if it's the same thing
        if (id == 4) id = junctionID; //used for updating art but not the actual pillar type

        if (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
        Instantiate(art[id], transform.position, transform.rotation, gameObject.transform);
        junctionID = id;
        //transform.GetChild(0)


        transform.localRotation = Quaternion.Euler(0, rotation, 0);
    }
    public GameObject pickup(GameObject source)
    {
        GameObject temp = topLevel.GetComponent<iPickUp>().pickup(source);
        return temp;
    }

    public bool drop(Vector3 dropLocation, Quaternion dropRotation)
    {
        topLevel.GetComponent<iPickUp>().drop(dropLocation, dropRotation);
        return topLevel;
    }

    public List<Vector3> GroupCoordinates()
    {
        throw new System.NotImplementedException();
    }

    public List<Vector3> GroupPerimiterCoordinates()
    {
        throw new System.NotImplementedException();
    }

    public bool multiplace(Vector3 source)
    {
        throw new System.NotImplementedException();
    }

    public GameObject FindWall(GameObject source)
    {
        return topLevel;
    }
}
