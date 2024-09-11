using System;
using UnityEngine;

// Manages the linked object's movement, rotation, and resizing based on input events.

public class PlayerAbilities : MonoBehaviour
{
    private PlayerController playerController;
    public float pushForce = 5.0f;
    public float pullForce = 10.0f;
    public float abilityRange = 10.0f;
    public float moveRange = 20.0f;
    public string interactableTag = "interact"; // The tag for interactable objects
    
    public Transform player;
    private Transform linkedObject;
    private Vector3 linkedOffset;
    private float lnkOffMagnitude;
    public float rotationSpeed = 45f;
    public Vector3[] sizeLevels = { new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), new Vector3(1.2f, 1.2f, 1.2f) };
    private int currentSizeLevel = 1;
    public float lerpSpeed = 10.0f;
    
    private Camera playerCamera;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerCamera = Camera.main;
        player = this.transform;
    }
    
    private void OnEnable()
    {
        InputHandler.OnLinkPerformed += LinkObject;
        InputHandler.OnPushPerformed += PushObject;
        InputHandler.OnPullPerformed += PullObject;
        InputHandler.OnRotateYPerformed += RotateYObject;
        InputHandler.OnRotateZPerformed += RotateZObject;
        InputHandler.OnEnlargePerformed += EnlargeObject;
        InputHandler.OnShrinkPerformed += ShrinkObject;
    }
    private void OnDisable()
    {
        InputHandler.OnLinkPerformed -= LinkObject;
        InputHandler.OnPushPerformed -= PushObject;
        InputHandler.OnPullPerformed -= PullObject;
        InputHandler.OnRotateYPerformed -= RotateYObject;
        InputHandler.OnRotateZPerformed -= RotateZObject;
        InputHandler.OnEnlargePerformed -= EnlargeObject;
        InputHandler.OnShrinkPerformed -= ShrinkObject;
    }

    private void Update()
    {
        if (linkedObject != null)
        {
            SyncObjectWithPlayer();
        }
    }
    
    // Schlüsselmethoden für linkedObject, verbindert ein Objekt mit Spieler/Staff und appliziert Bewegungen des Spielers auf linkedObject
    public void LinkObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        // Debug.DrawRay(ray.origin, ray.direction, Color.red, 4f);
        
        if (linkedObject != null)
        {
            linkedObject.GetComponent<Rigidbody>().isKinematic = false;
            linkedObject = null;
            currentSizeLevel = 1;
            Debug.Log("Object unlinked.");
            return;
        }
        
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(interactableTag))
            {
                //linkedOffset = hit.transform.position - player.position;
                lnkOffMagnitude = Vector3.Distance(player.position, hit.transform.position);
                
                linkedObject = hit.transform;
                linkedObject.GetComponent<Rigidbody>().isKinematic = true;
                Debug.Log($"Object {hit.collider.name} linked to player.");
            }
        }
    }
    private void SyncObjectWithPlayer()
    {
        Vector3 desiredPosition = player.position + playerCamera.transform.forward * lnkOffMagnitude;
        linkedObject.position = Vector3.Lerp(linkedObject.position, desiredPosition, Time.deltaTime * lerpSpeed);

        // Alte Version
        // Vector3 desiredPosition = transform.position + transform.TransformDirection(linkedOffset); //camera.forward * linkedOffset.magnitude
        // linkedObject.position = desiredPosition; //Vector3.lerp()
        linkedObject.RotateAround(player.position, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime);
        linkedObject.RotateAround(player.position, Vector3.right, Input.GetAxis("Mouse Y") * Time.deltaTime);
        // linkedObject.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }
    
    // Methoden zur Positionsmanipulation von linkedObject - WEITESTGEHEND VOLLSTÄNDIG, NEXT: FEINEINSTELLUNG UND TESTING
    private void PushObject()
    {
        if (linkedObject != null) { lnkOffMagnitude += pushForce; }
        else { ForceRay(1.25f); }  
    }

    private void PullObject()
    {
        if (linkedObject != null) { lnkOffMagnitude -= pushForce; }
        else { ForceRay(-1.25f); }
    }

    public void ForceRay(float modifier)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(interactableTag))
            {
                Vector3 pushDirection = (hit.collider.transform.position - playerCamera.transform.position).normalized;
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(pushDirection * pushForce * modifier, ForceMode.Impulse);
                }
            }
        }
    }

    // Methoden zur Rotation für linkedObject - ERGÄNZEN: RUNENWAHL
    private void RotateYObject()
    {
        if (linkedObject == null) return;
        linkedObject.Rotate(player.up, rotationSpeed);
        NormalizeRotation();
    }
    private void RotateZObject()
    {
        if (linkedObject == null) return;
        linkedObject.Rotate(player.forward, rotationSpeed);
        NormalizeRotation();
    }
    private void NormalizeRotation()
    {
        Vector3 eulerRotation = linkedObject.rotation.eulerAngles;

        eulerRotation.x = Mathf.Round(eulerRotation.x / rotationSpeed) * rotationSpeed;
        eulerRotation.y = Mathf.Round(eulerRotation.y / rotationSpeed) * rotationSpeed;
        eulerRotation.z = Mathf.Round(eulerRotation.z / rotationSpeed) * rotationSpeed;

        linkedObject.rotation = Quaternion.Euler(eulerRotation);
    }
    
    // Methoden zur Größenmanipulation von linkedObject - ERGÄNZEN: Größenmanipulation des Spielers
    public void EnlargeObject()
    {
        if (linkedObject == null) return;
        if (currentSizeLevel < sizeLevels.Length - 1)
        {
            currentSizeLevel++;
            linkedObject.localScale = sizeLevels[currentSizeLevel];
        }
    }
    public void ShrinkObject()
    {
        if (linkedObject == null) return;
        if (currentSizeLevel > 0)
        {
            currentSizeLevel--;
            linkedObject.localScale = sizeLevels[currentSizeLevel];
        }
    }
    
    
    
}
