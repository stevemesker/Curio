using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Player", menuName = "New Characters/Player")]

public class SO_Player : Pawn
{
    /*
    [Tooltip("Amount of money the NPC is carrying (or in the case of a guard how much they charge the player for being naughty.")]
    public int Money;

    [Tooltip("Art Prefab used for the character")]
    public GameObject Art;

    [Tooltip("How fast the unit is")]
    public float movementSpeed;

    //add list of status effects here

    [Tooltip("Defines what type of character this is")]
    public enum characterType { player, helper, shopper, vendor, guard, misc };

    //add animation assets here
    */

    [Tooltip("All of the Store managers that belong to shops the player owns")]
    public List<GameObject> StoresOwned;

    [Tooltip("The default camera that will be spawned")]
    public GameObject CameraAttachment;


    //
    [System.NonSerialized]
    public UnityEvent<int> ev_MoneyChange;

    private void OnEnable()
    {
        if (ev_MoneyChange == null)
        ev_MoneyChange = new UnityEvent<int>();
    }

    public void ChangeMoney(int amount)
    {
        Money -= amount;
        ev_MoneyChange.Invoke(Money);
    }

    
}
