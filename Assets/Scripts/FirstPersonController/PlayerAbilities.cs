using System;
using UnityEngine;

// Manages the linked object's movement, rotation, and resizing based on input events.

public class PlayerAbilities : MonoBehaviour
{
    private PlayerController playerController;
    public float pushForce = 10f;
    public float pullForce = 10f;
    public float abilityRange = 10f;
    public string interactableTag = "interact"; // The tag for interactable objects
    
    public Transform player;
    private Transform linkedObject;
    private Vector3 linkedOffset;
    public float moveSpeed = 10.0f; // Speed for moving the linked object
    public float rotationSpeed = 45f; // Speed for rotating the object in degrees per second
    public Vector3[] sizeLevels = { new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1f, 1f, 1f), new Vector3(1.5f, 1.5f, 1.5f) };
    private int currentSizeLevel = 1;
    
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
        InputHandler.OnRotateYPerformed += RotateYObject;
        InputHandler.OnRotateZPerformed += RotateZObject;
    }
    
    private void OnDisable()
    {
        InputHandler.OnLinkPerformed -= LinkObject;
        InputHandler.OnRotateYPerformed -= RotateYObject;
        InputHandler.OnRotateZPerformed -= RotateZObject;
    }

    private void Update()
    {
        if (linkedObject != null)
        {
            SyncObjectWithPlayer();
        }
    }
    /*
    void LateUpdate()
    {
        if (linkedObject != null)
        {
            var objectRotation = linkedObject.transform.rotation.eulerAngles;
            linkedObject.transform.rotation = Quaternion.Euler(0, objectRotation.y, 0);
        }
    }
    */
    
    public void LinkObject()
    {
        if (linkedObject != null)
        {
            // Unlink the current object
            linkedObject = null;
            Debug.Log("Object unlinked.");
            return;
        }
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(interactableTag))
            {
                linkedOffset = hit.transform.position - player.position;
                linkedObject = hit.transform;
                Debug.Log($"Object {hit.collider.name} linked to player.");
            }
        }
    }
    private void SyncObjectWithPlayer()
    {
        Vector3 desiredPosition = transform.position + transform.TransformDirection(linkedOffset);
        linkedObject.position = desiredPosition;

        // float mouseY = Input.GetAxis("Mouse Y");
        // linkedObject.position += Vector3.up * mouseY * Time.deltaTime;
        linkedObject.RotateAround(player.position, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime);
        linkedObject.RotateAround(player.position, Vector3.right, Input.GetAxis("Mouse Y") * Time.deltaTime);
        // linkedObject.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        
    }
    
    public void RotateYObject()
    {
        if (linkedObject == null) return;
        linkedObject.Rotate(Vector3.up, rotationSpeed);
    }
    public void RotateZObject()
    {
        if (linkedObject == null) return;
        linkedObject.Rotate(Vector3.forward, rotationSpeed);
    }
    
    /*
    public void ResizeObject()
    {
        if (linkedObject == null) return;
        
        int direction = (int)Mathf.Sign();
        ChangeObjectSize(direction);
    }
    
    public void ChangeObjectSize(int direction)
    {
        currentSizeLevel = Mathf.Clamp(currentSizeLevel + direction, 0, sizeLevels.Length - 1);
        linkedObject.localScale = sizeLevels[currentSizeLevel];
    }
    */
    
}
