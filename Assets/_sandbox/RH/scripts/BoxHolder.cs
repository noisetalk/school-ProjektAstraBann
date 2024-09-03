using UnityEngine;

public class GrabObject : MonoBehaviour
{
    public float grabForce = 100f;
    public float grabDistance = 5f;
    public LayerMask grabbableLayer;

    private GameObject grabbedObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Grab();
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            Release();
        }
    }

    void Grab()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, grabDistance, grabbableLayer))
        {
            grabbedObject = hit.collider.gameObject;

            // Überprüfen, ob das Objekt bereits gegriffen wurde
            if (grabbedObject.GetComponent<FixedJoint>() == null)
            {
                FixedJoint joint = grabbedObject.AddComponent<FixedJoint>();
                joint.connectedBody = GetComponent<Rigidbody>();
            }
        }
    }

    void Release()
    {
        if (grabbedObject != null)
        {
            Destroy(grabbedObject.GetComponent<FixedJoint>());
            grabbedObject = null;
        }
    }
}