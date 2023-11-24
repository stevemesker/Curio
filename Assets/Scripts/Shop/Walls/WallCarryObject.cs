using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCarryObject : MonoBehaviour, iPickUp
{
    public SO_Wall wallArt; //used to remember what type of wall the carried wall art is
    public GameObject wall; //wall being instantiated when placing

    public List<Vector3> primaryCoordinates;
    public List<Vector3> secondaryCoordinates;

    public bool placing; //used for toggling on the inbetween step of placing the wall

    //private variables
    private int wallLength; //tells the wall how long it's going to be when placed and while updating the hologram
    public GameObject currentWall; //wall being instantiated

    public void Awake()
    {
        if (wallArt == null) Debug.LogError("ERROR! Wall carry object " + gameObject.name + " has no assigned wall art type!");
    }

    public void UpdateWallLength(Vector3 originLocation, Vector3 dropLocation)
    {
        //used to figure out the wall length giving the starting point of originLocation and ending at dropLocation
        int temp = 1; //used for reversing the selection

        primaryCoordinates = new List<Vector3>();
        secondaryCoordinates = new List<Vector3>();
        if (wallLength >= 2 && currentWall == null) { currentWall = Instantiate(wall, Player.player.GetComponent<PlayerMovement>().targeter.transform.position, Quaternion.identity); currentWall.GetComponent<WallObject>().renderChange("Hologram"); }
        


        if (Mathf.Abs(dropLocation.x - originLocation.x) > Mathf.Abs(dropLocation.z - originLocation.z))
        {
            print("Following the X");
            wallLength = (int) Mathf.Abs(dropLocation.x - originLocation.x);
            if (dropLocation.x < originLocation.x)temp = -1;
            for (int i = 0; i < wallLength+1; i++)
            {
                primaryCoordinates.Add(new Vector3(i * temp, 0, 0));
                secondaryCoordinates.Add(new Vector3(i * temp, 0, 1));
                secondaryCoordinates.Add(new Vector3(i * temp, 0, -1));
            }
            if (currentWall != null) currentWall.transform.localRotation = Quaternion.Euler(0, 90*temp, 0);
        }
        else
        {
            print("Following the Z");
            wallLength = (int)Mathf.Abs(dropLocation.z - originLocation.z);
            if (dropLocation.z < originLocation.z) temp = -1;
            for (int i = 0; i < wallLength+1; i++)
            {
                primaryCoordinates.Add(new Vector3(0, 0, i * temp));
                secondaryCoordinates.Add(new Vector3(1, 0, i * temp));
                secondaryCoordinates.Add(new Vector3(-1, 0, i * temp));
            }
            if (currentWall != null && dropLocation.z < originLocation.z) currentWall.transform.localRotation = Quaternion.Euler(0, 180, 0);
            else if (currentWall != null && dropLocation.z >= originLocation.z) currentWall.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (currentWall != null)
        {
            //currentWall.GetComponent<WallObject>().defaultArt = wallArt;
            currentWall.GetComponent<WallObject>().UpdateArt(wallArt);
            currentWall.GetComponent<WallObject>().wallSize = wallLength;
            currentWall.GetComponent<WallObject>().UpdateWall(false);
        }

    }

    public bool drop(Vector3 dropLocation, Quaternion dropRotation)
    {
        //dropping the wall outside shops
        if (Player.player.shop == null)
        {
            transform.parent = null;
            transform.position = dropLocation;
            transform.localRotation = dropRotation;
            GetComponent<Collider>().enabled = true;
            return true;
        }

        //dropping the wall in the shop
        if (placing == false)
        {
            //startpos = dropLocation;
            placing = true;
            Debug.Log("initializing placement");
            

            return false;
        }
        else
        {
            Debug.Log("Placing wall...");
            currentWall.GetComponent<WallObject>().UpdateWall(true);
            if (GameManager.gm != null) GameManager.gm.HoloEnd(currentWall);
            //currentWall.GetComponent<WallObject>().renderChange("Default");
        }

        Destroy(gameObject);
        return true;
    }

    public List<Vector3> GroupCoordinates()
    {
        return primaryCoordinates;
    }

    public List<Vector3> GroupPerimiterCoordinates()
    {
        return secondaryCoordinates;
    }

    public bool multiplace(Vector3 source)
    {

        return true;
    }

    public GameObject pickup(GameObject source)
    {
        primaryCoordinates = new List<Vector3>();
        primaryCoordinates.Add(Vector3.zero);

        secondaryCoordinates = new List<Vector3>();
        secondaryCoordinates.Add(Vector3.forward);
        secondaryCoordinates.Add(Vector3.forward * -1);
        secondaryCoordinates.Add(Vector3.right);
        secondaryCoordinates.Add(Vector3.left);
        secondaryCoordinates.Add(Vector3.forward + Vector3.right);
        secondaryCoordinates.Add(Vector3.forward + Vector3.left);
        secondaryCoordinates.Add(Vector3.forward * -1 + Vector3.right);
        secondaryCoordinates.Add(Vector3.forward * -1 + Vector3.left);

        GetComponent<Collider>().enabled = false;
        return gameObject;
    }

    public void renderChange(string renderLayer)
    {
        throw new System.NotImplementedException();
    }
}
