using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CraftingAnimComponent : MonoBehaviour
{
    [SerializeField] public List<AnimComponent> Components;

    private Quaternion startingRotation;

    Quaternion continuousRotation;
    Quaternion oscillatingRotation;

    private void Start()
    {
        startingRotation = transform.rotation;
    }
    private void Update()
    {
        
        float oscillationTime = 0f; // Keep track of the oscillation over time

        foreach (AnimComponent component in Components)
        {
            //print("boop");
            switch (component.animType)
            {
                case AnimType.Rotate:
                    //print("rotate");
                    transform.Rotate(component.axis, component.speed * Time.deltaTime);
                    //continuousRotation = Quaternion.AngleAxis(component.speed * Time.deltaTime, component.axis);
                    break;

                case AnimType.Jitter:
                    //print("Jitter");
                    float oscillationAngle = Mathf.Sin(Time.time * Mathf.PI * Components[0].speed) * Components[0].axis.magnitude;
                    Quaternion oscillationRotation = Quaternion.AngleAxis(oscillationAngle, Components[0].axis.normalized);
                    //print(oscillationRotation);
                    //transform.localRotation += oscillationRotation;

                    // Calculate oscillation over time
                    oscillationTime += Time.deltaTime * component.speed;
                    float currentOscillationAngle = Mathf.Sin(oscillationTime) * component.axis.magnitude;

                    // Calculate the oscillating rotation
                    oscillatingRotation = Quaternion.AngleAxis(currentOscillationAngle, component.axis);

                    break;

                case AnimType.Move:
                    //print("Moving");

                    break;

                default: break;
            }

            
        }
        //transform.rotation = continuousRotation * oscillatingRotation * transform.rotation;
    }


        /*
        //oscilation code
        float oscillationAngle = Mathf.Sin(Time.time * Mathf.PI * Components[0].speed) * Components[0].axis.magnitude;

        Quaternion oscillationRotation = Quaternion.AngleAxis(oscillationAngle, Components[0].axis.normalized);

        //transform.rotation = startingRotation * oscillationRotation;
        */

        //rotation code
        //startingRotation = transform.rotation * Quaternion.Euler( Components[1].axis);
        //transform.Rotate(Components[1].axis, Components[1].speed * Time.deltaTime);
        //print(Components[1].speed);
        
    
}

public enum AnimType 
{ 
    Rotate, Move, Jitter 
}

[System.Serializable]
public class AnimComponent
{
    //[EnumToggleButtons] [Title("Component Type")] public enum AnimType {Rotate, Move, Jitter}
    [EnumToggleButtons] [Title("Component Type")] public AnimType animType;
    public float speed;
    public Vector3 axis;
    [ShowIf("@animType", AnimType.Move)]
    public Vector3 targetPosition;
}
