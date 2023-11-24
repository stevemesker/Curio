using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[SelectionBase]

public class WallObject : MonoBehaviour, iHologram
{
    [Header("Data")]
    [Tooltip("Wall Length")]
    public int wallSize;

    [Tooltip("Data for each section of the wall")]
    [SerializeField] public List<TimelineSegment> WallSequence;

    [Header("Attachments")]
    public GameObject wallContainer;
    public GameObject pillarContainer;
    public GameObject detailContainer;

    [Header("Content")]
    [Tooltip("pillar that is instantiated along the wall")]
    public GameObject PillarPrefab;
    public GameObject WallPrefab;
    public SO_Wall defaultArt;


    //private variables
    int pillarCount; //stores what the previous wall size was so we don't have to reset every time it updates
    string currentRenderLayer; //used to remember what layer the wall is currently on for placement

    

    [Button("Update Wall")]
    public void temp()
    {
        UpdateWall(true);
    }

    

    public void UpdateWall(bool neighborUpdate)
    {
        //script that runs when wall is modified
        //neighbor update setting to true means that all walls that intersect with this will also update. False means it won't and only this specific wall will update

        if (wallSize <= 1) { Debug.LogError("Error: Wall too short!"); return; }
        WallSequence = DetectIntersections(); //create the wall sequence timeline that controls this

        //sets wall length
        //this will change when we start segmenting walls
        GameObject temp = wallContainer.transform.GetChild(0).gameObject;
        if (temp.transform.childCount == 0) 
        {
            GameObject wallArt = Instantiate(defaultArt.wallRunnerArt, transform.position, transform.rotation, temp.transform);
        }
        
        temp.transform.localScale = new Vector3(temp.transform.localScale.x, temp.transform.localScale.y, wallSize);
        temp.GetComponent<Collider>().enabled = true;
        ///end of temporary wall stuff

        int counter = 1;
        pillarCount = 0;

        if (pillarContainer.transform.childCount == 0) 
        {
            temp = Instantiate(PillarPrefab);
            temp.GetComponent<WallComponent>().topLevel = gameObject;
            temp.GetComponent<WallComponent>().art = WallSequence[0].SegmentArt.wallPillarArt;
            temp.transform.parent = pillarContainer.transform;
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.Euler(Vector3.zero);
            temp.name = "Pillar Spawned 0";
            WallSequence[0].FillPillars.Add(temp);
        }
        else WallSequence[0].FillPillars.Add(pillarContainer.transform.GetChild(0).gameObject);

        for (int i = 0; i < WallSequence.Count-1; i++)
        {
            pillarCount = PillarCount(WallSequence[i+1].StartDistance - WallSequence[i].StartDistance); //defines how many pillers (after the starting pillar) should be in this wall sequence
            counter += 1 + pillarCount; //counts exactly how many pillars are needed so far (including previous sequences)
            //print(counter-pillarCount-1);

            if (counter > pillarContainer.transform.childCount) { SpawnPillars(i, counter - pillarContainer.transform.childCount);} //spawn needed pillars
            for (int j = counter - pillarCount - 1; j < counter-1; j++)
            {
                WallSequence[i].FillPillars.Add(pillarContainer.transform.GetChild(j).gameObject);
            }
            WallSequence[i + 1].FillPillars.Add(pillarContainer.transform.GetChild(counter - 1).gameObject);



            WallSequence[i+1].FillPillars[0].transform.localPosition = new Vector3(0, 0, WallSequence[i+1].StartDistance); //sets the first pillar position

            SetPillarPosition(counter - 2 - pillarCount, counter-1);

            UpdateWallArt(i);

            //update wall intersection pillar types
            if (neighborUpdate)
            {
                DetectPillarType(i);
                for (int j = 0; j < WallSequence[i].WallSegmentNeighbors.Count; j++)
                {
                    WallObject tempWallObjectFunc = WallSequence[i].WallSegmentNeighbors[j].GetComponent<WallObject>();
                    //print("updating neighbor " + WallSequence[i].WallSegmentNeighbors[j].name);
                    tempWallObjectFunc.UpdateWall(false);
                    //(int)Mathf.Round(transform.InverseTransformPoint(temp[i].transform.position).x
                    tempWallObjectFunc.UpdateNeighbors((int)Mathf.Round((WallSequence[i].WallSegmentNeighbors[j].transform.InverseTransformPoint(WallSequence[i].FillPillars[0].transform.position)).z));

                }
            }

            renderChange(currentRenderLayer);
        }

        if (neighborUpdate)
        {
            DetectPillarType(WallSequence.Count - 1);
            //DetectPillarType(0);
            for (int j = 0; j < WallSequence[WallSequence.Count-1].WallSegmentNeighbors.Count; j++)
            {
                WallObject tempWallObjectFunc = WallSequence[WallSequence.Count - 1].WallSegmentNeighbors[j].GetComponent<WallObject>();
                tempWallObjectFunc.UpdateWall(false);
                //tempWallObjectFunc.UpdateNeighbors((int)(WallSequence[WallSequence.Count - 1].WallSegmentNeighbors[j].transform.InverseTransformPoint(WallSequence[WallSequence.Count - 1].FillPillars[0].transform.position)).z);
                tempWallObjectFunc.UpdateNeighbors((int)Mathf.Round((WallSequence[WallSequence.Count - 1].WallSegmentNeighbors[j].transform.InverseTransformPoint(WallSequence[WallSequence.Count - 1].FillPillars[0].transform.position)).z));
            }
        }

        if (counter < pillarContainer.transform.childCount) { Debug.Log("Too many pillars, deleting " + (pillarContainer.transform.childCount - counter)); ResetWall(pillarContainer, pillarContainer.transform.childCount - counter); }
        
    }

    List<TimelineSegment> DetectIntersections()
    {
        //function that tests what walls are intersecting with this wall
        //Debug.Log("DetectIntersections(): Now Detecting wall intersections for " + gameObject.name);

        RaycastHit[] hit; //raycast to detect if any walls are merged with this wall
        List<int> temp = new List<int>(); //holds the distances of the walls that are intersecting
        List<GameObject> hitObjectParent = new List<GameObject>(); //the game object that is intersecting. This is found by testing the pillar/wall segment of the full wall and having it return who its owner is

        List<TimelineSegment> CompiledList = new List<TimelineSegment>(); //final list of wall sequences
        TimelineSegment tempTimeLine = new TimelineSegment(); //used to fill out the wall sequence

        hit = Physics.RaycastAll(transform.position + new Vector3(0,3,0) + (transform.forward * -0.6f), transform.forward, wallSize + 0.6f); //casts ray from right behind the wall to the very end of it
        
        for (int i = 0; i < hit.Length; i++) //go through every object hit in the raycast
        {
            GameObject tempWallParent = hit[i].transform.gameObject.GetComponent<iWall>().FindWall(gameObject); //find the parent wall of the wall piece that was hit
            if (hit[i].transform.parent != pillarContainer.transform && hit[i].transform.parent != wallContainer.transform && hitObjectParent.Contains(tempWallParent) == false) //make sure that hit piece isn't part of this object's walls/pillars or that hitObjectParent doesn't already have it (so it is a unique wall)
            {
                if (hit[i].distance - ((int)Mathf.Abs(hit[i].distance)) == .5) hit[i].distance += .1f;  //unity's rounding is a fucking joke so I have to fix it
                
                hitObjectParent.Add(tempWallParent); //adds the new wall
                //temp.Add((int)Mathf.Round(hit[i].distance)); //finds the intersecting object's distance from the origin
                temp.Add((int)Mathf.Round(transform.InverseTransformPoint(hit[i].point).z)); //finds the intersecting object's distance from the origin
                if (temp[temp.Count - 1] < 0) temp[temp.Count - 1] = 0;
            }
        }

        //Debug.Log("DetectIntersections(): Detected " + hitObjectParent.Count + " walls intersecting this one");
        //group up neighbors with their distances
        
        //solo wall
        if (hitObjectParent.Count == 0)
        {
            print("DetectIntersections(): no collision found, raw wall spawn");
            tempTimeLine = new TimelineSegment();
            tempTimeLine.StartDistance = 0;
            tempTimeLine.SegmentArt = defaultArt;
            CompiledList.Add(tempTimeLine);

            tempTimeLine = new TimelineSegment();
            tempTimeLine.StartDistance = wallSize;
            tempTimeLine.SegmentArt = defaultArt;
            CompiledList.Add(tempTimeLine);
            return CompiledList;
        }
        //end solo wall

        //create final compiled list
        tempTimeLine = new TimelineSegment();
        tempTimeLine.WallSegmentNeighbors.Add(hitObjectParent[0]);
        tempTimeLine.StartDistance = temp[0];
        tempTimeLine.SegmentArt = defaultArt;
        CompiledList.Add(tempTimeLine); //creates initial list

        for (int i = 1; i < hitObjectParent.Count; i++)
        {
            for (int j = 0; j < CompiledList.Count; j++)
            {
                if (CompiledList[j].StartDistance == temp[i])
                {
                    if (hitObjectParent[i] != null) CompiledList[j].WallSegmentNeighbors.Add(hitObjectParent[i]);
                    j = CompiledList.Count;
                }
                else if ( j == CompiledList.Count-1)
                {
                    tempTimeLine = new TimelineSegment();
                    if (hitObjectParent[i] != null) tempTimeLine.WallSegmentNeighbors.Add(hitObjectParent[i]);
                    tempTimeLine.StartDistance = temp[i];
                    tempTimeLine.SegmentArt = defaultArt;
                    CompiledList.Add(tempTimeLine);
                    j++;
                }
            }
        }
        //end list compile

        CompiledList.Sort(SortFunc); //sort list
        
        //fill list if there isn't a start/end cap
        if (CompiledList[0].StartDistance > 0)
        {
            tempTimeLine = new TimelineSegment();
            tempTimeLine.StartDistance = 0;
            tempTimeLine.SegmentArt = defaultArt;
            CompiledList.Insert(0, tempTimeLine);
        }
        if (CompiledList[CompiledList.Count-1].StartDistance < wallSize)
        {
            tempTimeLine = new TimelineSegment();
            tempTimeLine.StartDistance = wallSize;
            tempTimeLine.SegmentArt = defaultArt;
            CompiledList.Insert(CompiledList.Count, tempTimeLine);
        }
        
        //Debug.Log("DetectIntersections(): Wall is made of " + CompiledList.Count + " segments");
        return CompiledList; //return the compiled list
    }
    private int SortFunc(TimelineSegment a, TimelineSegment b)
    {
        if (a.StartDistance < b.StartDistance)
        {
            return -1;
        }
        if (a.StartDistance > b.StartDistance)
        {
            return 1;
        }
        return 0;
    }

    void DetectPillarType(int index)
    {
        if (WallSequence[index].WallSegmentNeighbors.Count == 0) { WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(0, 0); return; }
        //function that goes through wall sequence of index and determines what type of corner it is
        List<GameObject> temp = new List<GameObject>(WallSequence[index].WallSegmentNeighbors); //temporarily holds the list of neighbors, might not need this
        int leftPoint = 0; //used to find the furthest left point in the line
        int lengths = 0; //used to count overall length of walls
        WallObject neighbor;
        GameObject inLine = null; // holds the online object to take it out of the list

        //loop through to analyze neighbor lines
        for (int i = 0; i < temp.Count; i++)
        {
            neighbor = temp[i].GetComponent<WallObject>();

            //figures out if we have a zero x origin
            if ((int)transform.InverseTransformPoint(temp[i].transform.position).x == 0 && (int)transform.InverseTransformPoint(temp[i].transform.position + (temp[i].transform.forward * neighbor.wallSize)).x == 0)
            {
                inLine = temp[i];
                temp.RemoveAt(i);
                i -= 1;
            }
            else
            {
                //figures out farthest left point to calculate crossover
                lengths += neighbor.wallSize;
                if ((int)Mathf.Round(transform.InverseTransformPoint(temp[i].transform.position).x) < leftPoint)
                {
                    leftPoint = (int)Mathf.Round(transform.InverseTransformPoint(temp[i].transform.position).x);
                }
                if ((int)Mathf.Round(transform.InverseTransformPoint(temp[i].transform.position + (temp[i].transform.forward * neighbor.wallSize)).x) < leftPoint )
                {
                    leftPoint = (int)Mathf.Round(transform.InverseTransformPoint(temp[i].transform.position + (temp[i].transform.forward * neighbor.wallSize)).x);
                }
            }
        }

        if (index > 0 && index < WallSequence.Count-1) { WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(0, 0); print("this is in the middle of the wall and thus always has an inline"); inLine = gameObject; }
            if (temp.Count == 0) { WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(0, 0); print("This is an in-line"); return; }
        if (Mathf.Abs(leftPoint) - lengths == 0 || leftPoint == 0)
        {
            //print("this is not a crossover!");
            if (inLine != null)
            {
                //print("which means this is a T intersection!");
                if (leftPoint == 0) WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(2, 90); //print("It faces right!");
                else WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(2, -90); //print("It Faces Left!");
                return;
            }
            else
            {
                //print("which means this is a L intersection!");
                if (leftPoint == 0) WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(1, 90); //print("It faces right!");
                else WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(1, -90); //print("It Faces Left!");
                return;
            }
        }
        else
        {
            //print("this is a cross over!");
            if (inLine != null)
            {
                WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(3, 0); //print("which means this is a X intersection!");
                return;
            }
            else
            {
                WallSequence[index].FillPillars[0].GetComponent<WallComponent>().UpdatePillarType(2, 180); //print("which means this is a T intersection! And it faces left and right!");
                return;
            }
        }

    }

    public void ResetWall(GameObject source, int amount)
    {
        Debug.LogWarning("Resetting wall... There are " + amount + " pillars that must be removed");
        
        for (int i = 0; i < amount; i++)
        {
            DestroyImmediate(source.transform.GetChild(source.transform.childCount-1).gameObject);
        }
    }

    public int PillarCount(int distance)
    {
        //function that returns how many pillars need to be spawned between two points, not counting the posts
        return ((distance - 1) / 5) - (((distance - 1) / 5) % 1);
    }

    public void SpawnPillars(int id, int amount)
    {
        //spawns a number of pillars based on amount input for wall sequence of ID
        //Debug.LogWarning("Not enough pillars... Spawning more");
        GameObject temp;
        for (int i = 0; i < amount-1; i++)
        {
            temp = Instantiate(PillarPrefab);
            temp.GetComponent<WallComponent>().topLevel = gameObject;
            temp.GetComponent<WallComponent>().art = WallSequence[0].SegmentArt.wallPillarArt;
            temp.transform.parent = pillarContainer.transform;
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.Euler(Vector3.zero);
            temp.name = "Pillar Spawned " + (pillarContainer.transform.childCount -1);
            temp.GetComponent<Collider>().enabled = true;
        }
        //make starting pillar for next group
        temp = Instantiate(PillarPrefab);
        temp.GetComponent<WallComponent>().topLevel = gameObject;
        temp.GetComponent<WallComponent>().art = WallSequence[0].SegmentArt.wallPillarArt;
        temp.transform.parent = pillarContainer.transform;
        temp.transform.localPosition = Vector3.zero;
        temp.transform.localRotation = Quaternion.Euler(Vector3.zero);
        temp.name = "Pillar Spawned " + (pillarContainer.transform.childCount - 1);
        temp.GetComponent<Collider>().enabled = true;
    }

    void SetPillarPosition (int positionOne, int positionTwo)
    {
        int temp = PillarCount((int)Mathf.Abs(pillarContainer.transform.GetChild(positionTwo).transform.localPosition.z - pillarContainer.transform.GetChild(positionOne).transform.localPosition.z)); //defines the grouping they're in
        int points = temp - 3 + (((int)Mathf.Abs(pillarContainer.transform.GetChild(positionTwo).transform.localPosition.z - pillarContainer.transform.GetChild(positionOne).transform.localPosition.z) - 1) % -5);
        
        GameObject selector;

        for (int i = 0; i < (int)Mathf.Ceil((float)temp / 2); i++)
        {
            selector = pillarContainer.transform.GetChild(positionOne + i + 1).gameObject;
            selector.transform.localPosition = pillarContainer.transform.GetChild(positionOne + i).localPosition + new Vector3(0, 0, 4);
            if (points < 0)
            {
                selector.transform.localPosition = new Vector3(selector.transform.localPosition.x, selector.transform.localPosition.y, selector.transform.localPosition.z - 1);
                points += 1;
            }

            if (points > 0)
            {
                selector.transform.localPosition += new Vector3(0, 0, 1);
                points -= 1;
            }
        }
        for (int i = 0; i < temp / 2; i++)
        {
            selector = pillarContainer.transform.GetChild(positionTwo - i - 1).gameObject;
            selector.transform.localPosition = pillarContainer.transform.GetChild(positionTwo - i).localPosition - new Vector3(0, 0, 4);

            if (points > 0)
            {
                selector.transform.localPosition -= new Vector3(0, 0, 1);
                points -= 1;
            }
        }
        
    }

    public void UpdateNeighbors(int pillarCoordinates)
    {
        Debug.Log(pillarCoordinates);
        for (int i = 0; i < WallSequence.Count; i++)
        {
            if (WallSequence[i].StartDistance == pillarCoordinates)
            {
                WallSequence[i].FillPillars[0].SetActive(false);
                return;
            }
        }
        
    }

    public void RemoveWall()
    {
        WallSequence[0].FillPillars[0].GetComponent<Collider>().enabled = false;
        WallPrefab.GetComponent<Collider>().enabled = false; //might need to update this later
        //function called when wall is being removed so all neighbor walls become primary walls
        for (int i = 0; i < WallSequence.Count; i++)
        {
            for (int j = 0; j < WallSequence[i].WallSegmentNeighbors.Count; i++)
            {
                WallSequence[i].WallSegmentNeighbors[j].GetComponent<WallObject>().ResetPillars(false);
                WallSequence[i].WallSegmentNeighbors[j].GetComponent<WallObject>().UpdateWall(true);
            }
        }
    }

    [Button("Reset Pillars")]
    public void ResetPillars(bool leavePillars)
    {
        WallSequence = new List<TimelineSegment>();

        if (leavePillars == false)
        {
            print("deleting " + pillarContainer.transform.childCount + " pillars");
            for (int i = 0; i < pillarContainer.transform.childCount; i++)
            {
                DestroyImmediate(pillarContainer.transform.GetChild(0).gameObject);
                i = -1;
            }

            for (int i = 0; i < detailContainer.transform.childCount; i++)
            {
                DestroyImmediate(detailContainer.transform.GetChild(0).gameObject);
                i = -1;
            }
        }
    }

    [Button("Update Art")]
    public void UpdateArt(SO_Wall input)
    {
        //swaps out the art for the wall
        //if (input == defaultArt) return;

        //change art SO
        defaultArt = input;

        //swap wall(this will probably change when I update wall segments
        DestroyImmediate(WallPrefab.transform.GetChild(0).gameObject);
        Instantiate(defaultArt.wallRunnerArt, transform.position, transform.rotation, WallPrefab.transform);

        //remove wall runner detail
        for (int i = 0; i < detailContainer.transform.childCount; i++)
        {
            DestroyImmediate(detailContainer.transform.GetChild(0).gameObject);
            i--;
        }

        print("Swapping the art for " + pillarContainer.transform.childCount + " pillars");
        //swap pillars
        for (int i = 0; i < WallSequence.Count; i++)
        {
            WallSequence[i].SegmentArt = defaultArt;
            for (int j = 0; j < WallSequence[i].FillPillars.Count; j++)
            {
                DestroyImmediate(WallSequence[i].FillPillars[j].transform.GetChild(0).gameObject);
                WallSequence[i].FillPillars[j].GetComponent<WallComponent>().UpdatePillarType(4, WallSequence[i].FillPillars[j].transform.localRotation.y);
                //Instantiate(WallSequence[i].SegmentArt.wallPillarArt[0], WallSequence[i].FillPillars[j].transform.position, WallSequence[i].FillPillars[j].transform.rotation, WallSequence[i].FillPillars[j].transform);
            }
            DetectPillarType(i);
            UpdateWallArt(i);
        }
    }

    public void UpdateWallArt(int sequence)
    {
        TimelineSegment WS = WallSequence[sequence];
        //print("Now duping details for wall sequence " + sequence);
        //update pillars

        //update wall
        //nothing for now

        //add wall details
        if (WS.SegmentArt.wallDupeArt.Count == 0) return; //do nothing if there is no wall dupe art details

        

        if (WS.SegmentArt.wallDupeRandom)
        {
            print("randomize wall art!");

            return;
        }


        //int totals = ((WallSequence[sequence + 1].StartDistance - WS.StartDistance) / WS.SegmentArt.wallDupeDistance) - (((WallSequence[sequence + 1].StartDistance - WS.StartDistance) / WS.SegmentArt.wallDupeDistance) % 1);
        int totals = (wallSize / WS.SegmentArt.wallDupeDistance) - ((wallSize / WS.SegmentArt.wallDupeDistance) % 1)-1;

        int counter = 0;
        GameObject temp;
        //print("Now duping wall detail for a distance of " + (WallSequence[sequence + 1].StartDistance - 1 - WallSequence[sequence].StartDistance));
        //print( "Distance for pillar spawning "+ (WallSequence[sequence + 1].StartDistance - WS.StartDistance));

        //Debug.LogWarning(((WallSequence[sequence + 1].StartDistance - WS.StartDistance) / WS.SegmentArt.wallDupeDistance) - (((WallSequence[sequence + 1].StartDistance - WS.StartDistance) / WS.SegmentArt.wallDupeDistance) % 1));
        for (int i = detailContainer.transform.childCount; i < totals; i++)
        {
            temp = Instantiate(WS.SegmentArt.wallDupeArt[counter], transform.position, transform.rotation, detailContainer.transform);
            //WS.WallSegmentFillers.Add(temp);
            counter += 1;
            if (counter > WS.SegmentArt.wallDupeArt.Count - 1) counter = 0;
        }

        
        for (int i = 0; i <totals; i++)
        {
            detailContainer.transform.GetChild(i).transform.localPosition = new Vector3 (0,0, i* WS.SegmentArt.wallDupeDistance + 1);
        }
        
        if (detailContainer.transform.childCount > totals)
        {
            ResetWall(detailContainer, detailContainer.transform.childCount - totals);
        }
    }

    public void renderChange(string renderLayer)
    {
        //sets the render layer for all art
        /*
        for (int i = 0; i < wallContainer.transform.childCount; i++) wallContainer.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer(renderLayer);
        for (int i = 0; i < pillarContainer.transform.childCount; i++) pillarContainer.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer(renderLayer);
        for (int i = 0; i < detailContainer.transform.childCount; i++) detailContainer.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer(renderLayer);
        */
        currentRenderLayer = renderLayer;
        var children = wallContainer.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            //            Debug.Log(child.name);
            child.gameObject.layer = LayerMask.NameToLayer(renderLayer);
        }

        children = pillarContainer.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            //            Debug.Log(child.name);
            child.gameObject.layer = LayerMask.NameToLayer(renderLayer);
        }

        children = detailContainer.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            //            Debug.Log(child.name);
            child.gameObject.layer = LayerMask.NameToLayer(renderLayer);
        }
    }

}

[System.Serializable]
public class TimelineSegment
{
    public SO_Wall SegmentArt;
    public int StartDistance;
    public int WallArtID;
    //public int WallSegmentSize;
    public List<GameObject> FillPillars = new List<GameObject>();
    public List<GameObject> WallSegmentNeighbors = new List<GameObject>();
    //public List<GameObject> WallSegmentFillers = new List<GameObject>();
    //public GameObject Wall;

}
