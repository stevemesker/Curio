using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlacer : MonoBehaviour
{
    public GameObject placer;
    public Vector3 snappedPosition;


    Vector3 previousPosition;
    // Update is called once per frame
    void Update()
    {
        
        snappedPosition = transform.position + (transform.forward/2) + transform.forward;
        snappedPosition.x = Mathf.Round(snappedPosition.x);
        snappedPosition.y = Mathf.Round(snappedPosition.y);
        snappedPosition.z = Mathf.Round(snappedPosition.z);

        Debug.DrawLine(transform.forward + transform.position, transform.forward+transform.position + transform.up, Color.red);
        Debug.DrawLine(snappedPosition, snappedPosition + transform.up, Color.blue);

        if (snappedPosition != previousPosition)
        {
            previousPosition = snappedPosition;
            placer.transform.position = snappedPosition;
        }
    }

    public Vector3 FindSnapPoint ()
    {
        Vector3 snap = transform.localPosition + (transform.forward / 2) + transform.forward;
        snap.x = Mathf.Round(snap.x);
        snap.y = Mathf.Round(snap.y);
        snap.z = Mathf.Round(snap.z);
        return snap;
    }

    public void DisablePointer ()
    {
        //turn off pointer
    }
}
