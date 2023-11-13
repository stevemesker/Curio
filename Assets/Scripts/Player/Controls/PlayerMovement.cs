using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    /////////////////////////////////////////////////
    [Header("MOVEMENT CODE")]
    /////////////////////////////////////////////////
    //--

    [SerializeField, Range(0f, 1f)]
    float stickSensitivity; //controls how far the player has to move the sticks before forward movement will occure

    [SerializeField, Range(0f, 1f)]
    float lookSensitivity;

    [SerializeField, Range(0f, 100f)]
    float Speed = 10f;

    [SerializeField, Range(0f, 25f)]
    float lookSpeed = 10f;

    private RaycastHit slopeHit;

    [SerializeField, Range(0f, 50f)]
    float maxSlopeAngle = 30f;

    Rigidbody rb;

    //--

    /////////////////////////////////////////////////
    [Header("Rotation Code")]
    /////////////////////////////////////////////////
    //--
    
    private PlayerInput pInput;
    [SerializeField] private Vector2 lStickInput;

    private Vector3 moveVector; //handles the direction the player should move
    [SerializeField] Vector3 lookVector; //handles the direction the player should be looking at
    [SerializeField] float stickDistance; //holds how far the player has pushed the sticks

    //--

    /////////////////////////////////////////////////
    [Header("Interaction Code")]
    /////////////////////////////////////////////////
    //--
    [SerializeField] float grabDistance = 1; //how far something can be away from the player to grab it
    [SerializeField] public GameObject pCamera; //stores the player's camera
    [SerializeField] RaycastHit interactRay; //used for picking up objects and interacting
    [SerializeField] public GameObject targeter; //used for showing target location to player

    //--

    //private variables
    private Vector3 placerTargetLocation;
    private bool lockPlacer;


    [Header("_______Temp Variables_______")]
    public Vector3 testVector3; //current rotation of the camera
    public bool boop;

    #endregion

    #region Initializing
    private void Awake()
    {
        pInput = new PlayerInput();
        if (pCamera)
        {
            pCamera.GetComponent<cameraScript>().enabled = true;
        }
    }

    private void OnEnable()
    {
        pInput.Enable();

        pInput.Player.Move.performed += MoveInput;
        pInput.Player.Move.canceled += MoveInput;

        pInput.Player.Pickup.performed += GrabInput;
        pInput.Player.Drop.performed += DropInput;

        pInput.Player.Gather.performed += GatherInput;
        pInput.Player.Gather.performed += GatherCancel;

        pInput.Player.Interact.canceled += InteractInput;

        pInput.Player.RotateClockwise.performed += RotateC;
        pInput.Player.RotateCounterClockwise.performed += RotateCC;

        rb = gameObject.GetComponent<Rigidbody>();
    }
    private void OnDisable()
    {
        pInput.Disable();
    }

    #endregion

    #region Movement
    //MOVEMENT CODE

    private void Update()
    {
        if (lStickInput != Vector2.zero)
        {
            stickDistance = Vector2.Distance(Vector2.zero, lStickInput); //grab 2d vector of the sticks/keyboard
            moveVector = facingDirection();
            lookVector = new Vector3(moveVector.x, 0f, moveVector.z); //find the direction the character will be facing

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookVector), Time.fixedDeltaTime * lookSensitivity);

            Vector3 displacement;
            

            if (stickDistance >= lookSensitivity)
            {
                if (OnSlope())
                {
                    //displacement = (moveVector + GetSlopeMoveDirection()).normalized;
                    displacement = GetSlopeMoveDirection().normalized;
                    //displacement = moveVector;
                }
                else
                {
                    displacement = moveVector;
                }

                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(displacement), Time.fixedDeltaTime * lookSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookVector), Time.fixedDeltaTime * lookSpeed);

                //test = displacement;
                
                if (stickDistance >= stickSensitivity)
                {
                    rb.MovePosition(transform.position + (displacement * (Speed * (Mathf.Abs(lStickInput.x) + Mathf.Abs(lStickInput.y))) * Time.deltaTime));
                }

                
            }

            //update targeting
            Vector3 tempPlacer = Player.player.FindSnapPoint();
            if (placerTargetLocation != tempPlacer && Player.player.shop != null && Player.player.HeldObject.Count > 0)
            {
                placerTargetLocation = tempPlacer;
                if (lockPlacer == false)
                {
                    
                    UpdatePlacer(placerTargetLocation);
                    /*if (Player.player.HeldObject[0].GetComponent<iPickUp>().multiplace(placerTargetLocation) == true)
                    {
                        lockPlacer = true;
                    }*/
                }
                else
                {
                    print("Placer is currently locked at " + Player.player.shop.transform.InverseTransformPoint(targeter.transform.position) + " | extending out to " + placerTargetLocation);
                    //need to update the wall stuff here
                    Player.player.HeldObject[0].GetComponent<WallCarryObject>().UpdateWallLength(Player.player.shop.transform.InverseTransformPoint(targeter.transform.position), placerTargetLocation);
                    UpdatePlacer(Player.player.shop.transform.InverseTransformPoint(targeter.transform.position));
                }
                
            }
            //placerTargetLocation = Player.player.FindSnapPoint();

            //Debug.DrawLine(transform.position, moveVector + GetSlopeMoveDirection().normalized + transform.position, Color.blue);
            //Debug.DrawLine(transform.position, GetSlopeMoveDirection().normalized + transform.position, Color.green);
            //Debug.DrawLine(transform.position + transform.forward * grabDistance + transform.up, transform.position + transform.up * -1 + transform.forward, Color.yellow);

        }

        
    }

    public Vector3 facingDirection()
    {
        //find rotation of camera
        float camRot = pCamera.transform.eulerAngles.y;
        //get stick input location
        lStickInput = Vector2.ClampMagnitude(lStickInput, 1f);

        //offset stick location based on camera rotation
        Vector2 adjStick = new Vector2((lStickInput.x * Mathf.Cos(camRot * Mathf.Deg2Rad * -1f)) + (lStickInput.y * Mathf.Sin(camRot * Mathf.Deg2Rad)), (lStickInput.x * Mathf.Sin(camRot * Mathf.Deg2Rad * -1f)) + (lStickInput.y * Mathf.Cos(camRot * Mathf.Deg2Rad)));
        return (new Vector3(adjStick.x, 0f, adjStick.y));
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position + new Vector3(0f, .5f, 0f), Vector3.down, out slopeHit))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(transform.forward, slopeHit.normal).normalized;
    }

    private void UpdatePlacer(Vector3 targetLocation)
    {
        //script that updates target gameobject for placing objects in the shop

        if (!targeter) { Debug.LogWarning("WARNING! No targeting game object found"); return; }

        if (lockPlacer == false) targeter.transform.position = targetLocation + Player.player.shop.gameObject.transform.position;
        if (Player.player.HeldObject[0].GetComponent<iStorable>() == null)
        {
            //print(Player.player.HeldObject[0].GetComponent<iStorable>());
            targeter.GetComponent<TargetObject>().UpdateSelection(Player.player.HeldObject[0].GetComponent<iPickUp>().GroupCoordinates(), Player.player.HeldObject[0].GetComponent<iPickUp>().GroupPerimiterCoordinates());
        }
        else targeter.GetComponent<TargetObject>().itemUpdate();
    }

    #endregion

    #region Interactions
    //INTERACTION CODE\
    void GrabItem()
    {
        //handles picking up game objects if they have the IPickup interface and the player has an empty hand

        //GameObject freeHand = Player.player.FindFreeHand();
        //if (freeHand == null) return;

        GameObject grabbed = itemFinder();
        if (grabbed && grabbed.transform.GetComponent<iPickUp>() != null)
        {
            //Debug.Log("now picking up " + grabbed);
            Player.player.PickUpObject(grabbed.GetComponent<iPickUp>().pickup(gameObject));
            if (Player.player.shop != null) UpdatePlacer(placerTargetLocation);
        }
        
    }
    void DropItem()
    {
        //might need to change how this works if player is in shop

        if (Player.player.HeldObject.Count == 0) return; //no objects being held

        GameObject dropOff = itemFinder(); //scan to see if anything is in front of the player

        //tests to see if adding the item to an object in front of it
        if (dropOff != null) 
        {
            if (dropOff.GetComponent<iContainer>() != null && Player.player.HeldObject[0].GetComponent<iStorable>() != null) //check to see if the seen item is a container and if the held item can be contained
            {
                if (Player.player.HeldObject[0].GetComponent<iStorable>().Storable(dropOff)) //tests to see if the held item can go into the found item's inventory - this is redu
                {
                    Player.player.RemoveInventory(Player.player.HeldObject[0]); //removes the held item from the player's inventory
                    Player.player.resort(); //rearranges items so that the main item is in the main hand
                    return;
                }
            }
            else Debug.LogWarning("WARNING: " + Player.player.HeldObject[0].name + "cannot be added to " + dropOff.name);
            return;
        }

        //empty in front of player. Put that shit on the floor
        if (Player.player.shop == null)
        {
            //handles dropping item in the overworld
            //Debug.Log("Oh look we're outside");
            RaycastHit hit;
            if (Physics.Raycast(transform.position + transform.forward + transform.up, transform.up * -1, out hit, 1.25f))
            {
                if (hit.transform.tag == "Terrain")
                {
                    Player.player.HeldObject[0].GetComponent<iPickUp>().drop(hit.point, Player.player.transform.localRotation);
                    Player.player.RemoveInventory(Player.player.HeldObject[0]);
                    Player.player.resort(); //rearranges items so that the main item is in the main hand
                }
            }
            return;
        }

        //handles dropping the item in shops

        //currently doesn't handle oddly shaped items
        //also doesn't handle multi select objects like walls
        Debug.Log("Oh look we're in a shop " + Player.player.shop);
        Vector3 snappedPosition = Player.player.FindSnapPoint();

        Debug.LogWarning("Placing object in shop coordinate: " + snappedPosition.x + ":" + snappedPosition.z);

        if (Player.player.HeldObject[0].GetComponent<iPickUp>().multiplace(targeter.transform.position) == false)
        {
            
            if (Player.player.HeldObject[0].GetComponent<iPickUp>().drop(snappedPosition, Player.player.transform.localRotation) == true)
            {
                targeter.GetComponent<TargetObject>().EraseTarget();
                Player.player.shop.addItem(Player.player.HeldObject[0], snappedPosition, Player.player.HeldObject[0].transform.rotation);

                Player.player.RemoveInventory(Player.player.HeldObject[0]); //removes the held item from the player's inventory
                Player.player.resort(); //rearranges items so that the main item is in the main hand

                return;
            }

        }
        else
        {
            print("multiplace time!");
            lockPlacer = !lockPlacer;

            if (Player.player.HeldObject[0].GetComponent<iPickUp>().drop(snappedPosition, Player.player.transform.localRotation) == true)
            {
                targeter.GetComponent<TargetObject>().EraseTarget();
                Player.player.RemoveInventory(Player.player.HeldObject[0]); //removes the held item from the player's inventory
                Player.player.resort(); //rearranges items so that the main item is in the main hand
            }
        }
    }

    void GatherItem()
    {
        print("gathering");
        GameObject Gatherable = itemFinder(); //test to see if there's an object in front of the player
        GameObject openHand = Player.player.FindFreeHand();
        if (Gatherable == null || openHand == null) return;
        Debug.Log(Gatherable.name);
        
        Gatherable.GetComponent<iContainer>().RemoveInventory(Player.player.gameObject);
            
    }
    
    void Interact()
    {
        GameObject Interactable = itemFinder();
        if (Interactable == null) return;

        if (Interactable.GetComponent<iInteract>() != null)
        {
            Interactable.GetComponent<iInteract>().Interact(gameObject);
            //Debug.Log("Interacting with " + Interactable.name);
        }
    }

    GameObject itemFinder ()
    {
        //function that returns a game object if it is within range and is an itemObject. Returns null if nothing is found

        RaycastHit hit;
        
        if (Physics.Raycast(transform.position + new Vector3(0f, 1f, 0f), transform.forward * grabDistance, out hit, grabDistance))
        {
            //Debug.Log("found object collider " + hit.transform.name);
            return hit.transform.gameObject;
        }
        return null;
    }

    void RotateObject(int direction)
    {
        if (Player.player.HeldObject.Count == 0)
        {
            GameObject rotator = itemFinder();
            if (rotator == null) return;
            if (rotator.GetComponent<iRotate>() == null) return;
            rotator.GetComponent<iRotate>().Rotate(direction); return;
        }
        if (Player.player.HeldObject[0].GetComponent<iRotate>() == null) return;
        Player.player.HeldObject[0].GetComponent<iRotate>().Rotate(direction);
        if (Player.player.shop != null) UpdatePlacer(placerTargetLocation);
    }

    #endregion

    #region Input
    //INPUT CODE
    public void MoveInput(InputAction.CallbackContext context)
    {
        lStickInput = context.ReadValue<Vector2>();
    }

    public void GrabInput(InputAction.CallbackContext context)
    {
        GrabItem();
    }

    public void DropInput(InputAction.CallbackContext context)
    {
        DropItem();
    }

    public void GatherInput(InputAction.CallbackContext context)
    {
        GatherItem();
    }

    public void GatherCancel(InputAction.CallbackContext context)
    {
        //GatherItem();
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        Interact();
    }

    public void RotateC(InputAction.CallbackContext context)
    {
        RotateObject(1);
    }

    public void RotateCC(InputAction.CallbackContext context)
    {
        RotateObject(-1);
    }

    #endregion
}
