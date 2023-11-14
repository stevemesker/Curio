using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TargetObject : MonoBehaviour
{
    //script that handles organizing space selections in the shop

    [Header("Attachments")]

    public GameObject selectors; //selector prefab for instancing
    


    [Header("Container")]

    [Tooltip("Contains instanced prefabs of where the carried object is going to take up in the shop space")]
    public List<GameObject> primarySelectors;
    [Tooltip("Contains instanced prefabs of selectors that show the perimiter selectors")]
    public List<GameObject> perimiterSelectors;

    [Header("Temp Variables")]
    

    //private variables
    //private List<Vector3> previousList; //saved previous list
    private List<Vector3> finalList; //list of every selector on screen


    [Button("Update Selection Temp")]
    public void TestUpdate()
    {
        
    }

    #region CrafterUpdate
    public bool UpdateSelection (List<Vector3> spaces, List<Vector3> perimiterSpaces)
    {
        //function that handles testing and dispersing selectors
        //returns false if the object being held cannot be placed

        //Debug.Log("Updating Selectors...");
        if (spaces == null) { print("boop"); return itemUpdate();  } //determines if it's an object like an item that requires different icons. NEED TO CHANGE THIS LATER
        
        List<Vector3> Primary = new List<Vector3>(spaces);
        List<Vector3> Perimiter = new List<Vector3>(perimiterSpaces);

        List<Vector3> needToSpawn; //what's left over from analyzing current space selectors to decide what needs to be spawned


        //spawning primary objects
        if (primarySelectors.Count > 0) //check to make sure we already have space selectors
        {
            needToSpawn = SortList(Primary, primarySelectors, perimiterSelectors, true); //find out what is missing from our primary selectors
        }
        else
        {
            Debug.Log("Fresh spawn time");
            needToSpawn = Primary; //spawn em all. Probably needs to change once we get perimiter stuff set up
        }
            //create spaces
        //Debug.Log("Need to spawn " + needToSpawn.Count + " more spaces");
        if (needToSpawn.Count > 0) CreateSelectors(needToSpawn, primarySelectors, true); //need to add to the list for perimiter spaces





        //spawning perimiter objects
        needToSpawn = SortList(Perimiter, perimiterSelectors, null, false); //find out what is missing from our primary selectors
        if (needToSpawn.Count > 0) CreateSelectors(needToSpawn, perimiterSelectors, false); //need to add to the list for perimiter spaces


        //test for errors
        List<Vector3> errors = CheckSpaces(spaces); //this needs to be final list not spaces

        //update all spaces including errored ones
        //UpdateSpaces(errors);

        //check if placement is valid
        if (errors.Count != 0) 
        {
            Debug.LogWarning("Found problems with placing objects in " + errors.Count + " different spaces such as " + errors[0]);
            //return false; 
        }
        
        return true;
    }

    List<Vector3> SortList(List<Vector3> spaces, List<GameObject> bucket, List<GameObject> dump, bool setting)
    {
        //spaces are the list of spaces being observed
        //bucket is where they are coming from
        //dump is where they are going if they don't match spaces
        //setting is what the spaces need to be set to (either primary or perimeter)
        //returns what needs to be spawned still

        List<GameObject> temp = new List<GameObject>();

        for (int i = 0; i < spaces.Count; i++)
        {
            for (int j = 0; j < bucket.Count; j++)
            {
                if (spaces[i] == bucket[j].transform.position - gameObject.transform.position)
                {
                    //Debug.Log("Found a match for " + spaces[i] + " in space " + bucket[j].name + "!");
                    bucket[j].GetComponent<SelectorObject>().updateSelectionType(setting, true);
                    spaces.RemoveAt(i);
                    i--;
                    temp.Add(bucket[j]);
                    bucket.RemoveAt(j);
                    j = bucket.Count;
                }
            }
        }
        if (dump == null) RemoveSpaces(bucket);
        else dump.AddRange(bucket);
        bucket.Clear();
        bucket.AddRange(temp);

        return spaces;
    }


    void CreateSelectors (List<Vector3> spaces, List<GameObject> bucket, bool setting)
    {
        GameObject selector;

        //create primary selectors
        for (int i = 0; i<spaces.Count; i++)
        {
            selector = Instantiate(selectors,transform.position + spaces[i], Quaternion.identity);
            selector.GetComponent<SelectorObject>().updateSelectionType(setting, true);
            //selector.transform.parent = gameObject.transform;
            //selector.GetComponent<SelectorObject>().updateSelectionType(false, true);
            bucket.Add(selector);
        }

        //create border selectors
    }

    List<Vector3> CheckSpaces (List<Vector3> spaces)
    {
        List<Vector3> errorList = new List<Vector3>();
        RaycastHit hit;

        for (int i = 0; i < spaces.Count; i++)
        {
            //FUTURE WARNING! This is testing local position plus the clamped values. Possible problem with this not detecting in technically the right places so keep an eye on this
            if (Physics.Raycast(transform.position + spaces[i] + new Vector3(0f, 6f, 0f), Vector3.down*6, out hit))
            {
                //Debug.LogWarning("Warning when placing objects: " + hit.transform.name + " may be in the way");
                if (hit.transform.gameObject.tag != "Terrain")
                {
                    Debug.LogError("OH FUCK! Yeah that " + hit.transform.gameObject.name + " is in the way");
                    errorList.Add(spaces[i]);
                }
                
            }
        }

        return errorList;
    }

    void RemoveSpaces(List<GameObject> oldSpaces)
    {
        //Debug.LogWarning("Now deleting " + oldSpaces.Count + " spaces...");
        for (int i = 0; i < oldSpaces.Count; i++)
        {
            oldSpaces[i].GetComponent<SelectorObject>().Terminate();
            //primarySelectors.Remove(oldSpaces[oldSpaces.Count - 1]);
            
            //oldSpaces.RemoveAt(oldSpaces.Count - 1);
            
        }
    }

    public void EraseTarget()
    {
        

        for (int i = 0; i < primarySelectors.Count; i++)
        {
            Destroy(primarySelectors[0]);
            primarySelectors.RemoveAt(0);
            i--;
        }

        for (int i = 0; i < perimiterSelectors.Count; i++)
        {
            Destroy(perimiterSelectors[0]);
            perimiterSelectors.RemoveAt(0);
            i--;
        }

        //Destroy(gameObject);
    }
    #endregion

    public bool itemUpdate ()
    {
        GameObject selector;
        RaycastHit hit;

        if (primarySelectors.Count > 0)
        {
            EraseTarget();
        }

        if (Physics.Raycast(transform.position + new Vector3(0f, 6.5f, 0f), transform.up*-1, out hit, 7f))
        {
            if (hit.transform.tag == "Terrain")
            {
                return true;
               //Debug.Log("only hittin floor at position " + hit.point);
            }
            else
            {
                Debug.Log("welp I hit a thing! It's called " + hit.transform.name);
                if (hit.transform.gameObject.GetComponent<iContainer>() != null)
                {
                    selector = Instantiate(selectors, transform.position, Quaternion.identity);
                    selector.GetComponent<SelectorObject>().UpdateItemSelection(hit.point.y, true);
                    primarySelectors.Add(selector);
                    return true;
                }
                 else return false;
            }
        }
        return false;

    }

    [Button("TestRotate")]
    public Vector3 test(Vector3 input, float rotation)
    {
        Vector3 temp = new Vector3(input.x, input.y, input.z);

        temp = Quaternion.Euler(0, rotation, 0) * temp;

        temp = new Vector3(Mathf.Round(temp.x), Mathf.Round(temp.y), Mathf.Round(temp.z));
        return temp;
    }
}
