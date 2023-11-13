using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, iContainer, iPawn
{
    public static Player player;
    private PlayerMovement pController;


    [SerializeField] public SO_Player playerSO;

    /////////////////////////////////////////////////
    [Header("Pickup Objects")]
    /////////////////////////////////////////////////
    //--

    
    public List<GameObject> HeldObject; 

    //--

    /////////////////////////////////////////////////
    [Header("Attachments")]
    /////////////////////////////////////////////////
    //--

    public GameObject camTarget; //so the camera knows what to look at (my eyes are up here)
    public List<GameObject> HandTarget; //all the different hands
    [SerializeField] public Shop shop; //the current shop the player is in

    //--

    private void Awake()
    {
        if (player == null)
        {
            player = this;
        }
        else
        {
            Destroy(this);
        }

        //setting the camera if there isn't one set up yet
        pController = GetComponent<PlayerMovement>();

        if (pController.pCamera == null)
        {
            if (playerSO.CameraAttachment)
            pController.pCamera = Instantiate(playerSO.CameraAttachment);
        }
    }

    public bool DecreaseMoney(int amount)
    {
        if (playerSO.Money >= amount)
        {
            playerSO.ChangeMoney(Mathf.Abs(amount));
            return true;
        }
        else
        {
            return false;
        }

    }

    public bool IncreaseMoney(int amount)
    {
        if (playerSO != null)
        {
            playerSO.ChangeMoney(Mathf.Abs(amount) * -1);
            return true;
        }
        else
        {
            return false;
        }

    }

    public GameObject FindFreeHand()
    {
        if (HeldObject.Count < HandTarget.Count)
        {
            //Debug.Log("empty hand detected at hand positiom " + (HeldObject.Count) + " named :" + HandTarget[HeldObject.Count].name);
            return (HandTarget[HeldObject.Count]);
        }

        return null;
    }

    public bool AddInventory(GameObject source)
    {
        HeldObject.Add(source);
        source.transform.parent = HandTarget[HeldObject.Count - 1].transform;
        source.transform.localPosition = Vector3.zero;
        source.transform.localRotation = Quaternion.identity;
        //source.GetComponent<Collider>().enabled = false;
        return true;
    }

    public bool RemoveInventory(GameObject source)
    {
        HeldObject.Remove(source);
        return true;
    }
    public void PickUpObject(GameObject Grabbed)
    {
        GameObject freeHand = FindFreeHand();
        if (freeHand == null) return;
        //print("Found a free hand!");
        AddInventory(Grabbed);

        /*
        if (Grabbed.GetComponent<iPickUp>().pickup(FindFreeHand()) != null)
        {
            print("Found a free hand!");
            AddInventory(Grabbed);
        }
        */
    }

    public bool container(GameObject item)
    {
        return false;
    }

    public void resort ()
    {
        
        //resorts held items
    }

    public void Pawn(GameObject source)
    {
        
        return; //will probably put handling adding the player to shops here...
    }

    public bool EnterShop(Shop source)
    {
        shop = source;
        
        
        if (shop == null)
        {
            //gameObject.GetComponent<PlayerPlacer>().enabled = false;
            if (pController.targeter != null) { pController.targeter.GetComponent<TargetObject>().EraseTarget() ; Destroy(pController.targeter); }
            return false;
        }

        if (GameManager.gm) pController.targeter = GameManager.gm.SpawnTargeter();
        //gameObject.GetComponent<PlayerPlacer>().enabled = true;
        return true;
        
    }

    public Vector3 FindSnapPoint()
    {
        //used for finding player's primary snapped placement position
        Vector3 snap = transform.localPosition + (transform.forward / 2) + transform.forward;
        snap.x = Mathf.Round(snap.x);
        snap.y = Mathf.Round(snap.y);
        snap.z = Mathf.Round(snap.z);
        return snap;
    }
}