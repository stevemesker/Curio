using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloManager : MonoBehaviour
{
    public Material hologram; //holds the hologram material for changing the effect
    public float holoIntensity; //how bright we want the hologram to get
    public float changeSpeed; //how fast the hologram changes
    public float windDownSpeedMultiplyer; //how much faster the wind down is compared to the change speed

    public List<GameObject> targetObject; //all the objects that are turning off the holo effect

    private float currentIntensity = 1;
    private float currentTransparency = 1;

    void Update()
    {
        if (currentIntensity >= holoIntensity)
        {
            if (currentTransparency > 0f)
            {
                currentTransparency = Mathf.Lerp(currentTransparency, -.5f, Time.deltaTime * changeSpeed * windDownSpeedMultiplyer);
                hologram.SetFloat("_globalTransparency", currentTransparency);
            }
            else
            {
                print("hologram end...");
                for (int i = 0; i < targetObject.Count; i++) targetObject[i].GetComponent<iHologram>().renderChange("Default");
                targetObject = new List<GameObject>();
                currentIntensity = 1;
                currentTransparency = 1;
                hologram.SetFloat("_globalTransparency", 1);
                hologram.SetFloat("_globalIntensity", 1);
                this.enabled = false;
            }
        }
        else
        {
            currentIntensity = Mathf.Lerp(currentIntensity, holoIntensity + 1f, Time.deltaTime * changeSpeed);
            hologram.SetFloat("_globalIntensity", currentIntensity);
        }
    }


    private void onEnable()
    {
        currentIntensity = 1;
        currentTransparency = 1;
        hologram.SetFloat("_globalTransparency", 1);
        hologram.SetFloat("_globalIntensity", 1);
    }
}
