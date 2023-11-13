using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[SelectionBase]

public class SelectorObject : MonoBehaviour
{
    [Header("Setup")]
    public GameObject borderObject; //keeps track of the border object when selecting/swapping out materials
    public GameObject selectionObject; //keeps track of the selector when selecting/swapping out materials

    public GameObject itemSelectionObject; //keeps track of the selector for items

    [Header("Variables")]
    public float selectionHeight; //how high up the border goes when space is selected
    public float variance; //sometimes lerping doesn't get close enough, use this te determine the safe range
    public float selectionSpeed; //how fast the border moves if selected

    private bool selected; //determines if this is a selected space
    private float selectedHeight;
    public bool isDeleting = false; //triggers when a delete command is given

    void Update()
    {
        borderObject.transform.localPosition = Vector3.Lerp(borderObject.transform.localPosition, new Vector3(0, selectedHeight, 0), Time.deltaTime * selectionSpeed);
        if (selected == true && borderObject.transform.localPosition.y > selectedHeight - variance)
        {
            //if (isDeleting == true) Destroy(gameObject);
            this.enabled = false;
        }

        if (selected == false && borderObject.transform.localPosition.y < selectedHeight + variance)
        {
            if (isDeleting == true) Destroy(gameObject);
            this.enabled = false;
        }
    }

    [Button("Update Space")]
    public void updateSelectionType(bool id, bool type)
    {
        //function tells the object if it's a prime selector or a border selector using id
        //also uses type to determine material that should be used (0 meaning red not allowed, 1 meaning blue free space, and 2 meaning green adding item to bench)

        selectionObject.SetActive(id);
        selected = id;
        this.enabled = true;
        if (id) { selectedHeight = selectionHeight; return; }
        selectedHeight = 0f;
        return;
    }

    [Button("Delete")]
    public void Terminate()
    {
        //function that handles deleting selector
        isDeleting = true;
        selectionSpeed *= 10;
        updateSelectionType(false, true);
        UpdateItemSelection(0, false);
    }

    [Button("Force State")]
    public void ForceState(bool id, bool type)
    {
        selected = id;
        selectionObject.SetActive(id);
        if (id)
        {
            selectedHeight = selectionHeight;
            borderObject.transform.localPosition = new Vector3(borderObject.transform.localPosition.x, selectedHeight, borderObject.transform.localPosition.z);
        }
        else
        {
            selectedHeight = 0f;
            borderObject.transform.localPosition = new Vector3(borderObject.transform.localPosition.x, selectedHeight, borderObject.transform.localPosition.z);
        }
    }

    public void UpdateItemSelection(float height, bool id)
    {
        //function that turns on ui for item selections
        //height determines placement of the plumbob
        //id determines if it's on or off (not sure if I need this)
        //type determines color

        itemSelectionObject.SetActive(id);
        itemSelectionObject.transform.position = itemSelectionObject.transform.position + new Vector3(0, height, 0);
    }
}
