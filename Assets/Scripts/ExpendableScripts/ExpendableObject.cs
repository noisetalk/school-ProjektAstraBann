using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExpendableObject : MonoBehaviour
{
    private Renderer objectRenderer;
    private Collider objectCollider;
    public abstract void ExecuteEffect(GameObject other);
    public abstract void ExecuteRemovalStrategy();
    
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
    }
    // Wird bei Kollision mit einem anderen Collider aufgerufen und löst Item-Effekt aus
    private void OnTriggerEnter(Collider other) 
    {
        ExecuteEffect(other.gameObject);
        ExecuteRemovalStrategy();
    }
    // Kümmert sich um den Respawn sofern der Timer größer als 0 ist
    protected IEnumerator Respawn(float delay)
    {
        objectRenderer.enabled = false;
        objectCollider.enabled = false;
        yield return new WaitForSeconds(delay);
        objectRenderer.enabled = true;
        objectCollider.enabled = true;
    }
    
}