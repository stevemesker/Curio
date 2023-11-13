using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class CraftingBenchAssembler : MonoBehaviour
{
    //used for developer only
    //turns an empty game object into a crafting bench because there's a lot of steps and I don't wanna read a readme to remember how to do it...

    [SerializeField, Multiline(lines: 3)] private string devnotes = "Script that turns any empty game object into a crafting bench. When finished, unload all prefabs and remove this script before saving the prefab.";

    [Header("Variables to be filled out")]

    [Tooltip("Crafting Bench Scriptable Object that handles recipes and other data.")]
    public CraftingBench_SO craftingBenchBrain;

    [Min(1), Tooltip("How many primary volumes will be spawned. These are the primary input for crafting.")]
    public int PrimaryVolumeCount = 1; //how many primary volumes will be spawned

    [Tooltip("How many aux volumes will be spawned. These are additional crafting items needed like potion bottles or energy sources.")]
    public int AuxVolumeCount; //how many aux volumes will be spawned

    [Tooltip("How many output volumes will be spawned. These are for when crafting benches have automation and output their contents in a specific direction")]
    public int OutputVolumeCount; //how many output volumes will be spawned

    [Tooltip("How many empty space volumes will be spawned. These are to make sure the plaer isn't placing things that will obscure a necessary part of the crafting bench")]
    public int ESpaceVolumeCount; //how many empty space volumes will be spawned

    [Tooltip("If you want to show a different art when the bench is carried (typically if the bench is larger than a 1x1. If left empty the player will just carry the bench game object like normal")]
    public GameObject CarriedObjectPrefab; //if the crafting bench requires a different object when it is picked up


    [Header("Spawning Objects - must be filled out!")]

    public GameObject PrimaryVolumeObj;
    public GameObject AuxVolumeObj;
    public GameObject OutputVolumeObj;
    public GameObject ESpaceVolumeObj;

    [Header("Private Variables")]
    private CraftingBenchObject benchCode;
    private List<GameObject> primaryVolumesSpawned;
    private List<GameObject> AuxVolumesSpawned;
    private List<GameObject> EmptyVolumesSpawned;
    private List<GameObject> OutVolumesSpawned;

    [Header("Placement Coordinates")]
    public List<Vector3> PrimaryLocatorPoint;
    public List<Vector3> PerimeterLocatorPoint;


    //private GameObject carryArtLocation;

    [Button ("Create Bench Template")]
    public void createBench()
    {
        primaryVolumesSpawned = new List<GameObject>();
        AuxVolumesSpawned = new List<GameObject>();
        EmptyVolumesSpawned = new List<GameObject>();
        OutVolumesSpawned = new List<GameObject>();

        benchCode = gameObject.AddComponent<CraftingBenchObject>();
        benchCode.craftingBrain = craftingBenchBrain;

        int size = 0;
        int totalSize = PrimaryVolumeCount + AuxVolumeCount + ESpaceVolumeCount;
        for (size = 0; size * size < totalSize; size++) { } //this is probably the dumb way to do this but it doesn't need to happen at run time anyway. Size is how many columns/rows will be needed

        primaryVolumesSpawned = createVolume(size, 0, PrimaryVolumeObj, PrimaryVolumeCount);
        if (AuxVolumeCount > 0) AuxVolumesSpawned = createVolume(size, PrimaryVolumeCount, AuxVolumeObj, AuxVolumeCount);
        if (ESpaceVolumeCount > 0) EmptyVolumesSpawned = createVolume(size, PrimaryVolumeCount + AuxVolumeCount, ESpaceVolumeObj, ESpaceVolumeCount);
        if (OutputVolumeCount > 0) OutVolumesSpawned = createOutVolume();

        assignBrain(primaryVolumesSpawned);

        CreateArtParent();
        //assignPrimaryVolumes();
    }

    void assignBrain(List<GameObject> volumes)
    {
        for (int i = 0; i < volumes.Count; i++)
        {
            volumes[i].GetComponent<CraftingVolume>().brain = gameObject.GetComponent<CraftingBenchObject>();
        }
    }

    void CreateArtParent()
    {
        GameObject artObject = new GameObject("Art");
        artObject.transform.parent = gameObject.transform;
        artObject.transform.position = gameObject.transform.position;

        if (CarriedObjectPrefab != null)
        {
            artObject = new GameObject("CarriedArt");
            artObject.transform.parent = gameObject.transform;
            artObject.transform.position = gameObject.transform.position;

            GameObject spawnArt = Instantiate(CarriedObjectPrefab, transform.position, Quaternion.identity);
            spawnArt.transform.parent = artObject.transform;
            spawnArt.SetActive(false);
        }
        //artObject.name = "Art";
    }

    [Button("Assign Volumes")]
    public void assignPrimaryVolumes()
    {
        
        
        for (int i = 0; i < primaryVolumesSpawned.Count; i++)
        {
            benchCode.CraftingVolumes.Add(primaryVolumesSpawned[i]);
        }

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).name == "Art")
            {
                benchCode.primaryArt = gameObject.transform.GetChild(i).gameObject;
            }
            if (gameObject.transform.GetChild(i).name == "CarriedArt")
            {
                benchCode.carryArt = gameObject.transform.GetChild(i).gameObject;
            }
        }

        if (benchCode.CraftingVolumes.Count > 0 && CarriedObjectPrefab == null) Debug.LogError("ERROR! " + gameObject.name + " might be larger than a 1x1 object but does not have a carried form. This may look really bad in game, suggest assigning a carry object");


        //setVolumePlacers();
        CreatePlacementSpace();
        benchCode.PrimaryLocators = PrimaryLocatorPoint;
        benchCode.PerimeterLocators = PerimeterLocatorPoint;
    }

    void CreatePlacementSpace()
    {
        List<GameObject> volumesCombined = new List<GameObject>();
        if (primaryVolumesSpawned.Count != 0) volumesCombined.AddRange(primaryVolumesSpawned);
        if (AuxVolumesSpawned.Count != 0) volumesCombined.AddRange(AuxVolumesSpawned);
        if (EmptyVolumesSpawned.Count != 0) volumesCombined.AddRange(EmptyVolumesSpawned);
        if (volumesCombined.Count == 0) return;
        RaycastHit hit =  new RaycastHit();

        for (int i = 0; i<volumesCombined.Count; i++)
        {
            PrimaryLocatorPoint.Add(volumesCombined[i].transform.localPosition);
            for (int j = 0; j<8; j++)
            {
                switch(j)
                {
                    case 0:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(-1f, 0f, -1f));
                        
                        break;
                    case 1:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(-1f, 0f, 0f));
                       
                        break;
                    case 2:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(-1f, 0f, 1f));
                        
                        break;
                    case 3:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(0f, 0f, 1f));
                        
                        break;
                    case 4:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(1f, 0f, 1f));
                        
                        break;
                    case 5:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(1f, 0f, 0f));
                        
                        break;
                    case 6:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(1f, 0f, -1f));
                        
                        break;
                    case 7:
                        PerimeterLocatorPoint.Add(volumesCombined[i].transform.localPosition + new Vector3(0f, 0f, -1f));
                        
                        break;
                }
                
            }
        }

        for (int i = 0; i < PerimeterLocatorPoint.Count; i++)
        {
            Vector3 temp = new Vector3();
            temp = PerimeterLocatorPoint[i];
            PerimeterLocatorPoint.RemoveAt(i);
            if (PerimeterLocatorPoint.Contains(temp))
            {
                //print("Found a duplicate of " + temp + " at location " + i);
                
                for (int j = 0; j < PerimeterLocatorPoint.Count; j++)
                {
                    if (PerimeterLocatorPoint[j] == temp)
                    {
                        //print("Found match for " + temp + " with " + PerimeterLocatorPoint[j] + " at location " + j);
                        PerimeterLocatorPoint.RemoveAt(j);
                        j = 0;
                        i = 0;
                    }
                }
                
            }
            

            PerimeterLocatorPoint.Add(temp);
        }

        for (int i = 0; i < PerimeterLocatorPoint.Count; i++)
        {
            for (int j = 0; j < volumesCombined.Count; j++)
            {
                if (PerimeterLocatorPoint[i] == volumesCombined[j].transform.localPosition)
                {
                    PerimeterLocatorPoint.RemoveAt(i);
                    i--;
                    j--;
                }
            }
        }
    }

    List<GameObject> createVolume(int size, int placement, GameObject prefabVol, int amount)
    {
        //Debug.Log("creating aux volumes starting at position " + (placement + 1));
        List<GameObject> tempHolder = new List<GameObject>();
        int counter = 0; //counts to make sure I'm spawning the correct number of stuff

        int placementI = placement % size;

        //Debug.Log("should be placing at column " + (placement / size) + " and row " + (placement - ((placement / size) * size)));

        for (int i = placement / size; i < size; i++)
        {
            for (int j = placement - ((placement / size) * size); j < size; j++)
            {

                tempHolder.Add(PrefabUtility.InstantiatePrefab(prefabVol, transform) as GameObject);
                tempHolder[counter].transform.localPosition = new Vector3(i, 0f, j);
                counter += 1;
                if (counter == amount)
                {

                    return tempHolder;
                }
            }
        }
        return tempHolder;
    }

    List<GameObject> createOutVolume()
    {
        //function that spawns out volumes and parents them to primary volumes

        if (OutputVolumeCount > PrimaryVolumeCount)
        {
            Debug.LogWarning("Warning! There are more output volumes than there are primary volumes. Lowering output volumes amount.");
            OutputVolumeCount = PrimaryVolumeCount;
        }

        List<GameObject> tempHolder = new List<GameObject>();

        for (int i = 0; i < OutputVolumeCount; i++)
        {
            tempHolder.Add(PrefabUtility.InstantiatePrefab(OutputVolumeObj, transform) as GameObject);
            tempHolder[i].transform.parent = primaryVolumesSpawned[i].transform;
            tempHolder[i].transform.position = primaryVolumesSpawned[i].transform.position + primaryVolumesSpawned[i].transform.forward;
            //tempHolder[i].transform.localPosition = new Vector3(i, 0f, j);
        }
        return tempHolder;
        //return null;
    }

    [Button("Reset Bench")]
    public void ResetBench ()
    {
        for (int i = 0; i< gameObject.transform.childCount+1; i++)
        {
            //print("boop");
            DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
            i = 0;
        }

        DestroyImmediate(gameObject.GetComponent<CraftingBenchObject>());

        PrimaryLocatorPoint = new List<Vector3>();
        PerimeterLocatorPoint = new List<Vector3>();
    }

    /*
    [Button("Test")]
    public void setVolumePlacers()
    {
        for (int i = 0; i < PrimaryVolumeCount; i++)
        {
            print(gameObject.GetComponent<CraftingBenchObject>().CraftingVolumes[i].name);
            print(gameObject.GetComponent<CraftingBenchObject>().CraftingVolumes[i].transform.localPosition.x);
            print(gameObject.GetComponent<CraftingBenchObject>().CraftingVolumes[i].transform.localPosition.z);
        }
        
    }
    */
}
