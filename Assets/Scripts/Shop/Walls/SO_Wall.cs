using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wall", menuName = "Item/Wall")]

public class SO_Wall : ScriptableObject
{
    [Tooltip("Part of the wall that is scaled to fill the volume")]
    public GameObject wallRunnerArt;

    [Tooltip("Art that is duplicated along the wall as it is stretched out between pillars")]
    public List<GameObject> wallDupeArt;

    [Tooltip("How many spaces the dupe is dispersed")]
    public int wallDupeDistance = 1;

    [Tooltip("If checked: will randomly chose between list of dupes instead of in order as they are built in the list")]
    public bool wallDupeRandom;

    [Tooltip("Defines the art assets for the main pillars of the wall. Very important that for pillar pieces they are in the right order: 0) Basic 1) L Intersection 2) T Intersection 3) X Intersections")]
    public List<GameObject> wallPillarArt = new List<GameObject>();

}
