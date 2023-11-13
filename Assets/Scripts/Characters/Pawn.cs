using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pawn : ScriptableObject
{
    //Class that holds data that all NPCs have

    [Tooltip("Amount of money the NPC is carrying (or in the case of a guard how much they charge the player for being naughty.")]
    public int Money;

    [Tooltip("How fast the unit is")]
    public float movementSpeed;

    [Tooltip("Defines what class of character this is")]
    public enum characterClassType {player, wood, stone, metal, gold, diamond, other };

    [Tooltip("Defines what job this character has")]
    public enum characterJobType {player, vendor, guard, farmer, worker, artisan, warrior, mage, politician, townsfolk}

    //add animation assets here
}
